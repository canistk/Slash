using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slash.Core
{
	public class SxGrid
    {
		public SxToken token;
        private SxGrid[] m_Arr; // 0 = left, 1 = up, 2 = right, 3 = down
        private SxBoard m_Board;
        public SxBoard board => m_Board;
        public object UI { get; set; } // For UI data binding, can be used to store any additional data needed for UI representation

        public SxCoord coord; // Unique identifier for the grid, can be used for debugging or UI purposes
		public string ReadableId
        {
            get
            {
                var ch = (char)('A' + coord.x);
                return $"{ch}{coord.y + 1}"; // e.g., A1, B2, etc.
			}
        }

		public SxGrid(int x, int y, SxBoard board)
        {
            this.coord = new SxCoord(x, y);
			this.m_Board    = board;
			this.m_Arr      = new SxGrid[4];
		}

        public void SetGrid(eDirection direction, SxGrid grid)
        {
            if (direction < eDirection.Left || direction > eDirection.Down)
            {
                SxLog.Error("Invalid direction specified.");
                return;
            }
            m_Arr[(int)direction] = grid;
        }

        public SxGrid GetGrid(eDirection direction)
        {
            if (direction < eDirection.Left || direction > eDirection.Down)
            {
                SxLog.Error("Invalid direction specified.");
                return null;
            }
            return m_Arr[(int)direction];
		}

		public bool TryGetGrid(eDirection direction, out SxGrid grid)
        {
            if (direction < eDirection.Left || direction > eDirection.Down)
            {
				SxLog.Error("Invalid direction specified.");
                grid = null;
                return false;
            }
            grid = m_Arr[(int)direction];
            return grid != null;
		}

		public bool HasToken()
        {
            return token != null;
		}

        public delegate void TokenChangeEvent(SxGrid grid, SxToken from, SxToken to);
		public static event TokenChangeEvent EVENT_TokenChanged;
		public void SetToken(SxToken newToken)
		{
			var before = token;
			if (token != null)
            {
                SxLog.Warning("Token already on board, disposing the old token.");
                token.Dispose();
			}

            token = newToken;
            if (token != null)
                token.LinkGrid(this);
			EVENT_TokenChanged?.Invoke(this, before, token);
		}

        public void ClearToken()
        {
            var before = token;
            token = null;
            if (before != null)
				before.Dispose();
            EVENT_TokenChanged?.Invoke(this, before, null);
		}
	}
}