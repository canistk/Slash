using System;
using System.Collections;
using System.Collections.Generic;
namespace Slash.Core.Tasks
{
    public abstract class SxTask : SxTaskBase, IDisposable
	{
		public void Abort()
		{
			if (isDisposed)
				return;
			Dispose(disposing: true);
		}

		public sealed override bool Execute()
		{
			if (isDisposed || isCompleted)
				return false;

			isCompleted = !InternalExecute();
			return !isCompleted;
		}

		protected abstract bool InternalExecute();

		public bool isCompleted { get; private set; } = false;


		/// <summary>
		/// will be call during <see cref="Dispose(bool)"/>
		/// dispose managed state (managed objects)
		/// </summary>
		protected virtual void OnDisposing() { }
		/// <summary>
		/// TODO: free unmanaged resources (unmanaged objects) and override finalizer
		/// TODO: set large fields to null
		/// </summary>
		protected virtual void OnFreeMemory() { }

		#region Dispose
		public bool isDisposed { get; private set; } = false;

		protected void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					OnDisposing();
				}
				OnFreeMemory();
				isDisposed = true;
			}
		}

		~SxTask()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		void System.IDisposable.Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			// System.GC.SuppressFinalize(this);
		}
		#endregion Dispose
	}
}