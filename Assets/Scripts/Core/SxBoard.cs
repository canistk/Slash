using System.Collections;
using System.Collections.Generic;
namespace Slash.Core
{
    public class SxBoard
    {
        private SxGrid[,] m_Grids = null;

		public delegate void GridCreated(int x, int y, SxGrid grid);
		public SxBoard(int width, int height,
			GridCreated onCreated)
        {
            if (width <= 0 || height <= 0)
            {
                SxLog.Error($"Invalid grid dimensions: {width}x{height}. Both dimensions must be greater than zero.");
                return;
			}

			m_Grids = new SxGrid[width, height];
            
            // allocate grids
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    m_Grids[x, y] = new SxGrid(this);
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
					onCreated?.Invoke(x, y, grid);
                }
            }
        }
        public SxGrid TryGetGrid(int x, int y)
        {
            if (m_Grids == null ||
                x < 0 || x >= m_Grids.GetLength(0) ||
                y < 0 || y >= m_Grids.GetLength(1))
            {
                SxLog.Error($"Invalid grid coordinates: ({x}, {y}) out of ({m_Grids.GetLength(0)}, {m_Grids.GetLength(1)})");
                return null;
            }
            return m_Grids[x, y];
        }

        public bool HasToken(int x, int y)
        {
            var grid = TryGetGrid(x, y);
            if (grid == null)
            {
				SxLog.Error($"No grid found at ({x}, {y})");
                return false;
            }
            return grid.HasToken();
        }

        public bool TrySetToken(int x, int y, SxToken token)
		{
			var grid = TryGetGrid(x, y);
			if (grid == null)
			{
				SxLog.Error($"No grid found at ({x}, {y})");
				return false;
			}
			if (token == null)
			{
				SxLog.Error("Token cannot be null.");
				return false;
			}
			grid.SetToken(token);
			return true;
		}


		#region Rule
		eGameRule m_Rule = eGameRule.None;
		public eGameRule Rule
		{
			get { return m_Rule; }
			private set
			{
				if (m_Rule == value)
					return;
				m_Rule = value;
				SxLog.Info($"Game rule changed to: {m_Rule}");
				EVENT_GameRuleChanged?.Invoke(m_Rule);
			}
		}
		public void SetRule(eGameRule rule) => Rule = rule;
		#endregion Rule

		#region Events
		public event System.Action<eGameRule> EVENT_GameRuleChanged;
		public delegate void BoardCreated(SxBoard board);
		#endregion Events

		/// <summary>
		/// Tries to apply the player's selection on the grid.
		/// </summary>
		/// <param name="grid"></param>
		/// <returns>
		/// true = accept player selection, place or flip token.
		/// false = reject player selection, do not execute require.
		/// </returns>
		internal bool TryApplyPlayerSelection(SxGrid grid)
        {
            // Handle the click event on the grid,
            // based on current game logic.


            // grid.HasToken();

            switch (Rule)
            {
                default:
                case eGameRule.None:
				SxLog.Error("No game rule set. Cannot handle click on grid.");
                return false;

                case eGameRule.Reversi:
                // TODO: SxUtil.Reversi Rule
                break;

                case eGameRule.Gobang:
                break;

                case eGameRule.Checkers:
                break;
            }

            SxLog.Error("Unhandled game rule: " + Rule);
			return false;
        }
    }
}