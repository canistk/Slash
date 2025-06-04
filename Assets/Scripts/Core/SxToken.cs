using System.Collections;
using System.Collections.Generic;
namespace Slash.Core
{
    public class SxToken : System.IDisposable
	{
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
            return isWhite ? "White Token" : "Black Token";
        }

        public event System.Action EVENT_Flipped;

        public void Flip()
        {
            isWhite = !isWhite;
            EVENT_Flipped?.Invoke();
		}

		private SxGrid m_Grid = null;
		internal void LinkGrid(SxGrid grid)
		{
			this.m_Grid = grid;
		}
		public SxGrid GetGrid()
		{
			if (m_Grid == null)
			{
				throw new System.InvalidOperationException("Token is not linked to any grid.");
			}
			return m_Grid;
		}

		#region Disposable
		public event System.Action EVENT_Dispose;
		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					if (m_Grid.HasToken() && ReferenceEquals(m_Grid.token, this))
					{
						m_Grid.ClearToken();
					}
					m_Grid = null; // Unlink from parent grid
					if (EVENT_Dispose != null)
						EVENT_Dispose.Invoke();
				}

				EVENT_Flipped = null;
				EVENT_Dispose = null;
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