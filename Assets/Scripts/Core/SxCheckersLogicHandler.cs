using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slash.Core
{
	/// <summary>
	/// <see cref="eGameRule.Checkers"/>
	/// <see cref="https://en.wikipedia.org/wiki/Checkers"/>"/>
	/// Checkers is a strategy board game for two players, played on a square board with 64 squares.
	/// The objective is to capture all of the opponent's pieces or block them so they cannot move.
	/// </summary>
	public class SxCheckersLogicHandler : SxLogicHandler
	{
		public SxCheckersLogicHandler(SxBoard board) : base(board) { }

		protected override void OnInitBoard(SxBoard board)
		{
			// Initialize the board with the starting positions for Reversi
			board.GetGrid("A2").Link(SxToken.CreateBlack());
			board.GetGrid("A4").Link(SxToken.CreateBlack());
			board.GetGrid("A6").Link(SxToken.CreateBlack());
			board.GetGrid("A8").Link(SxToken.CreateBlack());
			board.GetGrid("B1").Link(SxToken.CreateBlack());
			board.GetGrid("B3").Link(SxToken.CreateBlack());
			board.GetGrid("B5").Link(SxToken.CreateBlack());
			board.GetGrid("B7").Link(SxToken.CreateBlack());

			board.GetGrid("G2").Link(SxToken.CreateWhite());
			board.GetGrid("G4").Link(SxToken.CreateWhite());
			board.GetGrid("G6").Link(SxToken.CreateWhite());
			board.GetGrid("G8").Link(SxToken.CreateWhite());
			board.GetGrid("H1").Link(SxToken.CreateWhite());
			board.GetGrid("H3").Link(SxToken.CreateWhite());
			board.GetGrid("H5").Link(SxToken.CreateWhite());
			board.GetGrid("H7").Link(SxToken.CreateWhite());
		}

		public SxCoord GetValidDirection(eTurn turn)
		{
			// Check if the turn is valid
			switch (turn)
			{
				case eTurn.Black:
					return new SxCoord(0, 1); // Black moves down-left or down-right
				case eTurn.White:
					return new SxCoord(0, -1); // White moves up-left or up-right
				default:
					SxLog.Error($"Invalid turn: {turn}");
					return default;
			}
		}

		protected override void OnChangeMode(SxBoard board)
		{
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

		public class NormalMoveInfo
		{
			public SxGrid grid;
			public SxToken token;
		}

		public class JumpMoveInfo : NormalMoveInfo
		{
			public SxGrid eatGrid; // The grid where the opponent's token is eaten
		}

		public override bool IsValidMove(SxGrid grid, SxToken token, out object data, bool throwError = false)
		{
			// Check if the grid and token are valid
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
				return _RuleError($"Invalid turn. Expected {Board.Turn}, but attempt to place {token}.");
			}

			// rules for checkers
			// 1) move the existing token, (Cannot place a new token)
			// 2.1) the token can jump over an opponent's token to an empty square
			// 2.2) can only capture an opponent's token by jumping over it to an empty square
			// 3) if a token reaches the opposite end of the board, it becomes a king and can move both forward and backward
			if (token.GetGrid() == null)
			{
				return _RuleError($"Cannot place any new {token}, under checkers rules.\nCan only move it.");
			}
			else if (token.GetGrid() == grid)
			{
				return _RuleError($"Token {token} is already on grid {grid.ReadableId}. Cannot move it to the same grid.");
			}

			if (grid.HasToken())
			{
				return _RuleError($"Grid {grid.ReadableId} already has a token. Cannot move token to there.");
			}
			// else, the grid is empty, and we can check the move cases
			var dir = GetValidDirection(Board.Turn);
			var dirRef = dir.y > 0;
			var v = grid.coord - token.GetGrid().coord;
			var absMatched = System.Math.Abs(v.x) == System.Math.Abs(v.y);
			var moveFwd = v.y > 0;
			if (System.Math.Abs(v.x) == 1 && absMatched)
			{
				// normal move, one square diagonally forward
				// check last move was a valid jump
				bool aligned = moveFwd == dirRef;
				if (!aligned && !token.isKing)
				{
					return _RuleError($"Invalid move from {token.GetGrid().ReadableId} to {grid.ReadableId}.\nCheckers only allows forward moves for normal tokens.");
				}
				// Valid move, one square diagonally forward
				data = new NormalMoveInfo
				{
					grid = grid,
					token = token,
				};
				return true;
			}
			else if (System.Math.Abs(v.x) == 2 && absMatched)
			{
				var eatCoord = token.GetGrid().coord + new SxCoord(v.x / 2, v.y / 2);
				if (!Board.TryGetGrid(eatCoord, out var eatGrid))
				{
					return _RuleError($"Invalid move from {token.GetGrid().ReadableId} to {grid.ReadableId}.\nCannot jump over an empty square at {eatCoord.ReadableId}.");
				}
				else if (!eatGrid.HasToken())
				{
					return _RuleError($"Invalid move from {token.GetGrid().ReadableId} to {grid.ReadableId}.\nCannot jump over an empty square at {eatGrid.ReadableId}.");
				}

				// Check if the token to be eaten is from the opponent
				if (eatGrid.token.GetTurn() == token.GetTurn())
				{
					return _RuleError($"Invalid move from {token.GetGrid().ReadableId} to {grid.ReadableId}.\nCannot jump over your own token at {eatGrid.ReadableId}.");
				}

				var aligned = moveFwd == dirRef;
				if (!aligned && !token.isKing)
				{
					return _RuleError($"Invalid move from {token.GetGrid().ReadableId} to {grid.ReadableId}.\nCheckers only allows forward moves for normal tokens.");
				}
				
				// Valid move, one square diagonally forward
				data = new JumpMoveInfo
				{
					grid = grid,
					token = token,
					eatGrid = eatGrid
				};
				return true;
			}
			else
			{
   				return _RuleError($"Invalid move from {token.GetGrid().ReadableId} to {grid.ReadableId}.\nCheckers only allows diagonal moves.");
			}
		}

		public override void ExecuteMove(SxGrid grid, SxToken token, object data)
		{
			if (data is not NormalMoveInfo normalMoveInfo)
			{
				SxLog.Error("Invalid data for normal move execution.");
				return;
			}

			if (data is JumpMoveInfo jumpMoveInfo)
			{
				// Remove the eaten token
				if (jumpMoveInfo.eatGrid == null || !jumpMoveInfo.eatGrid.HasToken())
				{
					SxLog.Error("Invalid jump move execution: eatGrid is null or does not have a token.");
					return;
				}

				jumpMoveInfo.eatGrid.token.Dispose();
				Board.AddScore(token.GetTurn(), 1);
			}
		}

	}
}