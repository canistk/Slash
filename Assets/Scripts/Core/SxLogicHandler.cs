using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slash.Core
{
    public abstract class SxLogicHandler
    {
        public SxBoard Board { get; private set; }
        public void InitBoard(SxBoard board)
        {
            if (board == null)
            {
                SxLog.Error("Board cannot be null.");
                return;
            }
            this.Board = board;
            OnInitBoard(board);
        }
		protected abstract void OnInitBoard(SxBoard board);


        public void ChangeMode(SxBoard board)
        {
            if (board == null)
            {
                SxLog.Error("Board cannot be null.");
                return;
            }
            this.Board = board;
            OnChangeMode(board);
		}
		protected abstract void OnChangeMode(SxBoard board);

		public abstract void SolveConflict();

		public bool TryGetGrid(int x, int y, out SxGrid grid) => Board.TryGetGrid(x, y, out grid);


		public bool TryPlaceToken(int x, int y, eTurn turn)
		{
			if (!Board.TryGetGrid(x, y, out var grid))
				return false;
			if (grid.HasToken())
				return false;
			var newToken = turn == eTurn.Black ? SxToken.CreateBlack() : SxToken.CreateWhite();
			return TryPlaceToken(grid, newToken);
		}

		public bool TryPlaceToken(string id, eTurn turn)
		{
			if (!Board.TryGetGrid(id, out var grid))
				return false;
			if (grid.HasToken())
				return false;

			var newToken = turn == eTurn.Black ? SxToken.CreateBlack() : SxToken.CreateWhite();
			return TryPlaceToken(grid, newToken);
		}

		public bool TryPlaceToken(SxGrid grid, SxToken token)
		{
			if (grid == null || token == null)
			{
				SxLog.Error("Invalid grid or token provided.");
				return false;
			}
			if (grid.HasToken())
			{
				SxLog.Warning($"Grid {grid.ReadableId} already has a token. Cannot place a new token here.");
				return false;
			}
			grid.SetToken(token);
			return true;
		}
	}

    /// <summary>
    /// <see cref="eGameRule.Reversi"/>
    /// </summary>
    public class SxReversiLogicHandler : SxLogicHandler
    {
		protected override void OnInitBoard(SxBoard board)
        {
            if (board == null)
            {
                SxLog.Error("Board cannot be null.");
                return;
            }
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

		}

		public override void SolveConflict()
		{
			Board.ChangeState(this, eGameState.SolveConflict);

			Board.ChangeState(this, eGameState.WaitingForInput);
		}
	}

	//public class  SxGobangLogicHandler : SxLogicHandler
	//{	
	//}

 //   public class SxCheckersLogicHandler : SxLogicHandler
 //   {
	//}
}