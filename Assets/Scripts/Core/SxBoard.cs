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

        public bool TryGetGrid(SxCoord coord, out SxGrid grid)
        {
            return TryGetGrid(coord.x, coord.y, out grid);
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

		#region State & Rule
        private eGameRule m_Rule;
		private eGameState m_State;
		private eTurn m_Turn;
        public eGameRule Rule => m_Rule;
        public eGameState State => m_State;
        public eTurn Turn => m_Turn;
		private SxLogicHandler m_Logic;

		public void Init(eGameRule rule, eTurn turn)
        {
            if (m_State != eGameState.None)
            {
                SxLog.Error($"Cannot change game rule from {m_State} to {rule}. Game is already in progress.");
                return;
            }
            m_State = eGameState.InitBoard;
            m_Rule = rule;
            m_Turn = turn;
            if (TryGetLogicHandler(out m_Logic))
            {
                m_Logic.InitBoard(this);
            }
            else
            {
                SxLog.Error($"Failed to get logic handler for rule: {rule}");
            }
            m_State = eGameState.SolveConflict;
		}

        internal void ChangeState(object caller, eGameState state)
        {
            if (caller is not SxLogicHandler logic)
                return;
            if (m_Logic != logic)
                return;

            m_State = state;
		}

        private bool TryGetLogicHandler(out SxLogicHandler logic)
        {
            logic = default;
            switch (m_Rule)
            {
                case eGameRule.Reversi:
                    logic = new SxReversiLogicHandler();
                    break;
                case eGameRule.Gobang:
                    //logic = new SxGobangLogicHandler();
                    break;
                case eGameRule.Checkers:
                    //logic = new SxCheckersLogicHandler();
                    break;
                default:
                    SxLog.Error($"Unsupported game rule: {m_Rule}");
                    return false;
			}
            return logic != null;
		}

		public bool TryChangeMode(eGameRule rule)
        {
            if (m_State <= eGameState.InitBoard)
            {
                SxLog.Error($"Cannot change game rule from {m_Rule} to {rule}. call Init() instead.");
                return false;
            }
            if (m_State == eGameState.ValidatingMove)
            {
                SxLog.Error($"Cannot change game rule from {m_Rule} to {rule}. Game is currently validating a move.");
                return false;
			}

            m_Rule = rule;
			if (TryGetLogicHandler(out m_Logic))
			{
				m_Logic.ChangeMode(this);
			}
			else
			{
				SxLog.Error($"Failed to get logic handler for rule: {rule}");
			}
            return true;
		}

		#endregion State & Rule

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
            if (m_State != eGameState.WaitingForInput)
                return false;

			// grid.HasToken();
			if (m_Logic != null)
			{
				// m_Logic.InitBoard

				return true;
			}

            SxLog.Error($"Unhandled game rule: {m_Rule}");
			return false;
        }
    }
}