using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slash
{
    public class SxGrids
    {
        private SxGrid[,] m_Grids = null;

        public SxGrids(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError($"Invalid grid dimensions: {width}x{height}. Both dimensions must be greater than zero.");
                return;
			}

			m_Grids = new SxGrid[width, height];
            
            // allocate grids
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    m_Grids[x, y] = new SxGrid();
                }
            }

            // set neighbors
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var grid = m_Grids[x, y];
                    if (x > 0)          grid.SetGrid(eDirection.Left,   m_Grids[x - 1, y]);
                    if (y > 0)          grid.SetGrid(eDirection.Up,     m_Grids[x, y - 1]);
                    if (x < width - 1)  grid.SetGrid(eDirection.Right,  m_Grids[x + 1, y]);
                    if (y < height - 1) grid.SetGrid(eDirection.Down,   m_Grids[x, y + 1]);
                }
            }
        }
        public SxGrid TryGetGrid(int x, int y)
        {
            if (m_Grids == null ||
                x < 0 || x >= m_Grids.GetLength(0) ||
                y < 0 || y >= m_Grids.GetLength(1))
            {
                Debug.LogError($"Invalid grid coordinates: ({x}, {y}) out of ({m_Grids.GetLength(0)}, {m_Grids.GetLength(1)})");
                return null;
            }
            return m_Grids[x, y];
        }
        public bool HasToken(int x, int y)
        {
            var grid = TryGetGrid(x, y);
            if (grid == null)
            {
                Debug.LogError($"No grid found at ({x}, {y})");
                return false;
            }
            return grid.HasToken();
        }
    }
}