using Slash.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace Slash
{
    public class UIGrid : UI3DRenderer
	{
		[System.Serializable]
		private class ClickVFX
		{
			public float duration = 0.5f;
			public float distance = 0.5f;
		}
		[SerializeField] private ClickVFX m_ClickVFX = new();
		[SerializeField] TMP_Text m_Label = null;
		public SxGrid data { get; private set; } = null;

		private Vector3 m_OriginalPosition;
		private Vector3 m_OriginalRotation;
		private void OnEnable()
		{
		}

		public void Init(SxGrid grid)
		{
			this.data = grid;
			this.data.UI = this;
			m_OriginalPosition = transform.localPosition;
			m_OriginalRotation = transform.localEulerAngles;

			if (m_Label)
			{
				m_Label.text = data.ReadableId;
			}
		}

		public void HandleClick()
		{
			if (data == null)
				throw new SetupException("data is not initialized. use Init()");
			else if (!ReferenceEquals(data.UI,this))
				throw new SetupException("UIGrid is not linked to the correct SxGrid instance. This can happen if the UIGrid is reused for another SxGrid without proper reinitialization.");
			
			InternalClicked();
		}


		private KeyValuePair<bool, float> m_LastClick;
		private void InternalClicked()
		{
			m_LastClick = new KeyValuePair<bool, float>(true, Time.timeSinceLevelLoad);
		}

		private void FixedUpdate()
		{
			if (!m_LastClick.Key)
				return;

			var pass = Time.timeSinceLevelLoad - m_LastClick.Value;
			var duration = m_ClickVFX.duration;
			var pt = duration > 0f ? Mathf.Clamp01(pass / duration) : 0f;

			var orgPos = m_OriginalPosition;
			var orgRot = m_OriginalRotation;
			var tarPos = m_OriginalPosition + Vector3.down * m_ClickVFX.distance;

			var pos = Vector3.Lerp(tarPos, orgPos, pt);
			var rot = Vector3.Lerp(orgRot, orgRot + Vector3.right * 360f, pt);

			if (pt < 1f)
			{
				transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(rot));
			}
			else
			{
				transform.SetLocalPositionAndRotation(orgPos, Quaternion.Euler(orgRot));
				m_LastClick = default;
			}
		}
	}
}