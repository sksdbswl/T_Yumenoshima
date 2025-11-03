// using UnityEngine;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
//
// namespace REIW
// {
//     public partial class CharacterPartsController
//     {
//         private Dictionary<PartsType, List<MagicaCloth>> _newMagicaCloths = null;
//         private Dictionary<PartsType, List<MagicaCloth>> NewMagicaCloths => _newMagicaCloths ??= new Dictionary<PartsType, List<MagicaCloth>>();
//
//         private Transform _myRootTrans = null;
//
//         private Dictionary<PartsType, List<Transform>> _newMagicaClothsBone = null;
//         private Dictionary<PartsType, List<Transform>> NewMagicaClothsBone => _newMagicaClothsBone ??= new Dictionary<PartsType, List<Transform>>();
//
//         private CancellationTokenSource _tokenSource = null;
//
//         private void RebuildCacheMagicaCloth(Transform target)
//         {
//             FindRootTrans(target, out _myRootTrans, out Transform srcParentTrans);
//         }
//
//         private void UnEquipMagicaCloth(IEnumerable<PartsType> parts = null)
//         {
//             if (parts == null)
//             {
//                 DestroyOldMagicaClothComponent(PartsType.All, NewMagicaCloths);
//                 return;
//             }
//
//             foreach (PartsType partstype in parts)
//                 DestroyOldMagicaClothComponent(partstype, NewMagicaCloths);
//         }
//
//         private void AddMagicaClothComponent<T>(T src, PartsType partstype, Dictionary<PartsType, List<T>> map) where T : Component
//         {
//             if (src == null)
//                 return;
//
//             if (map.TryGetValue(partstype, out List<T> list2))
//                 list2.Add(src);
//             else
//                 map.Add(partstype, new List<T>() { src });
//
//             if (src is Transform srctrans)
//                 srctrans.SetAsFirstSibling();
//         }
//
//         private void DestroyMagicaClothAll()
//         {
//             DestroyOldMagicaClothComponent(PartsType.All, NewMagicaCloths);
//             NewMagicaCloths.Clear();
//             DestroyOldMagicaClothComponent(PartsType.All, NewMagicaClothsBone);
//             NewMagicaClothsBone.Clear();
//         }
//
//         private void DestroyOldMagicaCloth(PartsType settype)
//         {
//             DestroyOldMagicaClothComponent(settype, NewMagicaCloths);
//         }
//         
//         private void DestroyOldMagicaClothBone(PartsType settype)
//         {
//             DestroyOldMagicaClothComponent(settype, NewMagicaClothsBone, (x) =>
//             {
//                 if (CacheTransformDictionary.TryGetValue(x.name, out List<Transform> list))
//                     list.Remove(x);
//             });
//         }
//         private void DestroyOldMagicaClothComponent<T>(PartsType settype, Dictionary<PartsType, List<T>> map, System.Action<T> action = null) where T : Component
//         {
//             if (settype == PartsType.All)
//             {
//                 foreach (var pair in map)
//                 {
//                     var list = pair.Value;
//                     DestroyMagicaClothList(list);
//                     list.Clear();
//                 }
//
//                 return;
//             }
//
//             if (map.TryGetValue(settype, out List<T> list2))
//             {
//                 DestroyMagicaClothList(list2);
//                 map[settype].Clear();
//             }
//
//             void DestroyMagicaClothList(List<T> removelist)
//             {
//                 removelist.RemoveAll(x => x == null);
//                 removelist.ForEach(x =>
//                 {
//                     action?.Invoke(x);
//                     UnityEngine.Object.Destroy(x.gameObject);
//                 });
//             }
//         }
//
//         private Dictionary<PartsType, List<T>> GetMagicaComponent<T>(Transform target, PartsType settype) where T : MonoBehaviour
//         {
//             Dictionary<PartsType, List<T>> result = new Dictionary<PartsType, List<T>>();
//             List<T> magicacomponentInChildren = new List<T>();
//             target.GetComponentsInChildren<T>(true, magicacomponentInChildren);
//             
//             PartsType[] partslist = EnumUtils.GetEnumValues<PartsType>(PartsType.Max);
//             foreach (PartsType partstype in partslist)
//             {
//                 if (settype != PartsType.All && settype != partstype)
//                     continue;
//
//                 string typename = partstype.ToString();
//                 List<T> magicacomponent = magicacomponentInChildren.FindAll(x => x.name.IndexOf(typename, StringComparison.OrdinalIgnoreCase) >= 0);
//                 if (magicacomponent.Count <= 0)
//                     continue;
//
//                 foreach (T component in magicacomponent)
//                 {
//                     magicacomponentInChildren.Remove(component);
//                     if (result.ContainsKey(partstype) == false)
//                         result.Add(partstype, new List<T>());
//
//                     result[partstype].Add(component);
//                 }
//             }
//
//             PartsType remaincloth = settype == PartsType.All ? PartsType.Custom : settype;
//             foreach (T magicacloth in magicacomponentInChildren)
//             {
//                 if (result.ContainsKey(remaincloth) == false)
//                     result.Add(remaincloth, new List<T>());
//                 
//                 result[remaincloth].Add(magicacloth);
//             }
//             
//             return result;
//         }
//
//         private void SetMagicaCloth(IReadOnlyDictionary<PartsType, SkinnedMeshRenderer> srclist, Transform scrroot, IReadOnlyDictionary<PartsType, List<MagicaCloth>> magicacloths, PartsType settype)
//         {
//             _tokenSource ??= new CancellationTokenSource();
//             SetMagicaCloth(srclist.Keys.ToList(), scrroot, magicacloths, settype, _tokenSource.Token).Forget();
//         }
//         
//         private void SetEquipWithMagicaCloth(IReadOnlyDictionary<PartsType, SkinnedMeshRenderer> srclist, Transform scrroot, IReadOnlyDictionary<PartsType, List<MagicaCloth>> magicacloths, PartsType settype)
//         {
//             SetEquip(srclist, settype);
//             _tokenSource ??= new CancellationTokenSource();
//             SetMagicaCloth(srclist.Keys.ToList(), scrroot, magicacloths, settype, _tokenSource.Token).Forget();
//         }
//
//         private async UniTaskVoid SetMagicaCloth(IReadOnlyList<PartsType> partslist, Transform scrroot, IReadOnlyDictionary<PartsType, List<MagicaCloth>> magicacloths, PartsType settype, CancellationToken token)
//         {
//             Dictionary<MagicaCloth, Dictionary<string, Transform>> newcloths = new Dictionary<MagicaCloth, Dictionary<string, Transform>>();
//
//             foreach (PartsType partstype in partslist)
//             {
//                 if (settype == PartsType.All)
//                 {
//                     DestroyOldMagicaCloth(partstype);
//                     DestroyOldMagicaClothBone(partstype);
//                 }
//                 else if (settype != PartsType.All && settype == partstype)
//                 {
//                     DestroyOldMagicaCloth(partstype);
//                     DestroyOldMagicaClothBone(partstype);
//                 }
//
//                 if (settype != PartsType.All && settype != partstype)
//                     continue;
//
//                 if (magicacloths == null || magicacloths.TryGetValue(partstype, out List<MagicaCloth> cloths2) == false)
//                     continue;
//
//                 foreach (MagicaCloth srccloth in cloths2)
//                 {
// //                    checkcloths.Remove(srccloth);
//
//                     MagicaCloth clone = AddNewMagicaComponent<MagicaCloth>(partstype, srccloth, NewMagicaCloths);
//                     clone.gameObject.SetActive(false);
//
//                     Transform clothtrans = srccloth.transform;
//                     var relRot = Quaternion.Inverse(scrroot.rotation) * clothtrans.rotation;
//                     var relPos = scrroot.InverseTransformPoint(clothtrans.position);
//                     var relScale = clothtrans.localScale;
//
//                     Transform cloneTrans = clone.transform;
//                     cloneTrans.ResetTransform(_myRootTrans, relRot, relPos, relScale);
//
//                     ///////////////////////////////////////////////
//                     /// 
//                     ClothSerializeData data = srccloth.SerializeData;
//
//                     Dictionary<string, Transform> newBones = new Dictionary<string, Transform>();
//                     foreach (Transform bone in data.rootBones)
//                     {
//                         Transform findtrans = FindMatchingTransform(bone, _myRootTrans);
//                         if (findtrans == null)
//                         {
//                             findtrans = GetNewBones(bone, _myRootTrans);
//                             AddMagicaClothComponent(findtrans, partstype, NewMagicaClothsBone);
//                         }
//                         else
//                             SyncChildrenByName(bone, findtrans, partstype);
//
//                         newBones.Add(bone.name, findtrans);
//                     }
//
//                     clone.SerializeData.sourceRenderers = RebindRenderer(data.sourceRenderers);
//                     clone.SerializeData.rootBones = newBones.Values.ToList();
//                     clone.SerializeData.colliderCollisionConstraint.colliderList = RebindCollision(data.colliderCollisionConstraint.colliderList, partstype);
//                     ;
//                     RebindRenderSetupSerializeData(clone);
//                     clone.ReplaceTransform(newBones);
//                     clone.DisableAutoBuild();
//                     newcloths.Add(clone, newBones);
//                 }
//             }
//
//             await UniTask.Yield();
//             if (token.IsCancellationRequested)
//                 return;
//             
//             foreach (var kv in newcloths)
//             {
//                 MagicaCloth cloth = kv.Key;
//                 cloth.gameObject.SetActive(true);
//                 cloth.BuildAndRun();
//             }
//         }
//
//         private void SyncChildrenByName(Transform src, Transform dst, PartsType parts)
//         {
//             if (src == null || dst == null)
//                 return;
//
//             Dictionary<string, Transform> srcMap = new Dictionary<string, Transform>();
//             Dictionary<string, Transform> dstMap = new Dictionary<string, Transform>();
//
//             foreach (Transform t in src) srcMap[t.name] = t;
//             foreach (Transform t in dst) dstMap[t.name] = t;
//
//             foreach (var kv in srcMap)
//             {
//                 var name = kv.Key;
//                 var s = kv.Value;
//
//                 if (dstMap.ContainsKey(name))
//                     continue;
//
//                 Transform clone = UnityEngine.Object.Instantiate(s.gameObject).transform;
//                 clone.ResetTransform(dst, s);
//                 AddNewBone(clone);
//             }
//
//             foreach (var kv in srcMap)
//             {
//                 if (dstMap.TryGetValue(kv.Key, out var d) == false)
//                     continue;
//
//                 SyncChildrenByName(kv.Value, d, parts);
//             }
//
//             void AddNewBone(Transform newbone)
//             {
//                 AddCacheTransform(newbone);
//                 AddMagicaClothComponent(newbone, parts, NewMagicaClothsBone);
//                 _newBones.Add(newbone.gameObject);
//             }
//         }
//
//         private List<Renderer> RebindRenderer(IReadOnlyList<Renderer> rendererlist)
//         {
//             List<Renderer> result = new List<Renderer>();
//
//             Dictionary<PartsType, Renderer> findresult = GetSkinnedMeshRenderers(rendererlist.ToList(), PartsType.All);
//             foreach (var kv in findresult)
//             {
//                 if (MySkinnedMeshRenderers.TryGetValue(kv.Key, out SkinnedMeshRenderer smr) == false)
//                     continue;
//
//                 result.Add(smr);
//                
//             }
//
//             return result;
//         }
//         private List<ColliderComponent> RebindCollision(IReadOnlyList<ColliderComponent> colliders, PartsType settype)
//         {
//             List<ColliderComponent> result = new List<ColliderComponent>();
//             foreach (ColliderComponent component in colliders)
//             {
//                 if (component == null)
//                     continue;
//                 
//                 Transform findtrans = FindMatchingTransform(component.transform, _myRootTrans);
//                 ColliderComponent findcomponent = findtrans?.GetComponent<ColliderComponent>() ?? null;
//                 if (findcomponent != null)
//                 {
//                     result.Add(findcomponent);
//                     continue;
//                 }
//
//                 RemoveCacheTransform(findtrans);
//                 
//                 ColliderComponent clone = UnityEngine.Object.Instantiate(component.gameObject).GetComponent<ColliderComponent>();
//                 findtrans = GetNewBones(component.transform.parent, _myRootTrans);
//                 Transform cloneTrans = clone.transform;
//                 cloneTrans.ResetTransform(findtrans, component.transform);
//                 AddCacheTransform(cloneTrans);
//                 result.Add(clone);
//             }
//
//             return result;
//         }
//
//         private T AddNewMagicaComponent<T>(PartsType partstype, T src, Dictionary<PartsType, List<T>> cachemap) where T : ClothBehaviour
//         {
//             T component = UnityEngine.Object.Instantiate(src.gameObject).GetComponent<T>();
//             
//             if (cachemap.TryGetValue(partstype, out List<T> list))
//                 list.Add(component);
//             else
//                 cachemap.Add(partstype, new List<T>() { component });
//             
//             return component;
//         }
//
//         public void ApplyEquipAndMagicaCloth(Transform srctarget, PartsType settype)
//         {
//             if (srctarget == null)
//                 return;
//
//             Dictionary<PartsType, SkinnedMeshRenderer> targetlist = GetSkinnedMeshRenderers(srctarget, settype);
//             ApplyEquipAndMagicaCloth(targetlist);
//         }
//
//         private void ApplyEquipAndMagicaCloth(Dictionary<PartsType, SkinnedMeshRenderer> targetlist)
//         {
//             if (GetMatchingTransform(targetlist, out Dictionary<Transform, Dictionary<PartsType, SkinnedMeshRenderer>> dic) == false)
//                 return;
//             
//             foreach (var kv in dic)
//             {
//                 Transform target = kv.Key; 
//                 FindRootTrans(target, out Transform srcRootTrans, out Transform srcParentTrans);
//                 Dictionary<PartsType, List<MagicaCloth>> magicacloths = GetMagicaComponent<MagicaCloth>(target, PartsType.All);
//                 SetEquipWithMagicaCloth(kv.Value, srcRootTrans, magicacloths, PartsType.All);
//             }
//         }
//
//         private bool GetMatchingTransform(Dictionary<PartsType, SkinnedMeshRenderer> targetlist, out Dictionary<Transform, Dictionary<PartsType, SkinnedMeshRenderer>> result)
//         {
//             result = null;
//             
//             if (targetlist == null || targetlist.Count == 0)
//                 return false;
//
//             result = targetlist
//                 .GroupBy(kv => kv.Value.transform.parent)
//                 .ToDictionary(
//                     g => g.Key,
//                     g => g.ToDictionary(kv => kv.Key, kv => kv.Value)
//                 );
//
//             return true;
//         }
//
//         private void FindRootTrans(Transform src, out Transform srcRootTrans, out Transform srcParentTrans)
//         {
//             srcRootTrans = null;
//             srcParentTrans = null;
//             
//             SkinnedMeshRenderer findskinedmeshrenderer = src.GetComponentInChildren<SkinnedMeshRenderer>(true);
//             if (findskinedmeshrenderer == null)
//                 return;
//
//             srcParentTrans = findskinedmeshrenderer.transform.parent;
//             for (int i = 0; i < srcParentTrans.childCount; ++i)
//             {
//                 if (srcParentTrans.GetChild(i).IsEmptyObject())
//                 {
//                     srcRootTrans ??= srcParentTrans.GetChild(i);
//                     return;
//                 }
//             }
//         }
//
//         private void RebindRenderSetupSerializeData(MagicaCloth cloth)
//         {
//             ClothSerializeData sdata = cloth.SerializeData;
//
//             List<RenderSetupData> setupDatas = new List<RenderSetupData>();
//             
//             if (sdata.clothType == ClothProcess.ClothType.MeshCloth)
//             {
//                 foreach (var ren in sdata.sourceRenderers)
//                 {
//                     if (ren)
//                     {
//                         setupDatas.Add(new RenderSetupData(null, ren));
//                     }
//                     else
//                         return;
//                 }
//             }
//             else if (sdata.clothType == ClothProcess.ClothType.BoneCloth)
//             {
//                 setupDatas.Add(new RenderSetupData(null, RenderSetupData.SetupType.BoneCloth, cloth.ClothTransform, sdata.rootBones, null, sdata.connectionMode, cloth.name));
//             }
//             else if (sdata.clothType == ClothProcess.ClothType.BoneSpring)
//             {
//                 // BoneSpringではLine接続のみ
//                 setupDatas.Add(new RenderSetupData(null, RenderSetupData.SetupType.BoneSpring, cloth.ClothTransform, sdata.rootBones, sdata.colliderCollisionConstraint.collisionBones, RenderSetupData.BoneConnectionMode.Line, cloth.name));
//             }
//             else
//                 return;
//             
//             List<RenderSetupSerializeData> datas  = new List<RenderSetupSerializeData>();
//             foreach (var setup in setupDatas)
//             {
//                 var meshSetupData = new RenderSetupSerializeData();
//                 meshSetupData.Serialize(setup);
//                 datas.Add(meshSetupData);
//             }
//             
//             cloth.GetSerializeData2().initData.clothSetupDataList = datas;
//         }
//         
//         [ContextMenu("ClearObject")]
//         private void ReleaseMagicaCloth()
//         {
//             DestroyMagicaClothAll();
//             Dispose();
//         }
//
//         private void Dispose()
//         {
//             if (_tokenSource?.IsCancellationRequested == false)
//             {
//                 _tokenSource.Cancel();
//                 _tokenSource.Dispose();
//             }
//             _tokenSource = null;
//         }
//     }
// }
