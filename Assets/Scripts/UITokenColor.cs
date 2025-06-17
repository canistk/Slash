using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Slash.Core;
using UnityEngine.UI;
namespace Slash
{
    public class UITokenColor : UIBoardCtrl
	{
        [SerializeField] Image m_Color;

		protected override void OnInit()
		{
		}

        void FixedUpdate()
        {
            if (!IsInited)
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

    }



	public abstract class UIBoardCtrl : MonoBehaviour
	{
		public bool IsInited { get; private set; } = false;
        protected virtual void Start()
        {
            Init();
        }

		protected abstract void OnInit();

		private async void Init()
		{
			if (IsInited)
				return;

			while (SxBoardManager.Instance == null)
				await Task.Delay(100);
			while (SxBoardManager.Instance.board.State <= Core.eGameState.InitBoard)
				await Task.Delay(1);
			m_Manager = SxBoardManager.Instance;
			OnInit();
			IsInited = true;
		}
		protected SxBoardManager m_Manager = null;
		protected SxBoard board => m_Manager.board;
	}
}