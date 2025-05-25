using System.Collections;
using System.Collections.Generic;

namespace Slash.Core
{
	public class SxGrid
    {
		public SxToken token;
        private SxGrid[] m_Arr; // 0 = left, 1 = up, 2 = right, 3 = down

        public SxGrid()
        {
            m_Arr = new SxGrid[4];
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
	}
}