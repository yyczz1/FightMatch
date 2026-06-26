using System;
using FlowPuzzle.Core;
using UnityEngine;

namespace FlowPuzzle.Editor.UI
{
    public static class FlowBoardViewGeometry
    {
        public static Rect CalculateBoardRect(Rect contentRect, int width, int height, float padding)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("Dimensions must be positive.");
            if (padding < 0f)
                throw new ArgumentOutOfRangeException(nameof(padding), "Padding must not be negative.");

            var inner = new Rect(
                contentRect.x + padding,
                contentRect.y + padding,
                contentRect.width - padding * 2f,
                contentRect.height - padding * 2f);

            var cellAspect = (float)width / height;
            var innerAspect = inner.width / inner.height;

            float boardW, boardH;
            if (cellAspect > innerAspect)
            {
                boardW = inner.width;
                boardH = inner.width / cellAspect;
            }
            else
            {
                boardH = inner.height;
                boardW = inner.height * cellAspect;
            }

            return new Rect(
                inner.x + (inner.width - boardW) / 2f,
                inner.y + (inner.height - boardH) / 2f,
                boardW, boardH);
        }

        public static Rect GetCellRect(Rect boardRect, int width, int height, FlowPos cell)
        {
            var cellW = boardRect.width / width;
            var cellH = boardRect.height / height;
            var x = boardRect.x + cell.x * cellW;
            var y = boardRect.y + (height - 1 - cell.y) * cellH; // Y inversion
            return new Rect(x, y, cellW, cellH);
        }

        public static bool TryGetCell(Rect boardRect, int width, int height, Vector2 point, out FlowPos cell)
        {
            cell = default;
            if (!boardRect.Contains(point)) return false;
            var relX = point.x - boardRect.x;
            var relY = boardRect.yMax - point.y; // invert Y
            var cx = (int)(relX / boardRect.width * width);
            var cy = (int)(relY / boardRect.height * height);
            if (cx < 0 || cx >= width || cy < 0 || cy >= height) return false;
            cell = new FlowPos(cx, cy);
            return true;
        }
    }
}
