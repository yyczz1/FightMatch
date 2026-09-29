using System;
using System.Collections.Generic;
using FightMatch.Input;
using FlowPuzzle.Core;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

namespace FightMatch.Presentation
{
    public sealed class CandidateBoardElement : VisualElement
    {
        private readonly CandidateBoardInputController controller;
        private Rect board;
        private Rect previousContent;
        private Matrix4x4 previousTransform;
        private int width, height;
        private float cell, panelScale;
        private bool geometryKnown, geometryValid, listening;
        private int? captured;
        private bool detaching;

        public CandidateBattlePlaybackFrame PlaybackOverride { get; private set; }
        public CandidateBattlePlaybackFrame ReferenceOverride { get; private set; }
        public void SetPlaybackOverride(CandidateBattlePlaybackFrame frame)
        { PlaybackOverride = frame ?? throw new ArgumentNullException(nameof(frame)); MarkDirtyRepaint(); }
        public void ClearPlaybackOverride() { PlaybackOverride = null; MarkDirtyRepaint(); }
        public bool SetReferenceOverride(CandidateBattlePlaybackFrame frame)
        {
            var face = controller.View.BattleSnapshot?.Board.Face;
            if (frame?.Face == null || face == null || PlaybackOverride != null || controller.View.PresentationToken != null ||
                face.FaceId != frame.Face.FaceId || face.Width != frame.Face.Width || face.Height != frame.Face.Height) return false;
            ReferenceOverride = frame; MarkDirtyRepaint(); return true;
        }
        public void ClearReferenceOverride() { ReferenceOverride = null; MarkDirtyRepaint(); }

        public CandidateBoardElement(CandidateBoardInputController controller, bool borrowed = false)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
            name = "candidate-board"; focusable = true;
            style.flexGrow = 1; style.minHeight = 200;
            style.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            RegisterCallback<PointerDownEvent>(OnDown);
            RegisterCallback<PointerMoveEvent>(OnMove);
            RegisterCallback<PointerUpEvent>(OnUp);
            RegisterCallback<PointerCancelEvent>(e => { if (controller.Gesture.ActivePointerId == e.pointerId) controller.CancelGesture(); });
            RegisterCallback<PointerCaptureOutEvent>(e => {
                if (!detaching && controller.Gesture.ActivePointerId == e.pointerId && !this.HasPointerCapture(e.pointerId)) controller.CancelGesture();
            });
            RegisterCallback<BlurEvent>(e => controller.CancelGesture());
            RegisterCallback<GeometryChangedEvent>(e => { geometryKnown = false; controller.CancelGesture(); MarkDirtyRepaint(); });
            RegisterCallback<AttachToPanelEvent>(e => { if (!listening) { controller.Changed += OnChanged; listening = true; } OnChanged(); });
            RegisterCallback<DetachFromPanelEvent>(e => {
                detaching = borrowed;
                if (!borrowed) controller.CancelGesture(); Release(); ClearReferenceOverride();
                if (listening) { controller.Changed -= OnChanged; listening = false; }
                geometryKnown = false;
                detaching = false;
            });
            generateVisualContent += Draw;
        }

        private void OnChanged()
        {
            if (controller.Gesture.Stage == GestureStage.Dragging) ClearReferenceOverride();
            if (captured.HasValue && controller.Gesture.ActivePointerId != captured) Release();
            MarkDirtyRepaint();
        }

        private void Release()
        {
            var id = captured; captured = null;
            if (id.HasValue && this.HasPointerCapture(id.Value)) this.ReleasePointer(id.Value);
        }

        private bool Geometry()
        {
            var face = controller.View.BattleSnapshot?.Board.Face;
            var w = face?.Width ?? 0; var h = face?.Height ?? 0;
            var rect = contentRect; var transform = worldTransform;
            var changed = !geometryKnown || rect != previousContent || transform != previousTransform || w != width || h != height;
            if (!changed) return geometryValid;
            geometryKnown = true; previousContent = rect; previousTransform = transform; width = w; height = h;
            geometryValid = false;
            controller.CancelGesture();
            if (panel == null || w <= 0 || h <= 0 || !Finite(rect.width) || !Finite(rect.height) || rect.width <= 0 || rect.height <= 0) return false;
            // Only an axis-aligned, positive uniform transform preserves square cells and panel-point thresholds.
            panelScale = transform.m00;
            if (!Finite(panelScale) || panelScale <= 0 || !Finite(transform.m11) ||
                Mathf.Abs(transform.m00 - transform.m11) > 0.0001f * panelScale ||
                Mathf.Abs(transform.m01) > 0.0001f || Mathf.Abs(transform.m10) > 0.0001f) return false;
            cell = Mathf.Min(rect.width / w, rect.height / h);
            board = new Rect(rect.x + (rect.width - cell * w) / 2, rect.y + (rect.height - cell * h) / 2, cell * w, cell * h);
            geometryValid = Finite(cell) && cell > 0;
            return geometryValid;
        }

