using Slash.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
namespace Slash
{
    public class UIToken : UI3DRenderer
	{
		private SxToken data;
		[SerializeField]
		private bool m_UpdateColor = false;

		private void Awake()
		{
			SxToken.EVENT_Updated += SxToken_EVENT_Updated;
			SxToken.EVENT_Linked += SxToken_EVENT_Linked;
		}

		private void SxToken_EVENT_Linked(SxGrid grid, SxToken token)
		{
			
		}

		private void SxToken_EVENT_Updated(SxToken _)
		{
			//if (data == null || obj != data)
			//	return;
			gameObject.name = $"Token [{(data.isWhite ? "White" : "Black")}]";
			var grid = data.GetGrid();
			if (grid != null && grid.UI is UIGrid gridUI)
			{
				MoveTo(gridUI);
			}
			UpdateColor();
		}

		public void Init(SxToken token)
		{
			this.data = token;
			this.data.UI = this;

			var grid = this.data.GetGrid();
			if (grid != null && grid.UI is UIGrid gridUI)
			{
				MoveTo(gridUI);
			}

			UpdateColor(true);
		}

		private void MoveTo(UIGrid gridUI)
		{
			if (gridUI == null)
				return;

			transform.SetParent(gridUI.transform, false);
			var pos = gridUI.transform.position + (Vector3.up * 0.5f);
			transform.position = pos;
		}
		private void FixedUpdate()
		{
			// SetColor with rotation animation
			if (!m_UpdateColor)
				return;

			if (data == null)
				return;
			var finalRot = data.isWhite ? Quaternion.identity : Quaternion.Euler(180f, 0f, 0f);
			if (transform.rotation != finalRot)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRot, 5f * 360f * Time.fixedDeltaTime);
			}
			else
			{
				UpdateColor(true);
			}
		}

		private void UpdateColor(bool force = false)
		{
			if (data == null)
				return;

			if (force)
			{
				var rot = data.isWhite ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(180f, 0f, 0f);
				transform.rotation = rot;
				m_UpdateColor = false;
				return;
			}
			m_UpdateColor = true;
		}

	}
}