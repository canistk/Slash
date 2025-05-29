using System.Collections;
using System.Collections.Generic;

namespace Slash.Core
{
	public class SxGrid
    {
		public SxToken token;
        private SxGrid[] m_Arr; // 0 = left, 1 = up, 2 = right, 3 = down
        private SxBoard m_Board;
        public SxBoard board => m_Board;
        public object UI { get; set; } // For UI data binding, can be used to store any additional data needed for UI representation


        public string Id { get; private set; } // Unique identifier for the grid, can be used for debugging or UI purposes
		public SxGrid(string id, SxBoard board)
        {
            this.Id         = id;
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

        public static event System.Action<SxGrid> EVENT_TokenChanged;
		public void SetToken(SxToken newToken)
        {
            if (token != null)
            {
                SxLog.Warning("Token already on board, disposing the old token.");
                token.Dispose();
			}
			if (newToken == null)
                return;

            token = newToken;
            if (token == null)
                throw new System.ArgumentNullException(nameof(newToken), "Token cannot be null.");
			token.LinkGrid(this);
            EVENT_TokenChanged?.Invoke(this);
		}

        public void ClearToken()
        {
            token = null;
			EVENT_TokenChanged?.Invoke(this);
		}

        public void HandleUIClick(object caller)
        {
            if (caller != UI)
                throw new SxException("Caller does not match the UI instance of this grid.");
            board.TryApplyPlayerSelection(this);
		}
	}
}