        private static bool Finite(float n) { return !float.IsNaN(n) && !float.IsInfinity(n); }

        private bool Sample(IPointerEvent e, out PointerSample sample)
        {
            sample = default;
            if (!Geometry() || !Finite(e.position.x) || !Finite(e.position.y)) { controller.CancelGesture(); return false; }
            var local = this.WorldToLocal(e.position);
            var x = local.x - board.xMin; var y = board.yMax - local.y;
            FlowPos? hit = null;
            if (x >= 0 && y >= 0 && x < board.width && y < board.height)
                hit = new FlowPos((int)Math.Floor(x / cell), (int)Math.Floor(y / cell));
            // Absolute panel coordinates are used only for gesture distance, never as domain cells.
            sample = new PointerSample(e.pointerId, e.position.x, e.position.y, hit);
            return true;
        }

        private void OnDown(PointerDownEvent e)
        {
            if (e.button != 0 && e.pointerType != PointerType.touch) return;
            controller.Refresh();
            if (!Sample(e, out var sample)) return;
            var points = cell * panelScale;
            var threshold = Mathf.Clamp(e.pointerType == PointerType.touch ? 10 : 6, points * 0.15f, points * 0.25f);
            controller.Down(sample, threshold);
            if (controller.Gesture.ActivePointerId != e.pointerId) return;
            captured = e.pointerId; this.CapturePointer(e.pointerId); Focus(); e.StopPropagation();
        }

        private void OnMove(PointerMoveEvent e)
        {
            if (!Sample(e, out var sample)) return;
            controller.Move(sample);
            if (controller.Gesture.ActivePointerId == e.pointerId) e.StopPropagation();
        }

        private void OnUp(PointerUpEvent e)
        {
            var wasActive = controller.Gesture.ActivePointerId == e.pointerId;
            if (Sample(e, out var sample)) controller.Up(sample);
            if (wasActive) e.StopPropagation();
            OnChanged();
        }

        private Vector2 Center(FlowPos p)
        { return new Vector2(board.xMin + (p.x + 0.5f) * cell, board.yMax - (p.y + 0.5f) * cell); }

        private void Line(Painter2D painter, IReadOnlyList<FlowPos> route, Color color, float thickness)
        {
            if (route.Count < 2) return;
            painter.strokeColor = color; painter.lineWidth = thickness; painter.BeginPath(); painter.MoveTo(Center(route[0]));
            for (var i = 1; i < route.Count; i++) painter.LineTo(Center(route[i]));
            painter.Stroke();
        }

        private void Draw(MeshGenerationContext context)
        {
            if (!Geometry()) return;
            var state = controller.View.BattleSnapshot; var painter = context.painter2D;
            var overlay = PlaybackOverride ?? ReferenceOverride;
            var face = overlay?.Face ?? state.Board.Face;
            var lockedRoutes = overlay?.LockedRoutes ?? state.Board.LockedRoutes;
            var pendingLinks = overlay?.PendingLinks ?? state.Board.PendingLinks;
            painter.lineWidth = 1; painter.strokeColor = new Color(0.3f, 0.32f, 0.38f); painter.BeginPath();
            for (var x = 0; x <= width; x++) { painter.MoveTo(new Vector2(board.xMin + x * cell, board.yMin)); painter.LineTo(new Vector2(board.xMin + x * cell, board.yMax)); }
            for (var y = 0; y <= height; y++) { painter.MoveTo(new Vector2(board.xMin, board.yMin + y * cell)); painter.LineTo(new Vector2(board.xMax, board.yMin + y * cell)); }
            painter.Stroke();
            foreach (var pair in face.Pairs)
            {
                var color = Color.HSVToRGB((pair.GeometryColorId % 12) / 12f, 0.65f, 1);
                foreach (var route in lockedRoutes)
                    if (route.PairKey.PairId == pair.PairId) Line(painter, route.Route, color, cell * 0.12f);
                var pending = false;
                foreach (var key in pendingLinks) if (key.PairId == pair.PairId) pending = true;
                foreach (var end in new[] { pair.EndpointA, pair.EndpointB })
                {
                    painter.fillColor = color; painter.strokeColor = color; painter.lineWidth = 2; painter.BeginPath();
                    painter.Arc(Center(end), cell * 0.2f, new Angle(0), new Angle(360)); painter.ClosePath();
                    if (pending) painter.Stroke(); else painter.Fill();
                }
            }
            var preview = controller.Gesture;
            Line(painter, overlay?.TemporaryRoute ?? preview.DraftCells, overlay == null && preview.HasInvalidSample ? Color.red : Color.white, cell * 0.08f);
        }
    }
}
