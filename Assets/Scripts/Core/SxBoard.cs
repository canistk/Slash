using Slash.Core.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
namespace Slash.Core
{
    public class SxBoard
    {
        private SxGrid[,] m_Grids = null;
        private Dictionary<string, SxGrid> m_GridLookup = new Dictionary<string, SxGrid>();
        private int[] m_Score; // 0 = white, 1 = black

        public int Width => m_Grids?.GetLength(0) ?? 0;
        public int Height => m_Grids?.GetLength(1) ?? 0;

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
            m_Score = new int[2] { 0, 0 }; // Initialize scores for white and black
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
                    if (x > 0) grid.SetGrid(eDirection.Left, m_Grids[x - 1, y]);
                    if (y > 0) grid.SetGrid(eDirection.Up, m_Grids[x, y - 1]);
                    if (x < width - 1) grid.SetGrid(eDirection.Right, m_Grids[x + 1, y]);
                    if (y < height - 1) grid.SetGrid(eDirection.Down, m_Grids[x, y + 1]);
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
                // SxLog.Error($"Invalid grid coordinates: ({x}, {y}) out of ({m_Grids.GetLength(0)}, {m_Grids.GetLength(1)})");
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

        public void SetScore(eTurn turn, int score)
        {
            if (turn == eTurn.White)
            {
                m_Score[0] = score;
            }
            else if (turn == eTurn.Black)
            {
                m_Score[1] = score;
            }
            else
            {
                SxLog.Error($"Invalid turn: {turn}. Cannot set score.");
            }
		}

        public void AddScore(eTurn turn, int score)
        {
            if (turn == eTurn.White)
            {
                m_Score[0] += score;
            }
            else if (turn == eTurn.Black)
            {
                m_Score[1] += score;
            }
            else
            {
                SxLog.Error($"Invalid turn: {turn}. Cannot add score.");
            }
		}

        public int GetScore(eTurn turn)
        {
            if (turn == eTurn.White)
            {
                return m_Score[0];
            }
            else if (turn == eTurn.Black)
            {
                return m_Score[1];
            }
            else
            {
                SxLog.Error($"Invalid turn: {turn}. Cannot get score.");
                return 0;
            }
        }
        public void ResetScores()
        {
            m_Score[0] = 0;
            m_Score[1] = 0;
		}

		#region State & Rule
        private eGameState m_State;
        private eTurn m_Turn;
		private eGameRule m_Rule;
        public eGameRule Rule
        {
            get => m_Rule;
            private set
            {
                if (m_Rule == value)
                    return;

                m_Rule = value;
				switch (value)
				{
					case eGameRule.Reversi:
					if (m_Logics[0] == null)
						m_Logics[0] = new SxReversiLogicHandler(this);
					break;
					case eGameRule.Gobang:
					if (m_Logics[1] == null)
						m_Logics[1] = new SxGobangLogicHandler(this);
					break;
					case eGameRule.Checkers:
					if (m_Logics[2] == null)
						m_Logics[2] = new SxCheckersLogicHandler(this);
					break;
					default:
					SxLog.Error($"Unsupported game rule: {Rule}");
                    break;
				}
			}
        }
        public eGameState State => m_State;
        public eTurn Turn => m_Turn;

        public void Init(eGameRule rule, eTurn turn)
        {
            if (m_State != eGameState.None)
            {
                SxLog.Error($"Cannot change game rule from {m_State} to {rule}. Game is already in progress.");
                return;
            }
            m_State = eGameState.InitBoard;
            m_Turn = turn;
            Rule = rule;
            //if (!TryChangeMode(rule))
            //{
            //    SxLog.Error($"Failed to get logic handler for rule: {rule}");
            //}
			// m_State = eGameState.SolveConflict;
			m_State = eGameState.WaitingForInput;
		}

        public void Update()
        {
            HandleTasks();
		}

        internal void ChangeState(object caller, eGameState state)
        {
            if (caller is not SxLogicHandler logic)
                return;
            if (!TryGetLogicHandler(out var current))
                return;
            if (current != logic)
                return;
            m_State = state;
        }

        // 0 = Reversi, 1 = Gobang, 2 = Checkers
        private SxLogicHandler[] m_Logics = new SxLogicHandler[3];
		private bool TryGetLogicHandler(out SxLogicHandler logic)
        {
			logic = m_Logics[(int)Rule - 1];
			return logic != null;
        }

