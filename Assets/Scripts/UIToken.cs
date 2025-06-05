using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slash.Core;
namespace Slash
{
    public class UIToken : UI3DRenderer
	{
		private SxToken data;

		public void Init(SxToken token)
		{
			this.data = token;
			this.data.UI = this;

			var pos = transform.position;

			var grid = this.data.GetGrid();
			if (grid != null && grid.UI is UIGrid gridUI)
			{
				pos = gridUI.transform.position + (Vector3.up * 0.5f);
			}

			var rot = token.isWhite ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(180f, 0f, 0f);
			transform.SetPositionAndRotation(pos, rot);
		}

		[System.Obsolete]
		public void SetColor(Color color)
		{
			mpb.SetColor("_Color", color);
			Apply();
		}
	}
}