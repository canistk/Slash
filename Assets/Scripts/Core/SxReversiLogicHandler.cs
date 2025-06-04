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
			board.GetGrid("D4").SetToken(SxToken.CreateWhite());
			board.GetGrid("D5").SetToken(SxToken.CreateBlack());
			board.GetGrid("E4").SetToken(SxToken.CreateBlack());
			board.GetGrid("E5").SetToken(SxToken.CreateWhite());
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

		public override bool IsValidMove(SxGrid grid, SxToken token, out object data)
		{
			data = null;
			// rules for Reversi:
			// 1) the grid must be empty
			// 2) the move must be adjacent to an existing token of the different color
			// 3) at least eat one token of the opposite color
			if (grid == null || token == null)
			{
				SxLog.Error("Invalid grid or token provided.");
				return false;
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
				SxLog.Error($"Invalid turn. Expected {Board.Turn}, but attempt to place {token.GetTurn()}.");
				return false;
			}

			var anchor = grid.coord;
			var tokenTurn = token.GetTurn();
			foreach (var g in GetAdjacentGrids(grid))
			{
				if (g == null)
					throw new System.Exception("Adjacent grid is null. This should not happen.");
				if (!g.HasToken())
					continue;

				// ignore same color tokens
				if (g.token.GetTurn() == tokenTurn)
					continue;

				var vector = g.coord - anchor;
				// ignore grids that cannot be eaten
				if (!CanEat(g, token, vector, out var eatList))
					continue;

				data = eatList; // return the list of grids that can be eaten
				return true;
			}
			return false;
		}

		public override void ExecuteMove(SxGrid grid, SxToken token, object data)
		{
			if (data == null || !(data is List<SxGrid> eatList))
			{
				SxLog.Error("Invalid data provided for executing move.");
				return;
			}

			if (eatList.Count == 0)
			{
				SxLog.Error("No tokens to eat. This should not happen if the move is valid.");
				return;
			}

			grid.SetToken(token); // Place the token on the grid
			for (int i = 0; i < eatList.Count; i++)
			{
				var eatGrid = eatList[i];
				if (eatGrid == null)
				{
					SxLog.Error($"Eat grid at index {i} is null. This should not happen.");
					continue;
				}
				if (!eatGrid.HasToken())
				{
					SxLog.Error($"Eat grid {eatGrid.ReadableId} does not have a token to eat. This should not happen.");
					continue;
				}
				eatGrid.ClearToken(); // Remove the token from the grid
			}
		}

		private bool CanEat(SxGrid anchor, SxToken token, SxCoord dir, out List<SxGrid> eatList)
		{
			eatList = new List<SxGrid>(8); // Initialize if not provided

			if (anchor == null || token == null || dir == null)
			{
				SxLog.Error("Invalid anchor, token, or direction provided.");
				return false;
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
					SxLog.Error($"Logic Error, grid {current.ReadableId} does not exist on the board.");
					return false;
				}

				if (!firstGrid.HasToken())
				{
					SxLog.Error($"Grid {current.ReadableId} does not have a token to eat.");
					return false;
				}

				if (firstGrid.token.GetTurn() == tokenTurn)
				{
					SxLog.Error($"Grid {current.ReadableId} has a token of the same color as the current token {tokenTurn}. Cannot eat.");
					return false;
				}

				eatList.Add(firstGrid);
				current += dir; // move in the direction
			}

			// Move in the direction until we find a token of the same color or reach the end of the board
			while (Board.TryGetGrid(current, out var grid) && grid.HasToken())
			{
				var isOpponent = grid.token.GetTurn() != tokenTurn;
				if (!isOpponent)
				{
					// We found a token of the same color, can eat
					return true;
				}
				eatList.Add(grid); // add the opponent's token to the eat list
				current += dir; // move in the direction
			}

			// If we reach here, it means we didn't find a token of the same color in the direction
			eatList.Clear();
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