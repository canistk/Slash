using System.Collections;
using System.Collections.Generic;
namespace Slash.Core
{
    public class SxBoard
    {
        private SxGrid[,] m_Grids = null;
        private Dictionary<string, SxGrid> m_GridLookup = new Dictionary<string, SxGrid>();

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
			m_GridLookup = new Dictionary<string, SxGrid>(width * height);

			// allocate grids
			for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var grid = new SxGrid(x, y, this);
					m_Grids[x, y] = grid;
					m_GridLookup.Add(grid.ReadableId, m_Grids[x, y]);
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
        public bool TryGetGrid(int x, int y, out SxGrid grid)
        {
            if (m_Grids == null ||
                x < 0 || x >= m_Grids.GetLength(0) ||
                y < 0 || y >= m_Grids.GetLength(1))
            {
                SxLog.Error($"Invalid grid coordinates: ({x}, {y}) out of ({m_Grids.GetLength(0)}, {m_Grids.GetLength(1)})");
                grid = default;
				return false;
            }
            grid = m_Grids[x, y];
            return grid != null;
        }

        public bool TryGetGrid(string id, out SxGrid grid)
        {
            return m_GridLookup.TryGetValue(id, out grid);
		}

        public SxGrid GetGrid(int x, int y)
        {
            if (TryGetGrid(x, y, out var grid))
            {
                return grid;
            }
            SxLog.Error($"No grid found at ({x}, {y})");
            return null;
		}

        public SxGrid GetGrid(string id)
        {
            if (TryGetGrid(id, out var grid))
            {
                return grid;
            }
            SxLog.Error($"No grid found with ID: {id}");
            return null;
        }

        public bool HasGrid(int x, int y)
        {
            return TryGetGrid(x, y, out _);
		}

		public bool HasToken(int x, int y)
        {
            if (!TryGetGrid(x, y, out var grid))
                return false;

            if (grid == null)
            {
				SxLog.Error($"No grid found at ({x}, {y})");
                return false;
            }
            return grid.HasToken();
        }

        public bool TrySetToken(int x, int y, SxToken token)
		{
			if (token == null)
			{
				SxLog.Error("Token cannot be null.");
				return false;
			}

            if (!TryGetGrid(x, y, out var grid))
            {
                return false;
            }

			if (grid == null)
			{
				SxLog.Error($"No grid found at ({x}, {y})");
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