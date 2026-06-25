using System;
using System.Collections.Generic;

namespace FlowPuzzle.Core
{
    public sealed class FlowBoard
    {
        public const int EmptyColorId = -1;

        private readonly int[,] cells;

        public int Width { get; }
        public int Height { get; }
        public int OccupiedCellCount { get; private set; }

        public FlowBoard(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            cells = new int[width, height];
            OccupiedCellCount = 0;

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                cells[x, y] = EmptyColorId;
        }

        public bool IsInside(FlowPos position)
        {
            return position.x >= 0 && position.x < Width
                && position.y >= 0 && position.y < Height;
        }

        public bool IsEmpty(FlowPos position)
        {
            RequireInside(position);
            return cells[position.x, position.y] == EmptyColorId;
        }

        public int Get(FlowPos position)
        {
            RequireInside(position);
            return cells[position.x, position.y];
        }

        public void Set(FlowPos position, int colorId)
        {
            RequireInside(position);
            if (colorId < 0)
                throw new ArgumentOutOfRangeException(nameof(colorId));

            var wasEmpty = cells[position.x, position.y] == EmptyColorId;
            var willBeEmpty = colorId == EmptyColorId;

            cells[position.x, position.y] = colorId;

            if (wasEmpty && !willBeEmpty)
                OccupiedCellCount++;
            else if (!wasEmpty && willBeEmpty)
                OccupiedCellCount--;
        }

        public void Clear(FlowPos position)
        {
            RequireInside(position);

            if (cells[position.x, position.y] != EmptyColorId)
            {
                cells[position.x, position.y] = EmptyColorId;
                OccupiedCellCount--;
            }
        }

        public List<FlowPos> GetNeighbors(FlowPos position)
        {
            RequireInside(position);

            var result = new List<FlowPos>(4);

            // deterministic order: right, left, up, down
            TryAddNeighbor(result, position.x + 1, position.y);
            TryAddNeighbor(result, position.x - 1, position.y);
            TryAddNeighbor(result, position.x, position.y + 1);
            TryAddNeighbor(result, position.x, position.y - 1);

            return result;
        }

        private void TryAddNeighbor(List<FlowPos> list, int x, int y)
        {
            var pos = new FlowPos(x, y);
            if (IsInside(pos))
                list.Add(pos);
        }

        private void RequireInside(FlowPos position)
        {
            if (!IsInside(position))
                throw new ArgumentOutOfRangeException(nameof(position));
        }
    }
}
