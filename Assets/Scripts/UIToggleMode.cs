using Slash.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
namespace Slash
{
	public class UIToggleMode : UIBoardCtrl
	{
		[SerializeField] ToggleGroup m_Group;
		[SerializeField] Toggle m_Reversi;
		[SerializeField] Toggle m_Gobang;
		[SerializeField] Toggle m_Checkers;

		public IEnumerable<Toggle> GetToggles()
		{
			if (m_Reversi)	yield return m_Reversi;
			if (m_Gobang)	yield return m_Gobang;
			if (m_Checkers)	yield return m_Checkers;
		}
		protected override void OnInit()
		{
			foreach (var t in GetToggles())
			{
				t.group = m_Group;
				t.onValueChanged.AddListener(OnSelected);
			}
		}

		private void OnSelected(bool active)
		{
			if (!active)
				return;
			var flag = 
				m_Reversi.isOn	? eGameRule.Reversi :
				m_Gobang.isOn	? eGameRule.Gobang :
				m_Checkers.isOn	? eGameRule.Checkers :
				eGameRule.None;
			if (flag == eGameRule.None)
				return;
			board.TryChangeMode(flag);
		}
	}
}
