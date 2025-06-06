using System.Collections;
using System.Collections.Generic;
namespace Slash.Core
{
    public abstract class SxLogicHandler
    {
        public SxBoard Board { get; private set; }

		public SxLogicHandler(SxBoard board)
		{
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

		public abstract bool IsValidMove(SxGrid grid, SxToken token, out object data, bool throwError = false);

		public abstract void ExecuteMove(SxGrid grid, SxToken token, object data);

		#region Utilities
		public bool TryGetGrid(int x, int y, out SxGrid grid) => Board.TryGetGrid(x, y, out grid);

		public bool TryPlaceToken(SxGrid grid, eTurn turn)
		{
			return TryPlaceToken(grid.coord, turn);
		}

		public bool TryPlaceToken(SxCoord coord, eTurn turn)
		{
			return TryPlaceToken(coord.x, coord.y, turn);
		}

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
			grid.Link(token);
			return true;
		}
		#endregion Utilities
	}


	//public class  SxGobangLogicHandler : SxLogicHandler
	//{	
	//}

 //   public class SxCheckersLogicHandler : SxLogicHandler
 //   {
	//}
}