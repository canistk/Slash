using System.Collections;
using System.Collections.Generic;
using Slash.Core;
namespace Slash
{
    public class SxBoardManager
	{
        private static SxBoardManager m_Instance = null;
        public static SxBoardManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new SxBoardManager();
                }
                return m_Instance;
            }
		}

		#region Rule
		eGameRule m_Rule = eGameRule.None;
		public eGameRule Rule
        {
            get { return m_Rule; }
            private set
            {
                if (m_Rule == value)
                    return;
                m_Rule = value;
                SxLog.Info($"Game rule changed to: {m_Rule}");
				EVENT_GameRuleChanged?.Invoke(m_Rule);
            }
		}
		public void SetRule(eGameRule rule) => Rule = rule;
		#endregion Rule

		#region Events
		public event System.Action EVENT_BoardReset;
		public event System.Action EVENT_BoardCreated;
        public event System.Action<eGameRule> EVENT_GameRuleChanged;
		public delegate void BoardCreated(SxBoard board);
		#endregion Events


		private SxBoard m_Board = null;

        public void Init(int width, int height, eGameRule rule, SxBoard.GridCreated onCreated)
        {
            if (m_Board != null)
            {
                SxLog.Warning("Board already initialized. Reinitializing will reset the board.");
                ResetBoard();
			}
            m_Board = new SxBoard(width, height, onCreated);
            SetRule(rule);
			EVENT_BoardCreated?.Invoke();
		}

		public void ResetBoard()
        {
            m_Board = null;
            SxLog.Info("Board has been reset.");
		}


	}
}