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

		/// <summary>
		/// Retrieves all adjacent grids around the specified grid.
		/// based on clockwise order: LB, L, LT, T, RT, R, RB, B
		/// </summary>
		/// <param name="grid"></param>
		/// <returns></returns>
		protected IEnumerable<SxGrid> GetAdjacentGrids(SxGrid grid)
		{
			if (grid == null)
			{
				SxLog.Error("Invalid grid or token provided.");
				yield break;
			}

			var anchor = grid.coord;
			var obj = default(SxGrid);
			// Clockwise : LB, L, LT, T, RT, R, RB, B
			if (Board.TryGetGrid(anchor.x - 1, anchor.y + 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x - 1, anchor.y + 0, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x - 1, anchor.y - 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 0, anchor.y - 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 1, anchor.y - 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 1, anchor.y + 0, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 1, anchor.y + 1, out obj)) yield return obj;
			if (Board.TryGetGrid(anchor.x + 0, anchor.y + 1, out obj)) yield return obj;
		}
		#endregion Utilities
	}

 //   public class SxCheckersLogicHandler : SxLogicHandler
 //   {
	//}
}