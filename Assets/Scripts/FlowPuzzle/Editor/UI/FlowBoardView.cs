using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Draft;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    internal static class FlowEditorColorPalette
    {
        private static readonly Color[] Colors =
        {
            Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan,
            new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f), new Color(0f, 0.7f, 0.7f),
            new Color(1f, 0.4f, 0.7f), new Color(0.5f, 1f, 0f), new Color(0.7f, 0.3f, 0f)
        };

        private static readonly string[] Names =
        {
            "Red", "Blue", "Green", "Yellow", "Magenta", "Cyan",
            "Orange", "Purple", "Teal", "Pink", "Lime", "Brown"
        };

        internal static Color GetColor(int colorId) => Colors[colorId % Colors.Length];

        internal static string GetDisplayName(int colorId)
        {
            var name = Names[colorId % Names.Length];
            return colorId < Names.Length ? name : $"{name} ({colorId + 1})";
        }
    }

    public sealed class FlowBoardView : VisualElement
    {
        public event Action<FlowPos> CellHovered;
        public event Action<FlowPos> CellSelected;
        public event Action<IReadOnlyList<FlowPos>> CellStrokeCompleted;

        private FlowLevelData levelData;
        private FlowSolutionData solutionData;
        private List<FlowDraftConstraintData> constraintData;
        private List<FlowDraftPairData> draftEndpointData;

        private bool isStrokeActive;
        private FlowPos? lastStrokeCell;
        private readonly List<FlowPos> strokeCells = new List<FlowPos>();

        public FlowBoardView()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        public void SetData(FlowLevelData level, FlowSolutionData solution) { levelData = level != null ? DeepCopyLevel(level) : null; solutionData = solution != null ? DeepCopySolution(solution) : null; constraintData = null; draftEndpointData = null; MarkDirtyRepaint(); }
        public void SetData(FlowLevelDraft draft)
        {
            if (draft == null) { ClearData(); return; }
            var dl = new FlowLevelData { levelId = draft.levelId, width = draft.width, height = draft.height };
            levelData = DeepCopyLevel(dl);
            solutionData = draft.currentSolution != null && !draft.isSolutionDirty && draft.isValidated
                ? DeepCopySolution(draft.currentSolution)
                : null;
            constraintData = draft.fixedConstraints?.Select(c => c.Clone()).ToList();
            draftEndpointData = draft.pairs?.Select(p => p.Clone()).ToList();
            MarkDirtyRepaint();
        }
        public void ClearData() { levelData = null; solutionData = null; constraintData = null; draftEndpointData = null; MarkDirtyRepaint(); }

        internal Rect debugContentRect = Rect.zero;
        private Rect ContentRect => debugContentRect != Rect.zero ? debugContentRect : contentRect;

        // ── Pointer handler that dispatch delegates to shared logic ──

        private void OnPointerDown(PointerDownEvent evt) => DoPointerDown(evt.localPosition, evt.pointerId);
        private void OnPointerMove(PointerMoveEvent evt) => DoPointerMove(evt.localPosition);
        private void OnPointerUp(PointerUpEvent evt) => DoPointerUp(evt.localPosition, evt.pointerId);
        private void OnPointerCancel(PointerCancelEvent evt) => DoPointerCancel();

        // ── Shared stroke logic (internal test seams) ──

        internal void DoPointerDown(Vector2 pos, int pointerId)
        {
            if (levelData == null) return;
            var br = FlowBoardViewGeometry.CalculateBoardRect(ContentRect, levelData.width, levelData.height, 4f);
            if (!FlowBoardViewGeometry.TryGetCell(br, levelData.width, levelData.height, pos, out var cell)) return;
            isStrokeActive = true; strokeCells.Clear(); strokeCells.Add(cell); lastStrokeCell = cell;
        }

        internal void DoPointerMove(Vector2 pos)
        {
            if (levelData == null) return;
            var br = FlowBoardViewGeometry.CalculateBoardRect(ContentRect, levelData.width, levelData.height, 4f);
            if (!FlowBoardViewGeometry.TryGetCell(br, levelData.width, levelData.height, pos, out var cell)) return;
            CellHovered?.Invoke(cell);
            if (!isStrokeActive) return;
            if (lastStrokeCell.HasValue && lastStrokeCell.Value.Equals(cell)) return;
            strokeCells.Add(cell); lastStrokeCell = cell;
        }

        internal void DoPointerUp(Vector2 pos, int pointerId)
        {
            if (!isStrokeActive) return;
            // Check if pointer up is on a new valid cell
            if (levelData != null)
            {
                var br = FlowBoardViewGeometry.CalculateBoardRect(ContentRect, levelData.width, levelData.height, 4f);
                if (FlowBoardViewGeometry.TryGetCell(br, levelData.width, levelData.height, pos, out var cell) && (!lastStrokeCell.HasValue || !lastStrokeCell.Value.Equals(cell)))
                    strokeCells.Add(cell);
            }
            isStrokeActive = false;
            if (strokeCells.Count == 1) CellSelected?.Invoke(strokeCells[0]);
            else if (strokeCells.Count > 1) CellStrokeCompleted?.Invoke(strokeCells.AsReadOnly());
            strokeCells.Clear(); lastStrokeCell = null;
        }

        internal void DoPointerCancel()
        {
            if (!isStrokeActive) return;
            isStrokeActive = false; strokeCells.Clear(); lastStrokeCell = null;
        }

        // ── render ──

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D; var rect = contentRect;
            painter.fillColor = new Color(0.12f, 0.12f, 0.14f); FillRect(painter, rect.x, rect.y, rect.width, rect.height);
            if (levelData == null) return;
            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(rect, levelData.width, levelData.height, 4f);
            var occ = new Dictionary<FlowPos, int>();
            if (solutionData != null) foreach (var p in solutionData.paths) foreach (var c in p.cells) occ[c] = p.colorId;
            var cons = new HashSet<FlowPos>();
            if (constraintData != null) foreach (var c in constraintData) if (c.cells != null) foreach (var cell in c.cells) cons.Add(cell);
            for (int x = 0; x < levelData.width; x++)
            for (int y = 0; y < levelData.height; y++)
            {
                var p = new FlowPos(x, y); var cr = FlowBoardViewGeometry.GetCellRect(boardRect, levelData.width, levelData.height, p);
                if (occ.TryGetValue(p, out var cid) && !cons.Contains(p)) { painter.fillColor = FlowEditorColorPalette.GetColor(cid); FillRect(painter, cr.x, cr.y, cr.width, cr.height); }
                if (cons.Contains(p)) { var cc = constraintData.FirstOrDefault(c2 => c2.cells != null && c2.cells.Contains(p)); var cid2 = cc?.colorId ?? 0; var constraintColor = FlowEditorColorPalette.GetColor(cid2); painter.fillColor = new Color(constraintColor.r, constraintColor.g, constraintColor.b, 0.5f); FillRect(painter, cr.x + 2, cr.y + 2, cr.width - 4, cr.height - 4); }
                painter.strokeColor = new Color(0.25f, 0.25f, 0.3f); painter.lineWidth = 0.5f; painter.BeginPath(); painter.MoveTo(new Vector2(cr.x, cr.y)); painter.LineTo(new Vector2(cr.xMax, cr.y)); painter.LineTo(new Vector2(cr.xMax, cr.yMax)); painter.LineTo(new Vector2(cr.x, cr.yMax)); painter.ClosePath(); painter.Stroke();
            }
            if (draftEndpointData != null)
            {
                foreach (var pair in draftEndpointData)
                {
                    if (pair.endpointA.HasValue) DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointA.Value, pair.colorId, true);
                    if (pair.endpointB.HasValue) DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointB.Value, pair.colorId, false);
                }
            }
            else if (levelData.pairs != null) foreach (var pair in levelData.pairs) { DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointA, pair.colorId, true); DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointB, pair.colorId, false); }
        }

        private void DrawEndpointMarker(Painter2D painter, Rect br, int w, int h, FlowPos cell, int colorId, bool isA)
        {
            var r = FlowBoardViewGeometry.GetCellRect(br, w, h, cell);
            var c = FlowEditorColorPalette.GetColor(colorId);
            painter.fillColor = Color.white;
            if (isA) FillDiamond(painter, r, r.width * 0.12f); else FillInsetRect(painter, r, r.width * 0.14f);
            painter.fillColor = new Color(c.r, c.g, c.b, 1f);
            if (isA) FillDiamond(painter, r, r.width * 0.24f); else FillInsetRect(painter, r, r.width * 0.27f);
        }
        private static void FillRect(Painter2D p, float x, float y, float w, float h) { p.BeginPath(); p.MoveTo(new Vector2(x, y)); p.LineTo(new Vector2(x + w, y)); p.LineTo(new Vector2(x + w, y + h)); p.LineTo(new Vector2(x, y + h)); p.ClosePath(); p.Fill(); }
        private static void FillInsetRect(Painter2D p, Rect r, float inset) => FillRect(p, r.x + inset, r.y + inset, r.width - inset * 2f, r.height - inset * 2f);
        private static void FillDiamond(Painter2D p, Rect r, float inset) { var cx = r.x + r.width * 0.5f; var cy = r.y + r.height * 0.5f; p.BeginPath(); p.MoveTo(new Vector2(cx, r.y + inset)); p.LineTo(new Vector2(r.xMax - inset, cy)); p.LineTo(new Vector2(cx, r.yMax - inset)); p.LineTo(new Vector2(r.x + inset, cy)); p.ClosePath(); p.Fill(); }

        internal enum BoardCellVisualKind { Empty, Solution, Constraint, Endpoint }
        internal enum EndpointVisualRole { None, A, B }
        internal BoardCellVisualKind GetDebugCellVisualKind(FlowPos cell)
        {
            if (levelData == null) return BoardCellVisualKind.Empty;
            if (draftEndpointData != null && draftEndpointData.Any(p => (p.endpointA.HasValue && p.endpointA.Value.Equals(cell)) || (p.endpointB.HasValue && p.endpointB.Value.Equals(cell)))) return BoardCellVisualKind.Endpoint;
            if (draftEndpointData == null && levelData.pairs != null && levelData.pairs.Any(p => p.endpointA.Equals(cell) || p.endpointB.Equals(cell))) return BoardCellVisualKind.Endpoint;
            if (constraintData != null && constraintData.Any(c => c.cells != null && c.cells.Contains(cell))) return BoardCellVisualKind.Constraint;
            if (solutionData != null && solutionData.paths.Any(p => p.cells.Contains(cell))) return BoardCellVisualKind.Solution;
            return BoardCellVisualKind.Empty;
        }

        internal EndpointVisualRole GetDebugEndpointRole(FlowPos cell)
        {
            if (draftEndpointData != null)
            {
                if (draftEndpointData.Any(p => p.endpointA.HasValue && p.endpointA.Value.Equals(cell))) return EndpointVisualRole.A;
                if (draftEndpointData.Any(p => p.endpointB.HasValue && p.endpointB.Value.Equals(cell))) return EndpointVisualRole.B;
            }
            else if (levelData?.pairs != null)
            {
                if (levelData.pairs.Any(p => p.endpointA.Equals(cell))) return EndpointVisualRole.A;
                if (levelData.pairs.Any(p => p.endpointB.Equals(cell))) return EndpointVisualRole.B;
            }
            return EndpointVisualRole.None;
        }

        private static FlowLevelData DeepCopyLevel(FlowLevelData s) { var c = new FlowLevelData { levelId = s.levelId, width = s.width, height = s.height, difficulty = s.difficulty, difficultyScore = s.difficultyScore }; if (s.pairs != null) foreach (var p in s.pairs) c.pairs.Add(new FlowPairData { colorId = p.colorId, endpointA = new(p.endpointA.x, p.endpointA.y), endpointB = new(p.endpointB.x, p.endpointB.y) }); return c; }
        private static FlowSolutionData DeepCopySolution(FlowSolutionData s) { var c = new FlowSolutionData { levelId = s.levelId }; if (s.paths != null) foreach (var p in s.paths) { var cc = new List<FlowPos>(p.cells.Count); foreach (var cell in p.cells) cc.Add(new FlowPos(cell.x, cell.y)); c.paths.Add(new FlowPathData { colorId = p.colorId, cells = cc }); } return c; }
    }
}
