using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public SxBoardManager()
        {
        }
	}
}