using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slash.Core
{

	/// <summary>
	/// <see cref="eGameRule.Reversi"/>
	/// <see cref="https://en.wikipedia.org/wiki/Reversi"/>
	/// (also known as Othello) is a pretty simple game.
	/// It consists of a 8x8 square board, and pieces with one black and one white side.
	/// </summary>
	public class SxReversiLogicHandler : SxLogicHandler
	{
		public SxReversiLogicHandler(SxBoard board) : base(board)
		{}

		protected override void OnInitBoard(SxBoard board)
		{
			// Initialize the board with the starting positions for Reversi
			board.GetGrid("D4").Link(SxToken.CreateWhite());
			board.GetGrid("D5").Link(SxToken.CreateBlack());
			board.GetGrid("E4").Link(SxToken.CreateBlack());
			board.GetGrid("E5").Link(SxToken.CreateWhite());
		}


		protected override void OnChangeMode(SxBoard board)
		{
			// TODO: validate the board state for Reversi rules
			// handle invalid cases from other game modes
			if (board == null)
			{
				SxLog.Error("Board cannot be null.");
				return;
			}
			throw new System.NotImplementedException();
		}

		public override void SolveConflict()
		{
			Board.ChangeState(this, eGameState.SolveConflict);

			Board.ChangeState(this, eGameState.WaitingForInput);
			throw new System.NotImplementedException();
		}

		public override bool IsValidMove(SxGrid grid, SxToken token, out object data, bool throwError = false)
		{
			bool _RuleError(string message)
			{
				var ex = new SxRuleException(message);
				if (throwError)
					throw ex;
				SxLog.Error(ex);
				return false;
			}

			data = null;
			// rules for Reversi:
			// 1) the grid must be empty
			// 2) the move must be adjacent to an existing token of the different color
			// 3) at least flip one token of the opposite color
			if (grid == null || token == null)
			{
				return _RuleError("Invalid grid or token provided.");
			}

			// Check if the grid is empty
			//if (grid.HasToken())
			//{
			//	SxLog.Error($"Grid {grid.ReadableId} already has a token. Cannot place a new token here.");
			//	return false;
			//}

			// Check if the token is valid
			if (Board.Turn != token.GetTurn())
			{
				return _RuleError($"Invalid turn. Expected {Board.Turn}, but attempt to place {token.GetTurn()}.");
			}

			var anchor = grid.coord;
			var tokenTurn = token.GetTurn();
			var flipList = new List<SxGrid>(8); // Initialize the flip list
			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"Anchor {grid.ReadableId}:{anchor}");
			foreach (var g in GetAdjacentGrids(grid))
			{
				if (g == null)
				{
					throw new System.Exception("Adjacent grid is null. This should not happen.");
				}

				var direction = g.coord - anchor;
				sb.Append($"{g.ReadableId} direction:({direction})");
				if (!g.HasToken())
				{
					sb.AppendLine(" no token to flip");
					continue;
				}

				// ignore same color tokens
				if (g.token.GetTurn() == tokenTurn)
				{
					sb.AppendLine(" same team, ignore.");
					continue;
				}

				try
				{
					// try flip to the end, based on vector direction.
					if (!CanFlip(g, token, direction, out var flipRows))
					{
						sb.AppendLine(" cannot flipped.");
						continue; // ignore grids that cannot be flip
					}

					sb.AppendLine($" valid row found {flipRows.Count}.");
					flipList.AddRange(flipRows);
				}
				catch (System.Exception ex)
				{
					if (throwError)
						throw ex;
					SxLog.Error($"Logic Error: {ex.Message}\n{sb.ToString()}");
					continue;
				}
			}

			data = flipList; // return the list of grids that can be flip
			if (flipList.Count == 0 && throwError)
			{
				throw new SxRuleException($"Invalid : Unable to flip on all direction.\n{sb.ToString()}");
			}
			SxLog.Info($"Valid move {anchor}, \n{sb.ToString()}");
			return flipList.Count > 0;
		}

		public override void ExecuteMove(SxGrid grid, SxToken token, object data)
		{
			if (data == null || !(data is List<SxGrid> flipList))
			{
				SxLog.Error("Invalid data provided for executing move.");
				return;
			}

			if (flipList.Count == 0)
			{
				SxLog.Error("No tokens to flip. This should not happen if the move is valid.");
				return;
			}

			grid.Link(token); // Place the token on the grid
			for (int i = 0; i < flipList.Count; i++)
			{
				var flipGrid = flipList[i];
				if (flipGrid == null)
				{
					SxLog.Error($"flip grid at index {i} is null. This should not happen.");
					continue;
				}
				if (!flipGrid.HasToken())
				{
					SxLog.Error($"flip grid {flipGrid.ReadableId} does not have a token to flip. This should not happen.");
					continue;
				}

				flipGrid.token.Flip();
			}
		}

		private bool CanFlip(SxGrid anchor, SxToken token, SxCoord dir, out List<SxGrid> flipList)
		{
			flipList = new List<SxGrid>(8); // Initialize if not provided

			if (anchor == null || token == null || dir == null)
			{
				throw new System.Exception("Invalid anchor, token, or direction provided.");
			}

			//if (anchor.HasToken())
			//{
			//	SxLog.Error($"Anchor grid {anchor.ReadableId} already has a token. Cannot place a new token here.");
			//	return false;
			//}

			var tokenTurn = token.GetTurn();
			var current = anchor.coord;
			// Check if the first grid in the direction has a token
			{
				if (!Board.TryGetGrid(current, out var firstGrid))
				{
					// throw new System.Exception($"Logic Error, grid {current.ReadableId} does not exist on the board.");
					return false;
				}

				if (!firstGrid.HasToken())
				{
					SxLog.Warning($"Grid {current.ReadableId} does not have a token to flip.");
					return false;
				}

				if (firstGrid.token.GetTurn() == tokenTurn)
				{
					SxLog.Warning($"Grid {current.ReadableId} has a token of the same color as the current token {tokenTurn}. Cannot flip.");
					return false;
				}

				flipList.Add(firstGrid);
				current += dir; // move in the direction
			}

			// Move in the direction until we find a token of the same color or reach the end of the board
			while (Board.TryGetGrid(current, out var grid) && grid.HasToken())
			{
				var isOpponent = grid.token.GetTurn() != tokenTurn;
				if (!isOpponent)
				{
					// We found a token of the same color, can flip
					return true;
				}
				flipList.Add(grid); // add the opponent's token to the flip list
				current += dir; // move in the direction
			}

			// If we reach here, it means we didn't find a token of the same color in the direction
			flipList.Clear();
			return false;
		}

		/// <summary>
		/// Retrieves all adjacent grids around the specified grid.
		/// based on order: LB, L, LT, T, RT, R, RB, B
		/// </summary>
		/// <param name="grid"></param>
		/// <returns></returns>
		private IEnumerable<SxGrid> GetAdjacentGrids(SxGrid grid)
		{
			if (grid == null)
			{
				SxLog.Error("Invalid grid or token provided.");
				yield break;
			}

			var anchor = grid.coord;
			var arr = new SxGrid[8]; // LB, L, LT, T, RT, R, RB, B
			var obj = default(SxGrid);
			if (Board.TryGetGrid(anchor.x - 1, anchor.y + 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x - 1, anchor.y + 0, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x - 1, anchor.y - 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 0, anchor.y - 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 1, anchor.y - 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 1, anchor.y + 0, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 1, anchor.y + 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 0, anchor.y + 1, out obj)) yield return obj;
		}
	}
}