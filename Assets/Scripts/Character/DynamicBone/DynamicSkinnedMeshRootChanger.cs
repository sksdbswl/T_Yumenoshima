using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace REIW
{
	public class DynamicSkinnedMeshBase : CacheMonoBehaviour
	{
		[System.Serializable]
		protected struct SaveInfo
		{
			[SerializeField]
			private Mesh mesh;
			[SerializeField]
			private Transform[] bones;
			[SerializeField]
			private Transform rootBone;
			[SerializeField]
			private SkinnedMeshRenderer _src;
			[SerializeField]
			private Material[] materials;
			[SerializeField]
			private Bounds bounds;

			public void Save(SkinnedMeshRenderer src)
			{
				if (src == null)
					return;

				_src = src;
				mesh = src.sharedMesh;
				bones = src.bones;
				rootBone = src.rootBone;

				if (materials != null)
				{
					foreach (Material mat in materials)
					{
						if (mat) Destroy(mat);
					}
				}
				materials = src.sharedMaterials.Select(x => Object.Instantiate(x)).ToArray();
				bounds = src.localBounds;
			}

			public void Restore()
			{
				if (_src == null)
					return;
				
				_src.enabled = false;
				_src.rootBone = rootBone;
				_src.bones = bones;
				_src.sharedMesh = mesh;
				_src.sharedMaterials = materials;
				_src.localBounds = bounds;
				_src.enabled = true;
			}
		}


		private int GetDepth(Transform t)
		{
			int d = 0;
			for (var cur = t; cur != null; cur = cur.parent) d++;
			return d;
		}

		public virtual void Restore()
		{

		}

		public virtual void OnDestroy()
		{
			
		}
	}

	public class DynamicSkinnedMeshRootChanger : DynamicSkinnedMeshBase
	{
		[SerializeField]
		private Transform _newRoot;

		private SkinnedMeshRenderer[] _mySkinnedMeshRenderers;
		private List<SaveInfo> _saveInfo = new List<SaveInfo>();
		private CharacterPartsController _characterPartsController;
		
		private void Awake()
		{
			_mySkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var t in _mySkinnedMeshRenderers)
			{
				SaveInfo saveinfo = new SaveInfo();
				saveinfo.Save(t);
				_saveInfo.Add(saveinfo);
			}
		}
		
		public void Initialize(CharacterPartsController controller, Animator animator, Transform newRoot)
		{
			_characterPartsController = controller;
			_newRoot = newRoot;
			SetChangeRootBone(_newRoot);
			animator.Rebind();
		}

		[ContextMenu("Restore")]
		public override void Restore()
		{
			base.Restore();
			foreach (SaveInfo info in _saveInfo)
				info.Restore();
		}

		private void SetChangeRootBone(Transform newRoot)
		{
			if (newRoot == null)
				return;

			foreach (var t in _mySkinnedMeshRenderers)
				SetChangeRootBone(newRoot, t);
		}

		private void SetChangeRootBone(Transform newRoot, SkinnedMeshRenderer renderer)
		{
			renderer.bones = _characterPartsController.GetNewBones(renderer.bones, _newRoot);
			Transform rootbone = _characterPartsController.FindMatchingTransform(renderer.rootBone, _newRoot);
			renderer.rootBone = rootbone;
		}

#if UNITY_EDITOR
		[ContextMenu("OnDynamicAction")]
		private void OnDynamicAction()
		{
			Awake();
			SetChangeRootBone(_newRoot);
		}
#endif
	}
}
