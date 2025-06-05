using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slash.Core
{
	public class SxGrid
    {
        public SxToken token { get; private set; } = null;
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

		public static event LinkTokenEvent EVENT_Linked, EVENT_Unlinked;
		public static event System.Action<SxGrid> EVENT_Updated;
		public void Link(SxToken newToken)
		{
			var before = token;
            token = newToken;
            if (before != null && ReferenceEquals(before.GetGrid(), this))
			{
				before.Link(null); // remove
				EVENT_Unlinked?.Invoke(this, before);
			}

			if (newToken != null)
            {
                if (!ReferenceEquals(newToken.GetGrid(), this))
				    newToken.Link(this); // link to THIS grid
				EVENT_Linked?.Invoke(this, newToken);
			}

			EVENT_Updated?.Invoke(this);
		}

	}
	public delegate void LinkTokenEvent(SxGrid grid, SxToken token);

}