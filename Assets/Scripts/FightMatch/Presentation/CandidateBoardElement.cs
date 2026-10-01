using System;
using System.Collections.Generic;
using FightMatch.Input;
using FlowPuzzle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FightMatch.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CandidateBoardElement : UnityEngine.UI.MaskableGraphic,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler,
        IPointerUpHandler, IEndDragHandler, IPointerExitHandler
    {
        private CandidateBoardInputController controller;
        private Rect board;
        private Matrix4x4 previousTransform;
        private int width, height;
        private float cell;
        private bool geometryKnown, geometryValid, finished = true, populating;
        private int? captured;
        private long generation;
        private SafeAreaFitter safeArea;
        public CandidateBattlePlaybackFrame PlaybackOverride { get; private set; }
        public CandidateBattlePlaybackFrame ReferenceOverride { get; private set; }
        internal bool HasActivePointer => captured.HasValue;

        internal void Bind(CandidateBoardInputController owner)
        {
            Unbind(); controller = owner ?? throw new ArgumentNullException(nameof(owner));
            controller.Changed += OnChanged;
            safeArea = GetComponentInParent<SafeAreaFitter>();
            if (safeArea != null) safeArea.Changed += InvalidateGeometry;
            raycastTarget = true; geometryKnown = false; OnChanged();
        }
        internal void Unbind() => Unbind(PointerCancellationCause.Cancelled);
        internal void Unbind(PointerCancellationCause cause)
        {
            CancelPointer(cause);
            if (controller != null) controller.Changed -= OnChanged;
            if (safeArea != null) safeArea.Changed -= InvalidateGeometry;
            controller = null; safeArea = null;
            PlaybackOverride = null; ReferenceOverride = null; geometryKnown = false;
            SetVerticesDirty();
        }
        public void SetPlaybackOverride(CandidateBattlePlaybackFrame frame)
        { PlaybackOverride = frame ?? throw new ArgumentNullException(nameof(frame)); SetVerticesDirty(); }
        public void ClearPlaybackOverride() { PlaybackOverride = null; SetVerticesDirty(); }
        public bool SetReferenceOverride(CandidateBattlePlaybackFrame frame)
        {
            var face = controller?.View.BattleSnapshot?.Board.Face;
            if (frame?.Face == null || face == null || PlaybackOverride != null || controller.View.PresentationToken != null ||
                face.FaceId != frame.Face.FaceId || face.Width != frame.Face.Width || face.Height != frame.Face.Height) return false;
            ReferenceOverride = frame; SetVerticesDirty(); return true;
        }
        public void ClearReferenceOverride() { ReferenceOverride = null; SetVerticesDirty(); }
        private void OnChanged()
        {
            if (controller?.Gesture.Stage == GestureStage.Dragging) ReferenceOverride = null;
            if (captured.HasValue && controller?.Gesture.ActivePointerId != captured)
            { captured = null; finished = true; generation++; }
            if (!populating) SetVerticesDirty();
        }
        internal void CancelPointer() => CancelPointer(PointerCancellationCause.Cancelled);
        internal void CancelPointer(PointerCancellationCause cause)
        {
            var active = captured.HasValue || controller?.Gesture.ActivePointerId != null;
            captured = null; finished = true; generation++;
            if (active) controller?.CancelGesture(cause);
        }
        internal void CancelTrueTouch(int pointerId, Vector2 screenPosition)
        { if (captured == pointerId) CancelPointer(); }
        private void InvalidateGeometry()
        { geometryKnown = false; CancelPointer(); SetVerticesDirty(); }
        private bool Geometry()
        {
            var face = controller?.View.BattleSnapshot?.Board.Face;
            var w = face?.Width ?? 0; var h = face?.Height ?? 0;
            var rect = rectTransform.rect; var matrix = rectTransform.localToWorldMatrix;
            if (geometryKnown && rect == board && matrix == previousTransform && w == width && h == height) return geometryValid;
            CancelPointer();
            geometryKnown = true; board = rect; previousTransform = matrix; width = w; height = h; geometryValid = false;
            if (w <= 0 || h <= 0 || !Finite(rect.width) || !Finite(rect.height) || rect.width <= 0 || rect.height <= 0) return false;
            var scale = matrix.m00;
            if (!Finite(scale) || scale <= 0 || !Finite(matrix.m11) ||
                Mathf.Abs(scale - matrix.m11) > .0001f * scale || Mathf.Abs(matrix.m01) > .0001f || Mathf.Abs(matrix.m10) > .0001f) return false;
            cell = Mathf.Min(rect.width / w, rect.height / h);
            geometryValid = Finite(cell) && cell > 0; return geometryValid;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        internal FlowPos? CellAtLocal(Vector2 local)
        {
            if (!Geometry() || !Finite(local.x) || !Finite(local.y) ||
                local.x < board.xMin || local.y < board.yMin || local.x >= board.xMax || local.y >= board.yMax) return null;
            var x = Mathf.FloorToInt((local.x - board.xMin) / cell);
            var y = Mathf.FloorToInt((local.y - board.yMin) / cell);
            return x >= 0 && y >= 0 && x < width && y < height ? new FlowPos(x, y) : (FlowPos?)null;
        }
        internal Vector2 CellCenter(FlowPos position)
        {
            if (!Geometry()) throw new InvalidOperationException("Board geometry is unavailable.");
            return new Vector2(board.xMin + (position.x + .5f) * cell, board.yMin + (position.y + .5f) * cell);
        }
        private bool Sample(PointerEventData data, out PointerSample sample)
        {
            sample = default;
            if (!Geometry() || !Finite(data.position.x) || !Finite(data.position.y) ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var local))
            { CancelPointer(); return false; }
            sample = new PointerSample(data.pointerId, local.x, local.y, CellAtLocal(local)); return true;
        }
        public void OnInitializePotentialDrag(PointerEventData data) { data.useDragThreshold = false; }
        public void OnPointerDown(PointerEventData data)
        {
            if (controller == null || data.pointerId < 0 && data.button != PointerEventData.InputButton.Left) return;
            if (captured.HasValue)
            {
                if (captured != data.pointerId) controller.PublishFeedback(GestureFeedbackKind.SecondPointerIgnored);
                return;
            }
            controller.Refresh();
            if (!Sample(data, out var sample)) return;
            var threshold = Mathf.Clamp(data.pointerId >= 0 ? 10 : 6, cell * .15f, cell * .25f);
            controller.Down(sample, threshold);
            if (controller.Gesture.ActivePointerId != data.pointerId) return;
            captured = data.pointerId; finished = false; generation++;
        }
        public void OnDrag(PointerEventData data)
        {
            if (captured != data.pointerId || finished) return;
            var current = generation;
            if (Sample(data, out var sample) && current == generation && captured == data.pointerId) controller.Move(sample);
        }
        public void OnPointerUp(PointerEventData data) { FinishPointer(data); }
        public void OnEndDrag(PointerEventData data) { FinishPointer(data); }
        public void OnPointerExit(PointerEventData data) { if (captured == data.pointerId) CancelPointer(); }
        private void FinishPointer(PointerEventData data)
        {
            if (captured != data.pointerId || finished) return;
            var current = generation;
            if (!Sample(data, out var sample) || current != generation || captured != data.pointerId) return;
            finished = true; captured = null; generation++; controller.Up(sample);
        }
        protected override void OnRectTransformDimensionsChange()
        { base.OnRectTransformDimensionsChange(); InvalidateGeometry(); }
        protected override void OnDisable() { CancelPointer(PointerCancellationCause.FocusLost); base.OnDisable(); }
        protected override void OnDestroy() { Unbind(); base.OnDestroy(); }
        private void OnApplicationFocus(bool focused) { if (!focused) CancelPointer(PointerCancellationCause.FocusLost); }
        private void OnApplicationPause(bool paused) { if (paused) CancelPointer(PointerCancellationCause.FocusLost); }

        private static void Quad(UnityEngine.UI.VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            var start = mesh.currentVertCount;
            mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero);
            mesh.AddVert(c, tint, Vector2.zero); mesh.AddVert(d, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start + 2, start + 3, start);
        }
        private static void Segment(UnityEngine.UI.VertexHelper mesh, Vector2 a, Vector2 b, Color tint, float thickness)
        {
            if (a == b) return;
            var delta = b - a; var offset = new Vector2(-delta.y, delta.x).normalized * (thickness * .5f);
            Quad(mesh, a - offset, a + offset, b + offset, b - offset, tint);
        }
        private void Route(UnityEngine.UI.VertexHelper mesh, IReadOnlyList<FlowPos> route, Color tint, float thickness)
        { for (var i = 1; i < route.Count; i++) Segment(mesh, CellCenter(route[i - 1]), CellCenter(route[i]), tint, thickness); }
        private void Endpoint(UnityEngine.UI.VertexHelper mesh, Vector2 center, Color tint, bool hollow)
        {
            const int sides = 20;
            for (var i = 0; i < sides; i++)
            {
                var a = i * Mathf.PI * 2 / sides; var b = (i + 1) * Mathf.PI * 2 / sides;
                var p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * cell * .2f;
                var q = center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * cell * .2f;
                if (hollow) Segment(mesh, p, q, tint, 2);
                else
                {
                    var start = mesh.currentVertCount;
                    mesh.AddVert(center, tint, Vector2.zero); mesh.AddVert(p, tint, Vector2.zero); mesh.AddVert(q, tint, Vector2.zero);
                    mesh.AddTriangle(start, start + 1, start + 2);
                }
            }
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear(); populating = true;
            try
            {
                if (!Geometry()) return;
                Quad(mesh, new Vector2(board.xMin, board.yMin), new Vector2(board.xMin, board.yMax),
                    new Vector2(board.xMax, board.yMax), new Vector2(board.xMax, board.yMin), new Color(.08f, .09f, .12f));
                var grid = new Color(.3f, .32f, .38f);
                for (var x = 0; x <= width; x++) Segment(mesh, new Vector2(board.xMin + x * cell, board.yMin), new Vector2(board.xMin + x * cell, board.yMin + height * cell), grid, 1);
                for (var y = 0; y <= height; y++) Segment(mesh, new Vector2(board.xMin, board.yMin + y * cell), new Vector2(board.xMin + width * cell, board.yMin + y * cell), grid, 1);
                var state = controller.View.BattleSnapshot;
                var overlay = PlaybackOverride ?? ReferenceOverride;
                var face = overlay?.Face ?? state.Board.Face;
                var locked = overlay?.LockedRoutes ?? state.Board.LockedRoutes;
                var pending = overlay?.PendingLinks ?? state.Board.PendingLinks;
                foreach (var pair in face.Pairs)
                {
                    var tint = Color.HSVToRGB((pair.GeometryColorId % 12) / 12f, .65f, 1);
                    foreach (var route in locked) if (route.PairKey.PairId == pair.PairId) Route(mesh, route.Route, tint, cell * .12f);
                    var hollow = false;
                    foreach (var key in pending) if (key.PairId == pair.PairId) hollow = true;
                    Endpoint(mesh, CellCenter(pair.EndpointA), tint, hollow); Endpoint(mesh, CellCenter(pair.EndpointB), tint, hollow);
                }
                Route(mesh, overlay?.TemporaryRoute ?? controller.Gesture.DraftCells,
                    overlay == null && controller.Gesture.HasInvalidSample ? Color.red : Color.white, cell * .08f);
            }
            finally { populating = false; }
        }
    }
}
