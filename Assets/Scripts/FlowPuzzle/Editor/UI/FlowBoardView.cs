using System;
using System.Collections.Generic;
using FlowPuzzle.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowBoardView : VisualElement
    {
        public event Action<FlowPos> CellHovered;
        public event Action<FlowPos> CellSelected;

        private FlowLevelData levelData;
        private FlowSolutionData solutionData;

        private static readonly Color[] Palette =
        {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.magenta, Color.cyan, new(1f, 0.5f, 0f), new(0.5f, 0f, 1f),
            new(0f, 0.7f, 0.7f), new(1f, 0.4f, 0.7f), new(0.5f, 1f, 0f), new(0.7f, 0.3f, 0f)
        };

        public FlowBoardView()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public void SetData(FlowLevelData level, FlowSolutionData solution)
        {
            levelData = level;
            solutionData = solution;
            MarkDirtyRepaint();
        }

        public void ClearData()
        {
            levelData = null;
            solutionData = null;
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            var rect = contentRect;

            // Background
            painter.fillColor = new Color(0.12f, 0.12f, 0.14f);
            FillRect(painter, rect.x, rect.y, rect.width, rect.height);

            if (levelData == null) return;

            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(
                rect, levelData.width, levelData.height, 4f);

            // Build occupancy from solution if present
            var occupiedColor = new Dictionary<FlowPos, int>();
            if (solutionData != null)
            {
                foreach (var path in solutionData.paths)
                    foreach (var cell in path.cells)
                        occupiedColor[cell] = path.colorId;
            }

            // Draw cells
            for (var x = 0; x < levelData.width; x++)
            {
                for (var y = 0; y < levelData.height; y++)
                {
                    var pos = new FlowPos(x, y);
                    var cellRect = FlowBoardViewGeometry.GetCellRect(
                        boardRect, levelData.width, levelData.height, pos);

                    if (occupiedColor.TryGetValue(pos, out var colorId))
                    {
                        var c = Palette[colorId % Palette.Length];
                        painter.fillColor = c;
                        FillRect(painter, cellRect.x, cellRect.y, cellRect.width, cellRect.height);
                    }

                    // Grid
                    painter.strokeColor = new Color(0.25f, 0.25f, 0.3f);
                    painter.lineWidth = 0.5f;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(cellRect.x, cellRect.y));
                    painter.LineTo(new Vector2(cellRect.xMax, cellRect.y));
                    painter.LineTo(new Vector2(cellRect.xMax, cellRect.yMax));
                    painter.LineTo(new Vector2(cellRect.x, cellRect.yMax));
                    painter.ClosePath();
                    painter.Stroke();
                }
            }

            // Draw endpoints larger
            if (levelData.pairs != null)
            {
                foreach (var pair in levelData.pairs)
                {
                    DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height,
                        pair.endpointA, pair.colorId);
                    DrawEndpointMarker(painter, boardRect, levelData.width, levelData.height,
                        pair.endpointB, pair.colorId);
                }
            }
        }

        private void DrawEndpointMarker(Painter2D painter, Rect boardRect, int w, int h, FlowPos cell, int colorId)
        {
            var r = FlowBoardViewGeometry.GetCellRect(boardRect, w, h, cell);
            var c = Palette[colorId % Palette.Length];
            painter.fillColor = new Color(c.r, c.g, c.b, 1f);
            var inset = r.width * 0.15f;
            FillRect(painter, r.x + inset, r.y + inset, r.width - inset * 2f, r.height - inset * 2f);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (levelData == null) return;
            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(
                contentRect, levelData.width, levelData.height, 4f);
            if (FlowBoardViewGeometry.TryGetCell(boardRect, levelData.width, levelData.height,
                    evt.localPosition, out var cell))
                CellHovered?.Invoke(cell);
        }

        private static void FillRect(Painter2D p, float x, float y, float w, float h)
        {
            p.BeginPath();
            p.MoveTo(new Vector2(x, y));
            p.LineTo(new Vector2(x + w, y));
            p.LineTo(new Vector2(x + w, y + h));
            p.LineTo(new Vector2(x, y + h));
            p.ClosePath();
            p.Fill();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (levelData == null) return;
            var boardRect = FlowBoardViewGeometry.CalculateBoardRect(
                contentRect, levelData.width, levelData.height, 4f);
            if (FlowBoardViewGeometry.TryGetCell(boardRect, levelData.width, levelData.height,
                    evt.localPosition, out var cell))
                CellSelected?.Invoke(cell);
        }
    }
}
