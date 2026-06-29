using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Draft;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowBoardView : VisualElement
    {
        public event Action<FlowPos> CellHovered;
        public event Action<FlowPos> CellSelected;
        public event Action<IReadOnlyList<FlowPos>> CellStrokeCompleted;

        private FlowLevelData levelData;
        private FlowSolutionData solutionData;
        private List<FlowDraftConstraintData> constraintData;

        private bool isStrokeActive;
        private FlowPos? lastStrokeCell;
        private readonly List<FlowPos> strokeCells = new List<FlowPos>();

        private static readonly Color[] Palette = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, new(1f,0.5f,0f), new(0.5f,0f,1f), new(0f,0.7f,0.7f), new(1f,0.4f,0.7f), new(0.5f,1f,0f), new(0.7f,0.3f,0f) };

        public FlowBoardView()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        public void SetData(FlowLevelData level, FlowSolutionData solution) { levelData = level != null ? DeepCopyLevel(level) : null; solutionData = solution != null ? DeepCopySolution(solution) : null; constraintData = null; MarkDirtyRepaint(); }
        public void SetData(FlowLevelDraft draft)
        {
            if (draft == null) { ClearData(); return; }
            var dl = new FlowLevelData { levelId = draft.levelId, width = draft.width, height = draft.height };
            foreach (var p in draft.pairs) if (p.endpointA.HasValue || p.endpointB.HasValue) dl.pairs.Add(new FlowPairData { colorId = p.colorId, endpointA = p.endpointA ?? default, endpointB = p.endpointB ?? default });
            levelData = DeepCopyLevel(dl);
            solutionData = draft.currentSolution != null ? DeepCopySolution(draft.currentSolution) : null;
            constraintData = draft.fixedConstraints?.Select(c => c.Clone()).ToList();
            MarkDirtyRepaint();
        }
        public void ClearData() { levelData = null; solutionData = null; constraintData = null; MarkDirtyRepaint(); }

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
                if (occ.TryGetValue(p, out var cid) && !cons.Contains(p)) { painter.fillColor = Palette[cid % Palette.Length]; FillRect(painter, cr.x, cr.y, cr.width, cr.height); }
                if (cons.Contains(p)) { var cc = constraintData.FirstOrDefault(c2 => c2.cells != null && c2.cells.Contains(p)); var cid2 = cc?.colorId ?? 0; painter.fillColor = new Color(Palette[cid2 % Palette.Length].r, Palette[cid2 % Palette.Length].g, Palette[cid2 % Palette.Length].b, 0.5f); FillRect(painter, cr.x + 2, cr.y + 2, cr.width - 4, cr.height - 4); }
                painter.strokeColor = new Color(0.25f, 0.25f, 0.3f); painter.lineWidth = 0.5f; painter.BeginPath(); painter.MoveTo(new Vector2(cr.x, cr.y)); painter.LineTo(new Vector2(cr.xMax, cr.y)); painter.LineTo(new Vector2(cr.xMax, cr.yMax)); painter.LineTo(new Vector2(cr.x, cr.yMax)); painter.ClosePath(); painter.Stroke();
            }
            if (levelData.pairs != null) foreach (var pair in levelData.pairs) { DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointA, pair.colorId); DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointB, pair.colorId); }
        }

        private void DrawEndpointMarker(Painter2D painter, Rect br, int w, int h, FlowPos cell, int colorId) { var r = FlowBoardViewGeometry.GetCellRect(br, w, h, cell); var c = Palette[colorId % Palette.Length]; painter.fillColor = new Color(c.r, c.g, c.b, 1f); var inset = r.width * 0.15f; FillRect(painter, r.x + inset, r.y + inset, r.width - inset * 2f, r.height - inset * 2f); }
        private static void FillRect(Painter2D p, float x, float y, float w, float h) { p.BeginPath(); p.MoveTo(new Vector2(x, y)); p.LineTo(new Vector2(x + w, y)); p.LineTo(new Vector2(x + w, y + h)); p.LineTo(new Vector2(x, y + h)); p.ClosePath(); p.Fill(); }

        internal enum BoardCellVisualKind { Empty, Solution, Constraint, Endpoint }
        internal BoardCellVisualKind GetDebugCellVisualKind(FlowPos cell)
        {
            if (levelData == null) return BoardCellVisualKind.Empty;
            if (levelData.pairs != null && levelData.pairs.Any(p => p.endpointA.Equals(cell) || p.endpointB.Equals(cell))) return BoardCellVisualKind.Endpoint;
            if (constraintData != null && constraintData.Any(c => c.cells != null && c.cells.Contains(cell))) return BoardCellVisualKind.Constraint;
            if (solutionData != null && solutionData.paths.Any(p => p.cells.Contains(cell))) return BoardCellVisualKind.Solution;
            return BoardCellVisualKind.Empty;
        }

        private static FlowLevelData DeepCopyLevel(FlowLevelData s) { var c = new FlowLevelData { levelId = s.levelId, width = s.width, height = s.height, difficulty = s.difficulty, difficultyScore = s.difficultyScore }; if (s.pairs != null) foreach (var p in s.pairs) c.pairs.Add(new FlowPairData { colorId = p.colorId, endpointA = new(p.endpointA.x, p.endpointA.y), endpointB = new(p.endpointB.x, p.endpointB.y) }); return c; }
        private static FlowSolutionData DeepCopySolution(FlowSolutionData s) { var c = new FlowSolutionData { levelId = s.levelId }; if (s.paths != null) foreach (var p in s.paths) { var cc = new List<FlowPos>(p.cells.Count); foreach (var cell in p.cells) cc.Add(new FlowPos(cell.x, cell.y)); c.paths.Add(new FlowPathData { colorId = p.colorId, cells = cc }); } return c; }
    }
}
