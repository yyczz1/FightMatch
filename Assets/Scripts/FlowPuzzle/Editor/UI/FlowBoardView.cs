using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Editor.Draft;
using UnityEditor;
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

        // Stroke state
        private bool isStrokeActive;
        private FlowPos? lastStrokeCell;
        private readonly List<FlowPos> strokeCells = new List<FlowPos>();

        private static readonly Color[] Palette =
        {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.magenta, Color.cyan, new(1f,0.5f,0f), new(0.5f,0f,1f),
            new(0f,0.7f,0.7f), new(1f,0.4f,0.7f), new(0.5f,1f,0f), new(0.7f,0.3f,0f)
        };

        public FlowBoardView()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCancel);
        }

        public void SetData(FlowLevelData level, FlowSolutionData solution)
        {
            levelData = level != null ? DeepCopyLevel(level) : null;
            solutionData = solution != null ? DeepCopySolution(solution) : null;
            constraintData = null;
            MarkDirtyRepaint();
        }

        public void SetData(FlowLevelDraft draft)
        {
            if (draft == null) { ClearData(); return; }
            var displayLevel = new FlowLevelData { levelId = draft.levelId, width = draft.width, height = draft.height };
            foreach (var p in draft.pairs)
            {
                if (p.endpointA.HasValue || p.endpointB.HasValue)
                    displayLevel.pairs.Add(new FlowPairData { colorId = p.colorId, endpointA = p.endpointA ?? default, endpointB = p.endpointB ?? default });
            }
            levelData = DeepCopyLevel(displayLevel);
            solutionData = draft.currentSolution != null ? DeepCopySolution(draft.currentSolution) : null;
            constraintData = draft.fixedConstraints != null && draft.fixedConstraints.Count > 0
                ? draft.fixedConstraints.Select(c => c.Clone()).ToList() : null;
            MarkDirtyRepaint();
        }

        public void ClearData()
        {
            levelData = null; solutionData = null; constraintData = null;
            MarkDirtyRepaint();
        }

        // ── render ──

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D; var rect = contentRect;
            painter.fillColor = new Color(0.12f, 0.12f, 0.14f);
            FillRect(painter, rect.x, rect.y, rect.width, rect.height);
            if (levelData == null) return;

            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(rect, levelData.width, levelData.height, 4f);

            // Solution cells
            var occupiedColor = new Dictionary<FlowPos, int>();
            if (solutionData != null)
                foreach (var path in solutionData.paths)
                    foreach (var cell in path.cells)
                        occupiedColor[cell] = path.colorId;

            // Constraint cells (for distinct rendering)
            var constraintSet = new HashSet<FlowPos>();
            if (constraintData != null)
                foreach (var c in constraintData)
                    if (c.cells != null)
                        foreach (var cell in c.cells)
                            constraintSet.Add(cell);

            for (var x = 0; x < levelData.width; x++)
            for (var y = 0; y < levelData.height; y++)
            {
                var pos = new FlowPos(x, y);
                var cellRect = FlowBoardViewGeometry.GetCellRect(boardRect, levelData.width, levelData.height, pos);
                if (occupiedColor.TryGetValue(pos, out var colorId) && !constraintSet.Contains(pos))
                {
                    painter.fillColor = Palette[colorId % Palette.Length];
                    FillRect(painter, cellRect.x, cellRect.y, cellRect.width, cellRect.height);
                }
                // Constraint cells — thinner overlay
                if (constraintSet.Contains(pos))
                {
                    var cc = constraintData.FirstOrDefault(c => c.cells != null && c.cells.Contains(pos));
                    var cid = cc?.colorId ?? 0;
                    painter.fillColor = new Color(Palette[cid % Palette.Length].r, Palette[cid % Palette.Length].g, Palette[cid % Palette.Length].b, 0.5f);
                    FillRect(painter, cellRect.x + 2, cellRect.y + 2, cellRect.width - 4, cellRect.height - 4);
                }
                // Grid
                painter.strokeColor = new Color(0.25f, 0.25f, 0.3f); painter.lineWidth = 0.5f;
                painter.BeginPath(); painter.MoveTo(new Vector2(cellRect.x, cellRect.y)); painter.LineTo(new Vector2(cellRect.xMax, cellRect.y));
                painter.LineTo(new Vector2(cellRect.xMax, cellRect.yMax)); painter.LineTo(new Vector2(cellRect.x, cellRect.yMax));
                painter.ClosePath(); painter.Stroke();
            }

            // Endpoints
            if (levelData.pairs != null)
                foreach (var pair in levelData.pairs)
                {
                    DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointA, pair.colorId);
                    DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height, pair.endpointB, pair.colorId);
                }
        }

        // ── pointer events ──

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (levelData == null) return;
            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(contentRect, levelData.width, levelData.height, 4f);
            if (!FlowBoardViewGeometry.TryGetCell(boardRect, levelData.width, levelData.height, evt.localPosition, out var cell))
                return;

            // Start stroke
            isStrokeActive = true;
            strokeCells.Clear();
            strokeCells.Add(cell);
            lastStrokeCell = cell;
            this.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (levelData == null) return;
            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(contentRect, levelData.width, levelData.height, 4f);
            if (!FlowBoardViewGeometry.TryGetCell(boardRect, levelData.width, levelData.height, evt.localPosition, out var cell))
            {
                CellHovered?.Invoke(default); // outside
                return;
            }
            CellHovered?.Invoke(cell);

            if (!isStrokeActive) return;
            // Deduplicate
            if (lastStrokeCell.HasValue && lastStrokeCell.Value.Equals(cell)) return;
            strokeCells.Add(cell);
            lastStrokeCell = cell;
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!isStrokeActive) return;
            isStrokeActive = false;
            this.ReleasePointer(evt.pointerId);
            if (strokeCells.Count == 1)
            {
                // Single click — emit CellSelected for endpoint tools
                CellSelected?.Invoke(strokeCells[0]);
            }
            else if (strokeCells.Count >= 1)
            {
                CellStrokeCompleted?.Invoke(strokeCells.AsReadOnly());
            }
            strokeCells.Clear();
            lastStrokeCell = null;
        }

        private void OnPointerCancel(PointerCaptureOutEvent evt)
        {
            if (!isStrokeActive) return;
            isStrokeActive = false;
            strokeCells.Clear();
            lastStrokeCell = null;
        }

        // ── render helpers ──

        private void DrawEndpointMarker(Painter2D painter, Rect br, int w, int h, FlowPos cell, int colorId)
        {
            var r = FlowBoardViewGeometry.GetCellRect(br, w, h, cell);
            var c = Palette[colorId % Palette.Length];
            painter.fillColor = new Color(c.r, c.g, c.b, 1f);
            var inset = r.width * 0.15f;
            FillRect(painter, r.x + inset, r.y + inset, r.width - inset * 2f, r.height - inset * 2f);
        }

        private static void FillRect(Painter2D p, float x, float y, float w, float h)
        {
            p.BeginPath(); p.MoveTo(new Vector2(x, y)); p.LineTo(new Vector2(x + w, y));
            p.LineTo(new Vector2(x + w, y + h)); p.LineTo(new Vector2(x, y + h));
            p.ClosePath(); p.Fill();
        }

        // ── internal test seam ──

        internal enum BoardCellVisualKind { Empty, Solution, Constraint, Endpoint }

        internal BoardCellVisualKind GetDebugCellVisualKind(FlowPos cell)
        {
            if (levelData == null) return BoardCellVisualKind.Empty;
            var isEndpoint = levelData.pairs != null && levelData.pairs.Any(p => p.endpointA.Equals(cell) || p.endpointB.Equals(cell));
            if (isEndpoint) return BoardCellVisualKind.Endpoint;
            if (constraintData != null && constraintData.Any(c => c.cells != null && c.cells.Contains(cell)))
                return BoardCellVisualKind.Constraint;
            if (solutionData != null && solutionData.paths.Any(p => p.cells.Contains(cell)))
                return BoardCellVisualKind.Solution;
            return BoardCellVisualKind.Empty;
        }

        // ── deep copy ──

        private static FlowLevelData DeepCopyLevel(FlowLevelData src)
        {
            var c = new FlowLevelData { levelId = src.levelId, width = src.width, height = src.height, difficulty = src.difficulty, difficultyScore = src.difficultyScore };
            if (src.pairs != null) foreach (var p in src.pairs) c.pairs.Add(new FlowPairData { colorId = p.colorId, endpointA = new(p.endpointA.x, p.endpointA.y), endpointB = new(p.endpointB.x, p.endpointB.y) });
            return c;
        }

        private static FlowSolutionData DeepCopySolution(FlowSolutionData src)
        {
            var c = new FlowSolutionData { levelId = src.levelId };
            if (src.paths != null) foreach (var p in src.paths) { var cc = new List<FlowPos>(p.cells.Count); foreach (var cell in p.cells) cc.Add(new FlowPos(cell.x, cell.y)); c.paths.Add(new FlowPathData { colorId = p.colorId, cells = cc }); }
            return c;
        }
    }
}
