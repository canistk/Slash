using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Slash.Core
{
	/// <summary>
	/// <see cref="eGameRule.Gobang"/>
	/// <see cref="https://en.wikipedia.org/wiki/Gomoku"/>"/>
	/// (also known as Five in a Row) is a game played on a square board,
	/// where players take turns placing their pieces on the board.
	/// The goal is to get five pieces in a row, either horizontally, vertically, or diagonally.
	/// </summary>
	public class SxGobangLogicHandler : SxLogicHandler
	{
		public SxGobangLogicHandler(SxBoard board) : base(board) { }

		protected override void OnInitBoard(SxBoard board)
		{
			// Initialize the board with an empty state for Gobang
		}

		protected override void OnChangeMode(SxBoard board)
		{
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
			if (grid == null || token == null)
			{
				return _RuleError("Invalid grid or token provided.");
			}

			// Check if the token is valid
			if (Board.Turn != token.GetTurn())
			{
				return _RuleError($"Invalid turn. Expected {Board.Turn}, but attempt to place {token.GetTurn()}.");
			}

			if (grid.HasToken())
			{
				return _RuleError($"Grid {grid.ReadableId} already has a token. Cannot place a new token here.");
			}

			// Rules for Gobang:
			// 1. The grid must be empty
			// 2. The game is played until one player gets five in a row, so we don't need to check for specific patterns here.

			data = grid;
			return true;
		}

		public override void ExecuteMove(SxGrid grid, SxToken token, object data)
		{
			if (grid == null || token == null)
			{
				throw new System.Exception("Invalid grid or token provided for execution.");
			}

			if (data != grid)
			{
				throw new System.Exception("Data does not match the grid provided for execution.");
			}

			// Assumeing the grid already has the target token placed.
			if (!grid.HasToken() || grid.token != token)
			{
				throw new System.Exception($"Grid {grid.ReadableId} does not have the token to place. Expected {token}, but found {grid.token}.");
			}

			if (!TryPlaceToken(grid, token)) // Place the token on the grid
			{
				throw new System.Exception($"Failed to place token({token.GetTurn()}) on grid {grid.ReadableId}.");
			}

			// check on all directions for five in a row
			// 1) head to the left most grid { LB, L, LT, T }, 
			// 2) start counting from the left most grid to the right most grid { RT, R, RB, B }
			var anchor = grid.coord;
			var d2h = new SxCoord[4]
			{
				anchor + new SxCoord(-1, +1),	// LB
				anchor + new SxCoord(-1, +0),	// L
				anchor + new SxCoord(-1, -1),	// LT
				anchor + new SxCoord(+0, -1),	// T
			};
			var d2t = new SxCoord[4]
			{
				anchor + new SxCoord(+1, -1),	// RT
				anchor + new SxCoord(+1, +0),	// R
				anchor + new SxCoord(+1, +1),	// RB
				anchor + new SxCoord(+0, +1),	// B
			};
			var headToken = new SxCoord[4];
			var tokenCnts = new int[4]; // To count tokens in each direction
			var tokenTurn = token.GetTurn();
			// Check direction, for the farthest grid,
			// which had same token color, cache it into headToken.
			for (int i = 0; i < d2h.Length; ++i)
			{
				var pt		= grid;
				var next	= default(SxCoord);
				do
				{
					headToken[i] = pt.coord; // Default to anchor if no valid head token found
					next = pt.coord + d2h[i];
					++tokenCnts[i]; // inclusive
				}
				while (Board.TryGetGrid(next.x, next.y, out pt) &&
					pt.HasToken() &&
					pt.token.GetTurn() == tokenTurn);

				pt		= grid;
				next	= anchor + d2t[i];
				while (Board.TryGetGrid(next.x, next.y, out pt) &&
					pt.HasToken() &&
					pt.token.GetTurn() == tokenTurn)
				{
					next = pt.coord + d2t[i];
					++tokenCnts[i]; // exclusive
				}
			}

			// check if any direction has five tokens in a row
			var eatGrids = new List<SxGrid>(8);
			for (int i = 0; i < headToken.Length; ++i)
			{
				var cnt = tokenCnts[i];
				if (cnt < 5)
					continue;

				for (int k = 0; k < cnt; ++k)
				{
					var coord = headToken[i] + (d2h[i] * k);
					if (!Board.TryGetGrid(coord, out var eatGrid))
						throw new System.Exception($"Invalid grid coordinate {coord} while checking for five in a row.");

					if (!eatGrid.HasToken())
						throw new System.Exception($"Grid {eatGrid.ReadableId} does not have a token to eat while checking for five in a row.");

					if (eatGrid.token.GetTurn() != tokenTurn)
						throw new System.Exception($"Grid {eatGrid.ReadableId} has a token of different Color({eatGrid.token.GetTurn()}), Expected ({tokenTurn}).");

					eatGrids.Add(eatGrid);
				}
			}

			var totalScore = eatGrids.Count;
			// TODO: calculate score based on the number of tokens eaten

			for (int i = 0; i < eatGrids.Count; ++i)
			{
				if (eatGrids[i] == null)
				{
					SxLog.Error($"eat grid at index {i} is null. This should not happen.");
					continue;
				}
				if (!eatGrids[i].HasToken())
				{
					SxLog.Error($"eat grid {eatGrids[i].ReadableId} does not have a token to eat. This should not happen.");
					continue;
				}
				eatGrids[i].token.Dispose();
			}
		}
	}
}