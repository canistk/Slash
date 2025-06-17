using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Slash.Core;
using UnityEngine;
using UnityEngine.Pool;
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
					//var prefab = Resources.Load("BoardManager");
					//GameObject.Instantiate(prefab);

					m_Instance = FindObjectOfType<SxBoardManager>();
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
		[SerializeField] Vector3 m_GridOffset = new Vector3(0f, -0.5f, 0f);
		[SerializeField] GameObject m_TokenPrefab = null;
		ObjectPool<UIToken> m_TokenPool;
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

			var capacity = m_Width * m_Height;
			
			m_TokenPool = new ObjectPool<UIToken>(
				() => Instantiate(m_TokenPrefab).GetComponent<UIToken>(),
				token => token.gameObject.SetActive(true),
				token => token.gameObject.SetActive(false),
				token => Destroy(token.gameObject),
				false, capacity, capacity);
		}

		private void OnEnable()
		{
			InitBySetting();
		}

		private void Update()
		{
			HandleRaycast();
			if (m_Board != null)
				m_Board.Update();
		}

		private SxBoard m_Board = null;
		public SxBoard board => m_Board;

		[ContextMenu("Init by Setting")]
		public void InitBySetting()
		{
			Init(m_Width, m_Height, eGameRule.Checkers);
		}

		public void Init(int width, int height, eGameRule rule)
        {
            if (m_Board != null)
            {
                SxLog.Warning("Board already initialized. Reinitializing will reset the board.");
                ResetBoard();
			}
			m_Board = new SxBoard(width, height, OnGridCreated);
			SxGrid.EVENT_Linked += OnSpawnToken;
			SxGrid.EVENT_Unlinked += OnDespawnToken;
			SxToken.EVENT_Updated += EVENT_onTokenUpdated;
			m_Board.Init(rule, eTurn.White);
			EVENT_BoardCreated?.Invoke();
		}

		public void ResetBoard()
        {
            m_Board = null;
			SxGrid.EVENT_Linked -= OnSpawnToken;
			SxGrid.EVENT_Unlinked -= OnDespawnToken;
			SxToken.EVENT_Updated -= EVENT_onTokenUpdated;
			EVENT_BoardReset?.Invoke();
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

			var pos		= new Vector3(x, 0, y) + m_GridOffset;
			var isWhite = (x + y) % 2 == 0;
			var prefab  = isWhite ? m_GridWPrefab : m_GridBPrefab;
			var go		= Instantiate(prefab, transform);
			var uiGrid = go.GetComponent<UIGrid>();
			if (uiGrid == null)
				throw new System.NullReferenceException();
			
			go.transform.SetPositionAndRotation(pos, Quaternion.identity);

			var colliders = go.GetComponentsInChildren<Collider>();
			if (m_ChessLayer == -1)
			{
				m_ChessLayer = LayerMask.NameToLayer("chess");
			}
			foreach (var c in colliders)
			{
				c.gameObject.layer = m_ChessLayer;
			}
			uiGrid.Init(grid);
		}

		Dictionary<System.Guid, UIToken> m_TokenMap = new Dictionary<System.Guid, UIToken>();
		private void OnSpawnToken(SxGrid grid, SxToken token)
		{
			if (token == null)
				return;
			if (!m_TokenMap.TryGetValue(token.Id, out var uiToken))
			{
				uiToken = m_TokenPool.Get();
				uiToken.Init(token);
				m_TokenMap.Add(token.Id, uiToken);
			}
		}

		private void OnDespawnToken(SxGrid grid, SxToken token)
		{
			if (token == null)
				return;
			if (!m_TokenMap.TryGetValue(token.Id, out var uiToken))
			{
				uiToken = token.UI as UIToken;
				if (uiToken == null)
				{
					SxLog.Warning("Token UI is not a UIToken. Cannot release token UI.");
					return;
				}
			}
			if (uiToken == null)
			{
				SxLog.Error("UIToken is null. Cannot release token UI.");
				if (token != null)
					SxLog.Error($"Token: {token}");
				else
					SxLog.Error("Token is null. Cannot release token UI.");
				if (!m_TokenMap.Remove(token.Id))
					SxLog.Error("Token not found in the map. Cannot remove it from the map.");
				return;
			}
			m_TokenMap.Remove(token.Id);
			m_TokenPool.Release(uiToken);
		}

		private void EVENT_onTokenUpdated(SxToken token)
		{
			if (token == null)
			{
				SxLog.Error("Token is null. Cannot update token UI.");
				return;
			}
			if (!m_TokenMap.TryGetValue(token.Id, out var uiToken))
			{
				// common case: token is not spawned yet, first link called from grid/token cause loop
				return;
			}

			// only updated whenever it's being found.
			
		}

		#region Handle Click
		[Header("Raycast Settings")]
		[SerializeField] float m_Distance = 1000f;
		[SerializeField] LayerMask m_LayerMask = 0;
		[SerializeField] QueryTriggerInteraction m_Qti = QueryTriggerInteraction.UseGlobal;
		[SerializeField] int m_HitBuffer = 20;
		private RaycastHit[] m_Hits = null;
		
		public class LastRaycastHit
		{
			public bool valid;
			public RaycastHit ray;
			public float distance;
			public SxGrid grid;
			public SxToken token;
		}
		private LastRaycastHit m_LastClick;
		public LastRaycastHit lastClick => m_LastClick;

		public bool TryGetRaycast(out RaycastHit raycastHit, out UIGrid gridUI)
		{
			if (m_Hits == null || m_Hits.Length != m_HitBuffer)
			{
				m_Hits = new RaycastHit[m_HitBuffer];
			}
			raycastHit = default;
			gridUI = null;
			var cam = Camera.main;
			if (cam == null)
				return false;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * m_Distance, Color.red, 5f);
			var hitCnt = Physics.RaycastNonAlloc(ray, m_Hits, m_Distance, m_LayerMask, m_Qti);
			if (hitCnt == 0)
				return false; // hit nothing
			var iter = m_Hits
				.Take(hitCnt)
				.Where(o => o.collider != null)
				.OrderBy(o => o.distance);
			foreach (var hit in iter)
			{
				raycastHit = hit;
				gridUI = hit.collider?.gameObject.GetComponent<UIGrid>();
				return true;
			}

			return false; // no valid hit found
		}

		public void HandleRaycast()
		{
			var cam = Camera.main;
			if (cam == null)
				return;
			if (!Input.GetMouseButtonUp(0))
				return;
			if (!m_Board.IsWaitingForPlayer)
				return;

			if (!TryGetRaycast(out RaycastHit raycastHit, out UIGrid grid))
				return;

			if (grid == null)
			{
				SxLog.Warning("Raycast hit a collider without UIGrid component.");
				return; // no valid hit found
			}

			m_LastClick = new LastRaycastHit
			{
				valid = true,
				ray = raycastHit,
				distance = raycastHit.distance,
				grid = grid.data,
				token = grid.data.token,
			};

			try
			{
				var id = grid?.data == null ? "null" : grid.data.ReadableId;
				SxLog.Info($"Clicked on grid at position {id}{grid.data.coord}", grid);
				try
				{
					m_Board.ApplyPlayerSelection(grid.data);

					// accept the click and handle the UI animation
					grid.HandleClick(); // UI animation
					return; // only handle the first hit
				}
				catch (SxRuleException ex)
				{
					SxLog.Warning($"{ex.Message}");
					return; // skip to the next hit
				}
				catch (System.Exception ex)
				{
					throw ex;
				}

			}
			catch (System.Exception ex)
			{
				SxLog.Error($"Unknown Error handling click on grid: {ex.Message}");
				return;
			}
		}
		#endregion Handle Click
	}
}