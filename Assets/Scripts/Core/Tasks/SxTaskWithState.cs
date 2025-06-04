using System.Collections;
using System.Collections.Generic;
namespace Slash.Core.Tasks
{
	/// <summary>
	/// task with common <see cref="OnEnter"/>, <see cref="OnComplete"/> state
	/// and require sub-class to implement <see cref="ContinueOnNextCycle"/>
	/// true = Continue next cycle
	/// false = the end of the task.
	/// </summary>
	public abstract class SxTaskWithState : SxTask
	{
		public enum eState
		{
			Idle,
			Running,
			Abort,
			Completed,
		}
		public eState state { get; private set; } = eState.Idle;

		protected abstract void OnEnter();

		/// <summary>
		/// Task will be process on each <see cref="Execute"/>
		/// </summary>
		/// <returns>
		/// true = Continue next cycle
		/// false = the end of the task.
		/// </returns>
		protected abstract bool ContinueOnNextCycle();

		/// <summary>
		/// will be call at the end of process, 
		/// include <see cref="OnDisposing"/>
		/// </summary>
		protected abstract void OnComplete();

		protected sealed override bool InternalExecute()
		{
			if (state == eState.Abort ||
				state == eState.Completed)
				return false;

			if (state == eState.Idle)
			{
				OnEnter();
				if (state == eState.Idle)
				{
					// can be abort during on enter.
					state = eState.Running;
				}
			} // Note: no else case, task should able to be finish within single frame.

			if (state == eState.Running)
			{
				if (!ContinueOnNextCycle())
				{
					state = eState.Completed;
					Abort();
					return false;
				}
			}

			return state < eState.Completed;
		}

		protected override void OnDisposing()
		{
			if (state >= eState.Running)
			{
				// fire if task being started,
				// even running, abort, Completed
				OnComplete();
			}
			base.OnDisposing();
		}

		public override void Reset()
		{
			base.Reset();
			state = eState.Idle;
		}
	}
}