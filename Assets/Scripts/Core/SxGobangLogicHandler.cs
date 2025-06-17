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

		protected override void OnChangeMode(eGameRule prev, SxBoard board)
		{
		}

		public override void SolveConflict()
		{
			Board.ChangeState(this, eGameState.SolveConflict);
			for (int x = 0; x < Board.Width; ++x)
			{
				for (int y = 0; y < Board.Height; ++y)
				{
					if (!Board.TryGetGrid(x, y, out var grid))
						continue;
					if (!grid.HasToken())
						continue;
				}
			}
			Board.ChangeState(this, eGameState.WaitingForInput);
			throw new System.NotImplementedException();
		}

		public override bool IsValidMove(SxGrid grid, out object data, bool throwError = false)
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
			if (grid == null)
			{
				return _RuleError("Invalid grid or token provided.");
			}

			if (grid.HasToken())
			{
				return _RuleError($"Grid {grid.ReadableId} already has a token. Cannot place a new token here.");
			}

			// Rules for Gobang:
			// 1. The grid must be empty
			// 2. The game is played until one player gets five in a row, so we don't need to check for specific patterns here.

			// check on all directions for five in a row
			// 1) head to the left most grid { LB, L, LT, T }, 
			// 2) start counting from the left most grid to the right most grid { RT, R, RB, B }
			var anchor = grid.coord;
			var d2h = new SxCoord[4]
			{
				new SxCoord(-1, +1),	// LB
				new SxCoord(-1, +0),	// L
				new SxCoord(-1, -1),	// LT
				new SxCoord(+0, -1),	// T
			};
			var d2t = new SxCoord[4]
			{
				new SxCoord(+1, -1),	// RT
				new SxCoord(+1, +0),	// R
				new SxCoord(+1, +1),	// RB
				new SxCoord(+0, +1),	// B
			};
			var headToken = new SxCoord[4] { anchor, anchor, anchor, anchor };
			var tokenCnts = new int[4]; // To count tokens in each direction
			var tokenTurn = Board.Turn;
			// Check direction, for the farthest grid,
			// which had same token color, cache it into headToken.
			bool _IsValid(SxGrid _grid)
			{
				if (_grid.coord == grid.coord)
					return true;
				if (!_grid.HasToken())
					return false;
				return _grid.token.GetTurn() == tokenTurn;
			}
			for (int i = 0; i < d2h.Length; ++i)
			{
				var pt = grid;
				var next = default(SxCoord);
				do
				{
					headToken[i] = pt.coord; // Default to anchor if no valid head token found
					next = pt.coord + d2h[i];
					++tokenCnts[i]; // inclusive
				}
				while (Board.TryGetGrid(next.x, next.y, out pt) &&
					_IsValid(pt));

				pt = grid;
				next = anchor + d2t[i];
				Debug.Log($"Checking {pt.ReadableId}, {next.ReadableId}: dir={d2t[i]}");
				while (Board.TryGetGrid(next.x, next.y, out pt) &&
					_IsValid(pt))
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
				if (cnt != 5)
				{
					// GD note : more then 5 will not be eaten. (no score)
					continue;
				}
				// Assume headToken[i] is Left most grid in the direction (d2h[i]),
				// already heading to the right most grid (d2t[i]).
				for (int k = 0; k < cnt; ++k)
				{
					var coord = headToken[i] + (d2t[i] * k);
					var isPlaceing = coord == anchor;
					if (isPlaceing)
					{
						// assume execute will handle placing before eat.
						eatGrids.Add(grid);
						continue;
					}

					if (!Board.TryGetGrid(coord, out var eatGrid))
						continue; // out of board.
					if (!eatGrid.HasToken())
						throw new System.Exception($"Grid {eatGrid.ReadableId} does not have a token to eat while checking for five in a row.");

					if (eatGrid.token.GetTurn() != tokenTurn)
						throw new System.Exception($"Grid {eatGrid.ReadableId} has a token of different Color({eatGrid.token.GetTurn()}), Expected ({tokenTurn}).");
					eatGrids.Add(eatGrid);
				}
			}

			var totalScore = eatGrids.Count;
			// calculate score based on the number of tokens eaten
			data = new PlaceInfo()
			{
				placeGrid = grid,
				eatGrids = eatGrids
			};
			return true;
		}

		private class PlaceInfo
		{
			public SxGrid placeGrid;
			public List<SxGrid> eatGrids;
		}

		public override void ExecuteMove(SxGrid grid, object data)
		{
			if (grid == null)
			{
				throw new System.Exception("Invalid grid or token provided for execution.");
			}

			if (data is not PlaceInfo info)
			{
				throw new System.Exception("Data does not match the grid provided for execution.");
			}

			var token = Board.Turn == eTurn.White ? SxToken.CreateWhite() : SxToken.CreateBlack();
			// Assumeing the grid already has the target token placed.
			if (grid.HasToken())
			{
				throw new System.Exception($"Grid {grid.ReadableId} have the token aready. Expected {token}, but found {grid.token}.");
			}

			if (!TryPlaceToken(grid, token)) // Place the token on the grid
			{
				throw new System.Exception($"Failed to place token({token.GetTurn()}) on grid {grid.ReadableId}.");
			}

			// Handle the logic for eating tokens if any
			for (int i = 0; i < info.eatGrids.Count; ++i)
			{
				var eatGrid = info.eatGrids[i];
				if (eatGrid == null)
				{
					SxLog.Error($"eat grid at index {i} is null. This should not happen.");
					continue;
				}
				if (!eatGrid.HasToken())
				{
					SxLog.Error($"eat grid {eatGrid.ReadableId} does not have a token to eat. This should not happen.");
					continue;
				}
				eatGrid.token.Dispose();
			}
		}
	}
}