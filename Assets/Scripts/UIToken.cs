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
			this.data.EVENT_Dispose -= Data_EVENT_Dispose;
			this.data.EVENT_Dispose += Data_EVENT_Dispose;


			var grid = this.data.GetGrid();
			if (grid == null)
				SxLog.Error("Unable to locate Grid UI.", this);
			var gridUI = grid.UI as UIGrid;
			var rot = token.isWhite ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(180f, 0f, 0f);
			var pos = gridUI.transform.position + (Vector3.up * 0.5f);
			transform.SetPositionAndRotation(pos, rot);
		}

		private void Data_EVENT_Dispose()
		{
		}

		public void SetColor(Color color)
		{
			mpb.SetColor("_Color", color);
			Apply();
		}
	}
}