        public bool TryChangeMode(eGameRule rule)
        {
            if (m_State <= eGameState.InitBoard)
            {
                SxLog.Error($"Cannot change game rule from {Rule} to {rule}. call Init() instead.");
                return false;
            }
            if (m_State == eGameState.ValidatingMove)
            {
                SxLog.Error($"Cannot change game rule from {Rule} to {rule}. Game is currently validating a move.");
                return false;
            }

            this.Rule = rule;
            if (TryGetLogicHandler(out var logic))
            {
                logic.ChangeMode(this);
                logic.SolveConflict();
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

        public bool IsWaitingForPlayer => m_State == eGameState.WaitingForInput;

        /// <summary>
        /// Tries to apply the player's selection on the grid.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns>
        /// true = accept player selection, place or flip token.
        /// false = reject player selection, do not execute require.
        /// </returns>
        public void ApplyPlayerSelection(SxGrid grid)
        {
            // Handle the click event on the grid,
            // based on current game logic.
            if (m_State != eGameState.WaitingForInput)
                throw new Exception($"Cannot apply player selection in state {m_State}. Expected WaitingForInput.");

			if (!TryGetLogicHandler(out var logic))
            {
                throw new System.InvalidOperationException($"No logic handler found for rule: {Rule}");
            }

            var token = m_Turn switch
            {
                eTurn.White => SxToken.CreateWhite(),
                eTurn.Black => SxToken.CreateBlack(),
                _ => throw new System.InvalidOperationException($"Invalid turn: {m_Turn}")
            };

            m_State = eGameState.ValidatingMove;
            try
            {
				if (!logic.IsValidMove(grid, token, out var data, throwError: true))
                {
                    throw new SxRuleException($"{logic.GetType()}, Invalid move, reset state and return");
                }

				logic.ExecuteMove(grid, token, data);
			}
            catch (SxRuleException ex)
            {
				m_State = eGameState.WaitingForInput;
                throw ex;
			}
            catch (System.Exception ex)
            {
                m_State = eGameState.WaitingForInput;
                throw new System.Exception($"{logic.GetType()}, Error while validating move: {ex.Message}\n{ex.StackTrace}");
			}


            // After executing the move, change the turn
            SxLog.Info($"{logic.GetType()} Valid move {grid.ReadableId}, validate completed.");
			// m_State = eGameState.WaitingForNPC;
			m_State = eGameState.WaitingForInput;
			EndTurn();
        }

        private void EndTurn()
        {
            // End the current turn and switch to the next turn
            var org = m_Turn;
			m_Turn = m_Turn switch
            {
                eTurn.White => eTurn.Black,
                eTurn.Black => eTurn.White,
                _ => throw new System.InvalidOperationException($"Invalid turn: {m_Turn}")
            };
            if (m_Turn == eTurn.None)
            {
                SxLog.Error($"Cannot end turn, turn is {org} -> {m_Turn}. Game might be over or not started.");
                return;
			}
			SxLog.Info($"Turn switch to {m_Turn}");
        }


		#region Handle Tasks
        private List<SxTaskBase> m_Tasks = new List<SxTaskBase>(8);
        private List<int> m_MarkDel = new List<int>(8);
		private void HandleTasks()
        {
            // Handle tasks related to the game board
            // This could include updating the board state, checking for win conditions, etc.
            if (m_Tasks == null || m_Tasks.Count == 0)
            {
                return;
            }
            
            m_MarkDel.Clear();
            var cnt = m_Tasks.Count;
            for (int i = 0; i < cnt; ++i)
            {
                var task = m_Tasks[i];
                if (task is null || (task is SxTask t0 && t0.isDisposed))
                {
                    // early return exception cases.
                    m_MarkDel.Add(i);
                    continue;
                }

                try
                {
                    // Execute the task and check if it is finished
                    // true = task is still running
                    // false = task is finished
                    var completed = !task.Execute();
					if (completed)
                    {
						if (task is SxTask t1)
                        {
							// internal task is finished, dispose it.
							t1.Abort();
                        }
                        m_MarkDel.Add(i);
					}
				}
                catch (System.Exception ex)
                {
                    SxLog.Error($"Error while handling task: {ex.Message}\n{ex.StackTrace}");
                    m_MarkDel.Add(i);
                    continue;
				}
			}

            if (m_MarkDel.Count > 0)
            {
				// remove aborted tasks FILO
				var i = m_MarkDel.Count;
                while (i-- > 0)
                {
                    var index = m_MarkDel[i];
                    m_Tasks.RemoveAt(index);
                }
            }
            m_MarkDel.Clear();
		}

        internal void AddTask(SxTaskBase task)
        {
            if (task == null)
            {
                SxLog.Error("Cannot add a null task to the board.");
                return;
            }
            m_Tasks.Add(task);
		}
        internal void CleanUpTasks()
        {
            if (m_Tasks == null)
                return;

            var i = m_Tasks.Count;
            while (i-- > 0)
            {
                if (m_Tasks[i] == null)
                {
                    continue;
                }

                if (m_Tasks[i] is SxTask task)
                {
                    try
                    {
                        task.Abort();
                    }
                    catch(System.Exception ex)
                    {
                        SxLog.Error($"Error while handling task: {ex.Message}\n{ex.StackTrace}");
					}
                }

                // optimize: delay clean up.
                // m_Tasks.RemoveAt(i);
			}
			m_Tasks.Clear();
		}
		#endregion Handle Tasks
	}
}