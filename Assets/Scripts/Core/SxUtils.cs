using System.Collections;
using System.Collections.Generic;
namespace Slash.Core
{
	/// <summary>
	/// GDD for Slash
	///  <see cref="https://docs.google.com/presentation/d/1gtWlXeyuOhin_iXzKzpWtwurvbSqCe486Cq9LbfW67M/edit?slide=id.g35b01949cc8_0_63#slide=id.g35b01949cc8_0_63"/>
	/// </summary>
	public static class SxUtils
    {
    }

	public static class SxLog
	{
		public static void Error(string message)
		{
			UnityEngine.Debug.LogError(message);
		}
		public static void Warning(string message)
		{
			UnityEngine.Debug.LogWarning(message);
		}
		public static void Info(string message)
		{
			UnityEngine.Debug.Log(message);
		}
	}

	public enum eDirection
	{
		Left = 0,
		Up = 1,
		Right = 2,
		Down = 3
	}

	public enum eGameRule
	{
		None = 0,

		/// <see cref="https://en.wikipedia.org/wiki/Reversi"/>
		/// (also known as Othello) is a pretty simple game.
		/// It consists of a 8x8 square board, and pieces with one black and one white side.
		Reversi,    

		/// <see cref="https://en.wikipedia.org/wiki/Gomoku"/>"/>
		/// (also known as Five in a Row) is a game played on a square board,
		/// where players take turns placing their pieces on the board.
		/// The goal is to get five pieces in a row, either horizontally, vertically, or diagonally.
		Gobang,
		
		/// <see cref="https://en.wikipedia.org/wiki/Checkers"/>
		/// (also known as Draughts) is a game played on a square board, where players take turns moving their pieces diagonally.
		/// The goal is to capture all of the opponent's pieces.
		checkers,   
	}

}