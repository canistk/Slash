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
	public class SetupException : System.Exception
	{
		public SetupException(string msg, System.Exception innerException) : base(msg, innerException) { }
		public SetupException(string msg) : base(msg) { }

		public SetupException() : base() { }
	}
	public class SxException : System.Exception
	{
		public SxException(string msg, System.Exception innerException) : base(msg, innerException) { }
		public SxException(string msg) : base(msg) { }

		public SxException() : base() { }
	}

	public class SxRuleException : SxException
	{
		public SxRuleException(string msg, System.Exception innerException) : base(msg, innerException) { }
		public SxRuleException(string msg) : base(msg) { }
		public SxRuleException() : base() { }
	}

	public static class SxLog
	{
		public static void Error(System.Exception exception, UnityEngine.Object obj = null)
		{
			if (exception == null)
			{
				Error("Exception is null.", obj);
				return;
			}
			var sb = new System.Text.StringBuilder();
			sb.AppendLine(exception.Message);
			sb.AppendLine("Stack Trace:");
			sb.AppendLine(exception.StackTrace);

			while (exception.InnerException != null)
			{
				exception = exception.InnerException;
				sb.AppendLine("------\nInner Exception:");
				sb.AppendLine(exception.Message);
			}
			UnityEngine.Debug.LogError(sb.ToString(), obj);
		}
		public static void Error(string message, UnityEngine.Object obj = null)
		{
			UnityEngine.Debug.LogError(message, obj);
		}
		public static void Warning(string message, UnityEngine.Object obj = null)
		{
			UnityEngine.Debug.LogWarning(message, obj);
		}
		public static void Info(string message, UnityEngine.Object obj = null)
		{
			UnityEngine.Debug.Log(message, obj);
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
		Checkers,   
	}

	public enum eTurn
	{
		None = 2,	// No player's turn (e.g., game over or not started)
		White = 1,	// White player's turn
		Black = 2,	// Black player's turn
	}

	public enum eGameState
	{
		None = 0,
		InitBoard,  // Initializing the board
		SolveConflict, // Prepare for change mode, resolve rule's conflicts, ensure the board is ready for the next step

		WaitingForInput, // player / enemy's turn, waiting for input
		ValidatingMove,
		WaitingForNPC,
	}

	public struct SxCoord
	{
		public int x;
		public int y;
		public SxCoord(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
		public override string ToString()
		{
			return $"({x}, {y})";
		}

		public string ReadableId
		{
			get
			{
				var ch = (char)('A' + x);
				return $"{ch}{y + 1}"; // e.g., A1, B2, etc.
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is SxCoord coord)
			{
				return x == coord.x && y == coord.y;
			}
			return false;
		}
		public override int GetHashCode() => (x, y).GetHashCode();

		public static bool operator ==(SxCoord a, SxCoord b) => a.Equals(b);
		public static bool operator !=(SxCoord a, SxCoord b) => !a.Equals(b);
		public static SxCoord operator +(SxCoord a, SxCoord b) => new SxCoord(a.x + b.x, a.y + b.y);
		public static SxCoord operator -(SxCoord a, SxCoord b) => new SxCoord(a.x - b.x, a.y - b.y);
		public static SxCoord operator *(SxCoord a, int scalar) => new SxCoord(a.x * scalar, a.y * scalar);
		public static SxCoord operator *(int scalar, SxCoord a) => new SxCoord(a.x * scalar, a.y * scalar);
	}
}