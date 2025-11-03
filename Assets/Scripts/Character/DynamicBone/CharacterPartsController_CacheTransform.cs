using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace REIW
{
    public partial class CharacterPartsController
    {
	    private List<GameObject> _newBones = new List<GameObject>();
	    public int NewBonesCount => _newBones.Count;
	    
		private void ReleaseCacheTransform()
		{
			_newBones.RemoveAll(x => x == null);
			_newBones.ForEach(x => Object.Destroy(x));
			_newBones.Clear();
		}

		public Transform GetNewBones(Transform srcbone, Transform stoptarget, System.Action<Transform> action = null)
		{
			var bonename = srcbone.name;
			if (string.IsNullOrEmpty(bonename))
				return null;

			Transform newBone  = FindMatchingTransform(srcbone, stoptarget);
			if (newBone != null)
				return newBone;

			Transform copybone = null;
			Transform parent = FindMatchingParent(srcbone, stoptarget, ref copybone);
			if (parent != null)
			{
				GameObject newobj = Object.Instantiate(copybone.gameObject);
				
				Transform objtrans = newobj.transform;
				objtrans.ResetTransform(parent, copybone);
				action?.Invoke(objtrans);
				
				_newBones.Add(newobj);
				MergeInto(CacheTransformDictionary, GetTransformListByName(objtrans));
			}
			
			return FindMatchingTransform(srcbone, stoptarget);
		}
		public Transform[] GetNewBones(Transform[] srcbones, Transform stoptarget, System.Action<Transform> action = null)
		{
			var srcBones = srcbones;
			var newBones = new Transform[srcBones.Length];
			Dictionary<string, List<Transform>> cachemap = CacheTransformDictionary;
			
			for (int i = 0; i < srcBones.Length; i++)
				newBones[i] = GetNewBones(srcBones[i], stoptarget, action);

			return newBones;
		}
		
		public Transform FindMatchingTransform(Transform bone, Transform stoptrans)
		{
			if (CacheTransformDictionary.TryGetValue(bone.name, out List<Transform> list) == false)
				return null;
			
			foreach (var t in list)
			{
				if (t == null)
					continue;
				
				if (stoptrans == t)
					return t;

				Transform tr = t.parent;
				Transform bonetr = bone.parent;

				while (tr != null && bonetr != null)
				{
					if (stoptrans == tr)
						return t;

					if (string.Equals(bonetr.name, tr.name, System.StringComparison.OrdinalIgnoreCase) == false)
						break;

					tr = tr.parent;
					bonetr = bonetr.parent;
				}

				if (tr == null && bonetr == null)
					return t;
			}

			return null;
		}
		
	    private Transform FindMatchingParent(Transform bone, Transform stopTrans, ref Transform copybone)
	    {
		    while (bone != null && bone != stopTrans)
		    {
			    Transform parent = FindMatchingTransform(bone, stopTrans);
			    if (parent != null)
				    return parent;

			    copybone = bone;
			    bone = bone.parent;
		    }

		    return stopTrans;
	    }

		public Transform GetOrCreateRootTransform(Transform parent, Transform srcRootTrans, Transform srcParentTrans, System.Action<Transform> action = null)
		{
			Transform newroot = _myTransform.FindAllChild(srcRootTrans.name);
			if (newroot != null)
				return newroot;

			newroot = Object.Instantiate(srcRootTrans.gameObject).transform;
			newroot.name = srcRootTrans.name;

			List<Transform> dellist = newroot.FindListAllChild(x => x.IsEmptyObject() == false);
			dellist.ForEach(x =>
			{
				GameObject go = x.gameObject;
				Object.DestroyImmediate(go);
			});

			var relRot = Quaternion.Inverse(srcParentTrans.rotation) * srcRootTrans.rotation;
			var relPos = srcParentTrans.InverseTransformPoint(srcRootTrans.position);
			var relScale = newroot.localScale;
			newroot.ResetTransform(parent, relRot, relPos, relScale);
			
			_newBones.Add(newroot.gameObject);
			action?.Invoke(newroot);
			MergeInto(CacheTransformDictionary, GetTransformListByName(newroot));
			return newroot;
		}
		
		void MergeInto(Dictionary<string, List<Transform>> target, Dictionary<string, List<Transform>> source)
		{
			foreach (var kv in source)
			{
				if (!target.TryGetValue(kv.Key, out var list))
				{
					list = new List<Transform>();
					target[kv.Key] = list;
				}

				foreach (var t in kv.Value)
				{
					if (t == null)
						continue;
					list.Remove(t);
					list.Add(t);
				}
			}
		}

		private Dictionary<string, List<Transform>> _cacheTransformDictionary = null;
		public Dictionary<string, List<Transform>> CacheTransformDictionary => _cacheTransformDictionary ??= GetTransformListByName(_myTransform);
		private Dictionary<string, List<Transform>> GetTransformListByName(Transform target) => target.GetComponentsInChildren<Transform>(true).GroupBy(x => x.name).ToDictionary(x => x.Key, x => x.ToList());

	    private void AddCacheTransform(Transform src)
	    {
		    if (CacheTransformDictionary.TryGetValue(src.name, out List<Transform> list))
		    {
			    list.Add(src);
			    return;
		    }

		    CacheTransformDictionary.Add(src.name, new List<Transform>() { src });
		    src.SetAsFirstSibling();
	    }

	    public bool RemoveCacheTransform(Transform src)
	    {
		    if (src == null)
			    return false;
		    if (CacheTransformDictionary.TryGetValue(src.name, out List<Transform> list) == false)
			    return false;

		    list.Remove(src);
		    Object.DestroyImmediate(src.gameObject);
		    return true;
	    }
    }
}
