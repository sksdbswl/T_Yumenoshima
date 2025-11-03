using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace REIW
{
    [RequireComponent(typeof(Animator))]
    public class CharacterPartsControllerTester : CacheMonoBehaviour
    {
        [Serializable]
        public class PartRendererBinding
        {
            [FormerlySerializedAs("PartType")]
            [CharacterPartsController.EnumOffsetPreviewAttribute()]
            public CharacterPartsController.PartsType characterPartType;
            public SkinnedMeshRenderer Renderer;
        }
        
        [Serializable] 
        public class TransformBindingList 
        {
            public Transform GroupTransform;
            public List<PartRendererBinding> PartRendererBindings = new();
        }

        [SerializeField]
        private List<TransformBindingList> _transformBindingLists = new();
        [SerializeField]
        private Transform _targetObject = null;

        [SerializeField]
        private ulong _equippartsScriptID = 0;
        [SerializeField]
        private EnumGender _equippartsGender = EnumGender.Male;

        [SerializeField]
        private Transform ChangeRoot;
        
        private CharacterLODPartsController  _controller = null;
        public CharacterLODPartsController Controller => _controller ??= new CharacterLODPartsController(this.transform);
        
        private void Awake()
        {
            _transformBindingLists.Clear();
            foreach (CharacterLODPartsController.GroupSkinnedMeshRenderer group in Controller.MyGroupSkinnedMeshRenderers)
            {
                TransformBindingList find = _transformBindingLists.Find(x=> x.GroupTransform == group.GroupTransform);
                if (find == null)
                {
                    find = new TransformBindingList
                    {
                        GroupTransform = group.GroupTransform,
                        PartRendererBindings = new List<PartRendererBinding>()
                    };
                    _transformBindingLists.Add(find);
                }
                
                foreach (var pair in group.PartsController.MySkinnedMeshRenderers)
                {
                    find.PartRendererBindings.Add(new PartRendererBinding
                    {
                        characterPartType = pair.Key,
                        Renderer = pair.Value
                    });
                }
            }
        }

        [ContextMenu("Rebuild Cache")]
        private void RebuildCache()
        {
            Controller.RebuildCache(0, MyTransform);
        }

        [ContextMenu("EquipAndMagica")]
        private void SetEquipAndMagica()
        {
            Controller.SetEquipAndMagicacloth(0, _targetObject, CharacterPartsController.PartsType.All);
        }

        [ContextMenu("Equip")]
        private void SetEquip()
        {
            Controller.SetEquip(0, _targetObject, CharacterPartsController.PartsType.All);
        }
        
        [ContextMenu("UnEquip")]
        private void UnEquip()
        {
            Controller.UnEquipParts(0);
        }
        
        [ContextMenu("UnEquipLast")]
        public void UnEquipLast()
        {
            Controller.UnEquipLast(0);
        }

        [ContextMenu("Save")]
        public void Save()
        {
            Controller.Save(0);
        }
        
        [ContextMenu("EquipDefaultGender")]
        public void EquipDefaultGender()
        {
             IDictionary<CharacterPartsController.PartsType, Transform> dic = GameDataModel.Singleton.EquipPartsDatas.DefulatParts(_equippartsGender);
             Controller.SetAllEquip(CharacterPartsControllerBase.ALL_LOD, dic);
        }

        [ContextMenu("EquipFromScript")]
        public void EquipFromScript()
        {
             EquipPartsDataSO.DataInfo info = GameDataModel.Singleton.EquipPartsDatas.GetDataInfo(_equippartsScriptID);
             Controller.SetEquip(0, info);
        }
        
        [ContextMenu("ResetParts")]
        public void ResetParts()
        {
            Controller.ResetParts(0);
        }
        
        [ContextMenu("SetLODFromSript")]
        public void SetLODFromSript()
        {
            Controller.SetEquip(_equippartsScriptID);
        }

        [ContextMenu("AnimatorRebind")]
        public void AnimatorRebind()
        {
            if (ChangeRoot == null)
                return;
            GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach(x =>
            {
                if (x.rootBone == null)
                    return;
                
                x.rootBone = ChangeRoot.FindAllChild(x.rootBone.name);
            });
            GetComponent<Animator>().Rebind();
        }

    }
}
