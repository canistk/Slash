using System.Collections;
using System.Collections.Generic;
namespace Slash.Core
{
    public class SxToken : System.IDisposable
	{
		public static event LinkTokenEvent EVENT_Linked, EVENT_Unlinked;
		public static event System.Action<SxToken> EVENT_Disposed;
		public static event System.Action<SxToken> EVENT_Updated;

		private eTurn m_Turn;
		private bool isDisposed;
		public object UI { get; set; } // For UI data binding, can be used to store any additional data needed for UI representation

		public eTurn GetTurn()
		{
			if (isDisposed)
			{
				throw new System.ObjectDisposedException("SxToken", "Cannot access token after it has been disposed.");
			}
			return m_Turn;
		}
		public bool isWhite
		{
			get
			{
				switch(m_Turn)
				{
					case eTurn.White:
						return true;
					case eTurn.Black:
						return false;
					default:
						throw new System.InvalidOperationException("Token is not initialized with a valid turn.");
				}
			}
			private set
			{
				m_Turn = value ? eTurn.White : eTurn.Black;
			}
		}
		public bool isBlack => !isWhite;

		// Indicates if the token is a king (used in games like Checkers)
		public bool isKing { get; set; } = false;
		public void SetKing(SxLogicHandler handler, bool value)
		{
			if (handler is not SxCheckersLogicHandler)
				throw new System.InvalidOperationException("Only Checkers logic handler can set king status.");
			isKing = value;
			TriggerUpdate();
		}

		public static SxToken CreateWhite()
        {
			return new SxToken { m_Turn = eTurn.White };
        }
        public static SxToken CreateBlack()
        {
            return new SxToken { m_Turn = eTurn.Black };
        }
        public override string ToString()
        {
			var color = isWhite ? "White" : "Black";
			var g = GetGrid();
			if (g != null)
			{
				return $"{color} Token [{g.ReadableId}]";
			}
			// If the token is not linked to any grid, just return the color and type
			return $"{color} Token [New]";
        }

		private void TriggerUpdate()
		{
			try
			{
				EVENT_Updated?.Invoke(this);
			}
			catch (System.Exception ex)
			{
				SxLog.Error($"Error during token update event. \n{ex.Message}");
			}
		}

        public void Flip()
        {
            isWhite = !isWhite;
			TriggerUpdate();
		}

		private SxGrid m_Grid = null;
		internal void Link(SxGrid grid)
		{
			var before = m_Grid;
			if (before != null && ReferenceEquals(before.token, this))
			{
				before.Link(null); // remove link from previous grid
				EVENT_Unlinked?.Invoke(before, this);
			}
			
			this.m_Grid = grid;

			if (grid != null)
			{
				if (!ReferenceEquals(grid.token, this))
					grid.Link(this); // link to new grid
				EVENT_Linked?.Invoke(grid, this);
			}

			EVENT_Updated?.Invoke(this);
		}
		public SxGrid GetGrid()
		{
			return m_Grid;
		}

		#region Disposable
		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					if (m_Grid != null &&
						m_Grid.HasToken() &&
						ReferenceEquals(m_Grid.token, this))
					{
						// remove THIS token in the grid
						m_Grid.Link(null);
					}
					m_Grid = null; // Unlink from parent grid
					if (EVENT_Disposed != null)
						EVENT_Disposed.Invoke(this);
				}

				EVENT_Updated = null;
				EVENT_Disposed = null;
				isDisposed = true;
			}
		}

		~SxToken()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			System.GC.SuppressFinalize(this);
		}
		#endregion Disposable
	}
}