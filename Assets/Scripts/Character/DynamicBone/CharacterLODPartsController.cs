using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace REIW
{
    public abstract class CharacterPartsControllerBase
    {
        public static int ALL_LOD = 100;
    }

    public class CharacterLODPartsController : CharacterPartsControllerBase
    {
        public CharacterLODPartsController(Transform myTransform, SkinnedMeshRenderer[] skinnedMeshRenderers = null)
        {
            Initialize(myTransform, skinnedMeshRenderers);
        }

        [Serializable]
        public class GroupSkinnedMeshRenderer
        {
            public CharacterPartsController PartsController;
            public Transform GroupTransform;
            public SkinnedMeshRenderer[] SkinnedMeshRenderers;
        }

        private List<GroupSkinnedMeshRenderer> _groupSkinnedMeshRenderers = new();
        public IList<GroupSkinnedMeshRenderer> MyGroupSkinnedMeshRenderers => _groupSkinnedMeshRenderers;

        private Transform _myTransform;
        private Transform _rootBone;

        private void Initialize(Transform myTransform, SkinnedMeshRenderer[] skinnedMeshRenderers = null)
        {
            _myTransform = myTransform;

            skinnedMeshRenderers ??= _myTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
            _groupSkinnedMeshRenderers = skinnedMeshRenderers
                .Where(r => r != null)
                .GroupBy(r => r.transform.parent)
                .Select(g =>
                {
                    var controller = new CharacterPartsController(g.Key);

                    return new GroupSkinnedMeshRenderer
                    {
                        PartsController = controller,
                        GroupTransform = g.Key,
                        SkinnedMeshRenderers = controller.MySkinnedMeshRenderers.Values.ToArray(),
                    };
                })
                .OrderBy(x => x.GroupTransform.name, StringComparer.Ordinal)
                .ToList();

            _rootBone = FindRootBone(_myTransform.transform, _groupSkinnedMeshRenderers.Select(x => x.GroupTransform).ToList());
            _rootBone ??= _myTransform.FindAllChild("root"); 

            Transform FindRootBone(Transform targettrans, IList<Transform> list)
            {
                foreach (Transform t in targettrans)
                {
                    if (t.IsEmptyObject() && list.Contains(t) == false)
                        return t;
                }

                return null;
            }
        }

        public void Save(int index) => ExecuteForTargets(index, x => x.Save());
        public void RebuildCache(int index, Transform srctarget) => ExecuteForTargets(index, x => x.RebuildCache(srctarget));
        public void UnEquipParts(int index) => ExecuteForTargets(index, x => x.UnEquipParts());
        public void UnEquipLast(int index) => ExecuteForTargets(index, x => x.UnEquipLast());
        public void ResetParts(int index) => ExecuteForTargets(index, x => x.ResetParts());
        public void SetAllEquip(int index, IDictionary<CharacterPartsController.PartsType, Transform> targetlist) => ExecuteForTargets(index, x => x.SetAllEquip(targetlist));
        public void SetEquip(int index, Transform srctarget, CharacterPartsController.PartsType settype) => ExecuteForTargets(index, x => x.SetEquip(srctarget, settype));
        //public void SetEquipAndMagicacloth(int index, Transform srctarget, CharacterPartsController.PartsType settype) => ExecuteForTargets(index, x => x.ApplyEquipAndMagicaCloth(srctarget, settype));
        //public void SetEquip(int index, EquipPartsDataSO.DataInfo info) => ExecuteForTargets(index, x => x.SetEquip(info));

        // public void SetEquipAll()
        // {
        //     for (EnumCategory category = EnumCategory.Top; category <= EnumCategory.Ring; ++category)
        //     {
        //         GameDataModel.Singleton.ItemData.GetAvatarItemData(category, 0);    
        //     }
        // }
        //
        // public void SetEquip(ulong scriptid)
        // {
        //     EquipPartsDataSO.DataInfo info = GameDataModel.Singleton.EquipPartsDatas.GetDataInfo(scriptid);
        //     if (info == null)
        //         return;
        //
        //     for (EquipPartsDataSO.ObjectLOD lod = EquipPartsDataSO.ObjectLOD.LOD1; lod <= EquipPartsDataSO.ObjectLOD.LOD3; ++lod)
        //     {
        //         GameObject obj = info.GetLODObject(lod);
        //         if (obj == null)
        //         {
        //             GetGroupPartsController((int)lod).EmptyEquip(info.Parts);
        //             continue;
        //         }
        //
        //         GetGroupPartsController((int)lod).SetEquip(obj.transform, info.Parts);
        //     }
        // }

        // private IEnumerable<CharacterPartsController> ResolveTargets(int index)
        // {
        //     if (index == ALL_LOD)
        //     {
        //         foreach (EquipPartsDataSO.ObjectLOD lod in Enum.GetValues(typeof(EquipPartsDataSO.ObjectLOD)))
        //             yield return GetGroupPartsController((int)lod);
        //     }
        //     else
        //     {
        //         yield return GetGroupPartsController(index);
        //     }
        // }

        private void ExecuteForTargets(int index, Action<CharacterPartsController> action)
        {
            // foreach (var controller in ResolveTargets(index))
            //     action(controller);
        }

        private CharacterPartsController GetGroupPartsController(int lod)
        {
            System.Index groupindex = lod < _groupSkinnedMeshRenderers.Count ? lod : ^1;
            return _groupSkinnedMeshRenderers[groupindex].PartsController;
        }
    }
}
