using System.Collections;
using System.Collections.Generic;
namespace Slash.Core.Tasks
{
	public abstract class SxTaskBase
	{
		/// <returns>
		/// true  = continue to execute on next cycle
		/// false = ending the task.
		/// </returns>
		public abstract bool Execute();
		public virtual void Reset() { }
	}
}