using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REIW
{
    public partial class CharacterPartsController
    {
        public enum PartsType
        {
            Eye,
            Face = 10,
            Foot = 20,
            Hair = 30,
            LowBody = 40,
            UpperBody = 50,
            
            Head = 60,
            Back = 70,
            Earring = 80,
            Face1 = 90,
            Face2 = 100,
            Ring = 110,
            
            Max,
            All = 200,
            Custom = 300,
        }

        public static bool TryConvertToPartsType(EnumParts itemEnumParts, out PartsType partsType)
        {
            partsType = itemEnumParts switch
            {
                EnumParts.Top     => PartsType.UpperBody,
                EnumParts.Bottom  => PartsType.LowBody,
                EnumParts.Shoes   => PartsType.Foot,

                EnumParts.Head    => PartsType.Head,
                EnumParts.Back    => PartsType.Back,
                EnumParts.Earring => PartsType.Earring,
                EnumParts.Face1   => PartsType.Face1,
                EnumParts.Face2   => PartsType.Face2,
                EnumParts.Ring    => PartsType.Ring,

                _ => PartsType.All
            };
            return (partsType != PartsType.All);
        }
        
        public class EnumOffsetPreviewAttribute : PropertyAttribute
        {
            
        }
        
        public CharacterPartsController(Transform transform)
        {
            Initialize(transform);
        }
        
        ~CharacterPartsController()
        {
            ReleaseNewBones();
            ReleaseCacheTransform();
            ReleaseMagicaCloth();
        }

        private Dictionary<PartsType, SkinnedMeshRenderer> _mySkinnedMeshRenderers  = null;
        public IDictionary<PartsType, SkinnedMeshRenderer> MySkinnedMeshRenderers => _mySkinnedMeshRenderers;
        
        private List<DynamicSkinnedMeshRebinder> _rebinders = new List<DynamicSkinnedMeshRebinder>();
        private IEnumerable<PartsType> _lastequipParts;

        private Transform _myTransform;
        
        private Dictionary<PartsType, SkinnedMeshRenderer> _originSkinnedMeshRenderers  = null;
        
        public Action<PartsType> OnPartsEquipped;
        
        private void Initialize(Transform transform)
        {
            _myTransform = transform;
            RebuildCache(_myTransform);
        }

        public void RebuildCache(Transform target)
        {
            _mySkinnedMeshRenderers = GetSkinnedMeshRenderers(target, PartsType.All);
            RebuildCacheMagicaCloth(target);
            
            _originSkinnedMeshRenderers = new Dictionary<PartsType,SkinnedMeshRenderer>();
            Utilities.DeepCopy(_mySkinnedMeshRenderers, _originSkinnedMeshRenderers);
        }

        public void ResetParts()
        {
            _lastequipParts = null;
            foreach (var pair in _originSkinnedMeshRenderers)
            {
                SetEquip(pair.Value.transform, pair.Key);
            }

            Save();
        }

        public void RebuildDefaultParts(EnumGender gender)
        {
            IDictionary<PartsType, Transform> parts = GameDataModel.Singleton.EquipPartsDatas.DefulatParts(gender);
            UnEquip();
            foreach (var pair in parts)
            {
                SetEquip(pair.Value, pair.Key);
            }
        }

        public static Dictionary<PartsType, Transform> GetDefaultSkinnedMeshRenderers(GameObject target)
        {
            Dictionary<PartsType, SkinnedMeshRenderer> dic = GetSkinnedMeshRenderers(target.transform, PartsType.All);
            return dic.ToDictionary(x => x.Key, x => x.Value.transform);
        }

        public static Dictionary<PartsType, SkinnedMeshRenderer> GetSkinnedMeshRenderers(Transform target, PartsType settype)
        {
            List<SkinnedMeshRenderer> renderersInChildren = new List<SkinnedMeshRenderer>();
            target.GetComponentsInChildren<SkinnedMeshRenderer>(true, renderersInChildren);
            return GetSkinnedMeshRenderers(renderersInChildren, settype);
        }
        
        private static Dictionary<PartsType, T> GetSkinnedMeshRenderers<T>(List<T> renderersInChildren, PartsType settype) where T : Renderer
        {
            Dictionary<PartsType, T> result = new Dictionary<PartsType, T>();

            PartsType[] partslist = EnumUtils.GetEnumValues<PartsType>(PartsType.Max);
            foreach (PartsType partstype in partslist)
            {
                if (settype != PartsType.All && settype != partstype)
                    continue;

                string typename = partstype.ToString();
                T renderer = renderersInChildren.Find(x => x.name.IndexOf(typename, StringComparison.OrdinalIgnoreCase) >= 0);
                if (renderer == null)
                    continue;

                renderersInChildren.Remove(renderer);
                result[partstype] = renderer;
            }
            
            if (settype == PartsType.All)
            {
                Dictionary<PartsType, int> customtypecount = new Dictionary<PartsType, int>();
                
                for (int i = 0; i < renderersInChildren.Count; ++i)
                {
                    PartsType findpartstype = PartsType.Custom;
                    foreach (PartsType partstype in partslist)
                    {
                        if (partstype >= PartsType.Max)
                            continue;

                        string typename = partstype.ToString();
                        if (renderersInChildren[i].name.IndexOf(typename, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            findpartstype = partstype;
                            break;
                        }
                    }

                    if (customtypecount.TryGetValue(findpartstype, out int count) == false)
                        customtypecount[findpartstype] = (int)findpartstype;
                    else
                        customtypecount[findpartstype] = count + 1;
                    
                    result[PartsType.Custom + customtypecount[findpartstype]] = renderersInChildren[i];
                }
            }

            return result;
        }

        private SkinnedMeshRenderer MakeSkinnedMeshRenderer(Transform parentstarget, Transform srctarget)
        {
            GameObject obj = new GameObject(srctarget.name);
            obj.transform.ResetTransform(parentstarget, srctarget.transform);
            return obj.AddComponent<SkinnedMeshRenderer>();
        }

        public class EquipPartsInfo
        {
            public EquipPartsDataSO.DataInfo DataInfo { get; init; }
            public EquipPartsDataSO.ObjectLOD LOD { get; init; } = EquipPartsDataSO.ObjectLOD.LOD1;
            public GameObject LODObject => DataInfo.GetLODObject(LOD);
        }

        public void ApplyEquipmentParts(IList<EquipPartsInfo> equippartslist)
        {
            Dictionary<PartsType, SkinnedMeshRenderer> dic = new Dictionary<PartsType, SkinnedMeshRenderer>();
            foreach (EquipPartsInfo info in equippartslist)
            {
                GameObject lodObject = info.LODObject;
                if (lodObject == null)
                    continue;
                
                Dictionary<PartsType, SkinnedMeshRenderer> targetlist = GetSkinnedMeshRenderers(lodObject.transform, info.DataInfo.Parts);
                foreach (var pair in targetlist)
                    dic[pair.Key] = pair.Value;
            }

            SetEquip(dic, PartsType.All);
        }

        public IReadOnlyDictionary<PartsType, SkinnedMeshRenderer> ConvertPartsToRenderers(IDictionary<PartsType, Transform> srclist)
        {
            return srclist
                .SelectMany(e => GetSkinnedMeshRenderers(e.Value, e.Key))
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value);
        }
        
        public void SetAllEquip(IDictionary<PartsType, Transform> targetlist)
        {
            UnEquip();
            SetEquip(ConvertPartsToRenderers(targetlist), PartsType.All);
        }

        private void SetEquip(IReadOnlyDictionary<PartsType, SkinnedMeshRenderer> srclist, PartsType settype)
        {
            _lastequipParts = srclist.Keys;

            List<PartsType> ownlist = _mySkinnedMeshRenderers.Keys.ToList();
            foreach (PartsType type in srclist.Keys)
            {
                if (_mySkinnedMeshRenderers.TryGetValue(type, out SkinnedMeshRenderer renderer) == false)
                    renderer = MakeSkinnedMeshRenderer(_myTransform, srclist[type].transform);

                DynamicSkinnedMeshRebinder rebinder = renderer.gameObject.GetorAddComponent<DynamicSkinnedMeshRebinder>();
                rebinder.CharacterPartsType = type;
                rebinder.Initialize(this, _myTransform, srclist[type], type);

                _rebinders.Remove(rebinder);
                _rebinders.Add(rebinder);

                renderer.enabled = true;
                ownlist.Remove(type);
            }

            if (settype == PartsType.All)
            {
                foreach (PartsType type in ownlist)
                {
                    if (_mySkinnedMeshRenderers.TryGetValue(type, out SkinnedMeshRenderer renderer) && type >= PartsType.All)
                    {
                        renderer.enabled = false;
                    }
                }
            }
            
            OnPartsEquipped?.Invoke(settype);
        }

        public void SetEquip(EquipPartsDataSO.DataInfo info, EquipPartsDataSO.ObjectLOD lod = EquipPartsDataSO.ObjectLOD.LOD1)
        {
            if (info == null)
                return;
            
            SetEquip(info.GetLODObject(lod).transform, info.Parts);
        }
        
        public void SetEquip(Transform srctarget, PartsType settype)
        {
            if (srctarget == null)
                return;

            Dictionary<PartsType, SkinnedMeshRenderer> targetlist = GetSkinnedMeshRenderers(srctarget, settype);
            SetEquip(targetlist, settype);
        }

        public void EmptyEquip(CharacterPartsController.PartsType partsType)
        {
            if (partsType == PartsType.All)
            {
                foreach (var pair in _mySkinnedMeshRenderers)
                {
                    pair.Value.enabled = false;
                }
                return;
            }
            
            if (_mySkinnedMeshRenderers.TryGetValue(partsType, out SkinnedMeshRenderer renderer) == false)
                return;

            renderer.enabled = false;
        }
        
        public void UnEquip()
        {
            UnEquipParts();
        }
        
        public void UnEquipLast()
        {
            UnEquipParts(_lastequipParts);
        }

        public void Save()
        {
            _rebinders.Clear();
            RebuildCache(_myTransform);
            _lastequipParts = null;
        }
        
        public void UnEquipParts(IEnumerable<PartsType> parts = null)
        {
            Dictionary<PartsType,DynamicSkinnedMeshRebinder> targetparts = new Dictionary<PartsType, DynamicSkinnedMeshRebinder>(); 

            foreach (DynamicSkinnedMeshRebinder rebinder in _rebinders)
            {
                if (!(parts?.Contains(rebinder.CharacterPartsType) ?? true))
                    continue;

                if (parts == null)
                    rebinder.Restore();
                else
                    rebinder.RestoreLast();

                targetparts.Add(rebinder.CharacterPartsType, rebinder);
            }

            foreach (KeyValuePair<PartsType, SkinnedMeshRenderer> pair in _mySkinnedMeshRenderers)
            {
                if (targetparts.Remove(pair.Key))
                    continue;
                
                pair.Value.enabled = true;
            }

            foreach (DynamicSkinnedMeshRebinder rebinder in targetparts.Values)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(rebinder.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(rebinder.gameObject);
            }
            targetparts.Clear();
            
            if (parts == null)
                _rebinders.Clear();
            else
                _rebinders.RemoveAll(x => parts.Contains(x.CharacterPartsType));

            UnEquipMagicaCloth(parts);
        }

        private void ReleaseNewBones()
        {
            foreach (var bone in _newBones)
            {
                if (bone == null)
                    continue;
                
                UnityEngine.Object.Destroy(bone.gameObject);
            }
            _newBones.Clear();
        }
    }

    public struct SmrSnapshot
    {
        public int MeshId;
        public int RootId;
        public int[] BoneIds;
        public int[] MaterialIds;
    }

    public class SkinnedMeshDirtyTracker
    {
        public bool IsDirty(SkinnedMeshRenderer smr, ref SmrSnapshot snap, bool resetTransformFlags = true)
        {
            bool dirty = false;

            int meshId = smr && smr.sharedMesh ? smr.sharedMesh.GetInstanceID() : 0;
            if (meshId != snap.MeshId)
            {
                snap.MeshId = meshId;
                dirty = true;
            }

            int rootId = smr && smr.rootBone ? smr.rootBone.GetInstanceID() : 0;
            if (rootId != snap.RootId)
            {
                snap.RootId = rootId;
                dirty = true;
            }

            var bones = smr && smr.bones != null ? smr.bones : Array.Empty<Transform>();
            if (snap.BoneIds == null || snap.BoneIds.Length != bones.Length)
            {
                snap.BoneIds = bones.Select(b => b ? b.GetInstanceID() : 0).ToArray();
                dirty = true;
            }
            else
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    int id = bones[i] ? bones[i].GetInstanceID() : 0;
                    if (snap.BoneIds[i] != id)
                    {
                        snap.BoneIds[i] = id;
                        dirty = true;
                    }
                }
            }

            // 본 이동 감지(포즈 변경)
            foreach (var b in bones)
            {
                if (b && b.hasChanged)
                {
                    dirty = true;
                }
            }

            if (resetTransformFlags)
            {
                foreach (var b in bones)
                    if (b)
                        b.hasChanged = false;
            }

            var mats = smr && smr.sharedMaterials != null ? smr.sharedMaterials : Array.Empty<Material>();
            if (snap.MaterialIds == null || snap.MaterialIds.Length != mats.Length)
            {
                snap.MaterialIds = mats.Select(m => m ? m.GetInstanceID() : 0).ToArray();
                dirty = true;
            }
            else
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    int id = mats[i] ? mats[i].GetInstanceID() : 0;
                    if (snap.MaterialIds[i] != id)
                    {
                        snap.MaterialIds[i] = id;
                        dirty = true;
                    }
                }
            }

            return dirty;
        }
    }
}
    