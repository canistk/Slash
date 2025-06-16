using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Slash.Core;
using UnityEngine.UI;
namespace Slash
{
    public class UITokenColor : MonoBehaviour
    {
        private bool m_Inited = false;
        [SerializeField] Image m_Color;
        void Start()
        {
            Init();
        }

        void FixedUpdate()
        {
            if (!m_Inited)
                return;

			if (m_Color)
            {
                var col =
                    board.Turn == eTurn.White ? Color.white :
                    board.Turn == eTurn.Black ? Color.black :
                    Color.magenta;
                m_Color.color = col;
            }

        }

        private async void Init()
        {
            if (m_Inited)
                return;

            while (SxBoardManager.Instance == null)
                await Task.Delay(100);
            while (SxBoardManager.Instance.board.State <= Core.eGameState.InitBoard)
                await Task.Delay(1);
            m_Manager = SxBoardManager.Instance;
            m_Inited = true;
		}
        SxBoardManager m_Manager = null;
        SxBoard board => m_Manager.board;
    }
}