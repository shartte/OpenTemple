using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.Raycast
{
    public struct PartialSectorObjectEnumerator
    {
        private readonly Rectangle _rectangle;

        private readonly List<GameObjectBody>[,] _tiles;

        private int _currentX;

        private int _currentY;

        private int _nextListIndex;

        public PartialSectorObjectEnumerator(List<GameObjectBody>[,] tiles, Rectangle rectangle)
        {
            _tiles = tiles;
            _rectangle = rectangle;
            _currentX = rectangle.Left;
            _currentY = rectangle.Top;
            _nextListIndex = 0;
            Current = null;
        }

        public bool MoveNext()
        {
            while (_currentX < _rectangle.Right && _currentY < _rectangle.Bottom)
            {
                var currentTiles = _tiles[_currentX, _currentY];
                if (currentTiles !=  null && _nextListIndex < currentTiles.Count)
                {
                    Current = currentTiles[_nextListIndex++];
                    return true;
                }

                _nextListIndex = 0;

                if (++_currentX >= _rectangle.Right)
                {
                    _currentX = _rectangle.Left;
                    _currentY++;
                }
            }

            return false;
        }

        public GameObjectBody Current { get; private set; }
    }
}