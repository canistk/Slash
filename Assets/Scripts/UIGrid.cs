using Slash.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slash
{
    public class UIGrid : UI3DRenderer
	{
		private SxGrid data;

		public void Init(SxGrid grid)
		{
			this.data = grid;
		}
	}
}