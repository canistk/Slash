using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Slash.Core;
using UnityEngine;
namespace Slash
{
    public class SxBoardManager : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod]
		public static void SelfInit()
		{
			ReferenceEquals(Instance, null);
		}

        private static SxBoardManager m_Instance = null;
        public static SxBoardManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
					// m_Instance = new SxBoardManager();
					var prefab = Resources.Load("BoardManager");
					GameObject.Instantiate(prefab);
					// let awake handle the instance creation
					// m_Instance = FindObjectOfType<SxBoardManager>();
				}
				return m_Instance;
            }
		}

		public event System.Action EVENT_BoardReset;
		public event System.Action EVENT_BoardCreated;

		[SerializeField] int m_Width = 8;
		[SerializeField] int m_Height = 8;
		[SerializeField] GameObject m_GridWPrefab = null;
		[SerializeField] GameObject m_GridBPrefab = null;
		[SerializeField] GameObject m_TokenPrefab = null;

		private void Awake()
		{
			if (m_Instance != null)
			{
				if (m_Instance == this)
				{
					return;
				}
				else
				{
					SxLog.Error("Multiple instances of SxBoardManager detected. Only one instance is allowed.");
					this.enabled = false;
					Destroy(gameObject);
					return;
				}
			}

			m_Instance = this;
			DontDestroyOnLoad(gameObject);
			InitBySetting();
		}

		private void Update()
		{
			HandleRaycast();
		}

		private SxBoard m_Board = null;

		[ContextMenu("Init by Setting")]
		public void InitBySetting()
		{
			Init(m_Width, m_Height, eGameRule.Reversi);
		}

		public void Init(int width, int height, eGameRule rule)
        {
            if (m_Board != null)
            {
                SxLog.Warning("Board already initialized. Reinitializing will reset the board.");
                ResetBoard();
			}
			m_Board = new SxBoard(width, height, OnGridCreated);
            m_Board.SetRule(rule);
			EVENT_BoardCreated?.Invoke();
		}

		public void ResetBoard()
        {
            m_Board = null;
            SxLog.Info("Board has been reset.");
		}


		private int m_ChessLayer = -1;
		private void OnGridCreated(int x, int y, SxGrid grid)
		{
			// Handle grid creation logic here if needed
			if (m_GridWPrefab == null || m_GridBPrefab == null)
			{
				SxLog.Info($"Grid created at ({x}, {y})");
				return;
			}

			var pos		= new Vector3(x, 0, y);
			var isWhite = (x + y) % 2 == 0;
			var prefab  = isWhite ? m_GridWPrefab : m_GridBPrefab;
			var go		= Instantiate(prefab, pos, Quaternion.identity);
			var comp	= go.GetComponent<UIGrid>();

			if (comp == null)
				throw new System.NullReferenceException();

			go.name		= $"Grid [{grid.ReadableId}] - {(isWhite?'W':'B')}";

			var colliders = go.GetComponentsInChildren<Collider>();
			if (m_ChessLayer == -1)
			{
				m_ChessLayer = LayerMask.NameToLayer("chess");
			}
			foreach (var c in colliders)
			{
				c.gameObject.layer = m_ChessLayer;
			}
			comp.Init(grid);
		}

		#region Handle Click
		[Header("Raycast Settings")]
		[SerializeField] float m_Distance = 1000f;
		[SerializeField] LayerMask m_LayerMask = 0;
		[SerializeField] QueryTriggerInteraction m_Qti = QueryTriggerInteraction.UseGlobal;
		[SerializeField] int m_HitBuffer = 20;
		private RaycastHit[] m_Hits = null;
		public void HandleRaycast()
		{
			var cam = Camera.main;
			if (cam == null)
				return;
			if (!Input.GetMouseButtonUp(0))
				return;
			if (m_Hits == null || m_Hits.Length != m_HitBuffer)
			{
				m_Hits = new RaycastHit[m_HitBuffer];
			}
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * m_Distance, Color.red, 5f);
			var hitCnt = Physics.RaycastNonAlloc(ray, m_Hits, m_Distance, m_LayerMask, m_Qti);
			if (hitCnt == 0)
				return; // hit nothing
			var iter = m_Hits
				.Take(hitCnt)
				.Where(o => o.collider != null)
				.OrderBy(o => o.distance);
			foreach (var obj in iter)
			{
				var grid = obj.collider.gameObject.GetComponent<UIGrid>();
				if (grid == null)
				{
					SxLog.Warning("Raycast hit a collider without UIGrid component.");
					continue;
				}

				try
				{
					grid.HandleClick();
					SxLog.Info($"Clicked on grid at position {grid.transform.position}", grid);
					return; // only handle the first hit
				}
				catch (System.Exception ex)
				{
					SxLog.Error($"Error handling click on grid: {ex.Message}");
					continue;
				}
			}
		}
		#endregion Handle Click
	}
}