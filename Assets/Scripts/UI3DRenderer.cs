using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slash
{
	public class UI3DRenderer : MonoBehaviour
	{
		[SerializeField] private Renderer m_Renderer = null;
		public Renderer Renderer
		{
			get
			{
				if (m_Renderer == null)
				{
					m_Renderer = GetComponentInChildren<Renderer>(true);
				}
				return m_Renderer;
			}
		}

		private Material m_Material = null;
		public Material material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = Renderer?.sharedMaterial;
				}
				return m_Material;
			}
		}

		MaterialPropertyBlock m_Mpb = null;
		public MaterialPropertyBlock mpb
		{
			get
			{
				if (m_Mpb == null)
					m_Mpb = new MaterialPropertyBlock();
				return m_Mpb;
			}
		}

		public void Apply() => Renderer.SetPropertyBlock(mpb);

		protected virtual void Reset()
		{
			ReferenceEquals(Renderer, null);
		}
	}
}