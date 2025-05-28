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
		}

		public void SetColor(Color color)
		{
			mpb.SetColor("_Color", color);
			Apply();
		}
	}
}