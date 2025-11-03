using System;
using System.Collections.Generic;
using REIW.Animations;
using REIW.Animations.Character;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace REIW
{
    public class ClientCharacter : MonoBehaviour
    {
        public CharacterBase LogicalCharacter => _logicalCharacter;
        public Animator CharacterAnimator => _characterAnimator;
        public CharacterVisualAttachment VisualAttachment => _visualAttachment;
        public CharacterAnimation CharacterAnimation => _characterAnimation;
        public AnimancerEvents AnimancerEvents => _animancerEvents;
        public CharacterAvatarBoneMapper AvatarBoneMapper => _avatarBoneMapper;
        //public CharacterCustomizer CharacterCustomizer => _characterCustomizer;

        [SerializeField] private Animator _characterAnimator;
        [SerializeField] private CharacterVisualAttachment _visualAttachment;
        [SerializeField] private CharacterAnimation _characterAnimation;
        [SerializeField] private AnimancerEvents _animancerEvents;
        [SerializeField] private CharacterAvatarBoneMapper _avatarBoneMapper;
        //[SerializeField] private CharacterCustomizer _characterCustomizer;

        //[FormerlySerializedAs("_characterActionEffect")] public CharacterEffectSound _characterEffectSound;
        
        private CharacterBase _logicalCharacter;
        //private EnumRace _race;
        //private EnumGender _gender;
        
        // private Dictionary<EnumParts, ItemDto.ItemData> prevEquippedAvatarItems = new()
        // { 
        //     { EnumParts.Head , null}, { EnumParts.Top, null }, { EnumParts.Bottom, null }, { EnumParts.Shoes, null },
        //     { EnumParts.Back , null}, { EnumParts.Earring , null}, { EnumParts.Face1 , null}, { EnumParts.Face2 , null}, { EnumParts.Ring , null},
        // };
#if UNITY_EDITOR
        private void Start()
        {
            // ref Check
            if (_characterAnimator == null)
            {
                Debug.LogError("Character animator is null");
            }
            if (_visualAttachment == null)
            {
                Debug.LogError("Character _visualAttachment is null");
            }
            if (_characterAnimation == null)
            {
                Debug.LogError("Character _characterAnimation is null");
            }
            if (_animancerEvents == null)
            {
                Debug.LogError("Character _animancerEvents is null");
            }
            if (_avatarBoneMapper == null)
            {
                Debug.LogError("Character _avatarBoneMapper is null");
            }
            // if (_characterCustomizer == null)
            // {
            //     Debug.LogError("Character _characterCustomizer is null");
            // }
            //
            //>
        }
        
    #endif
        
        //type
        // ingame, outgame

        // outgame character Initialize
        // public void Initialize(EnumRace race, EnumGender gender)
        // {
        //     Initialize(null, race, gender, CharacterType.OutGame);
        // }

        //CharacterBase  Initialize에서 호출 (InGame)
        // public void Initialize(CharacterBase logicalCharacter, EnumRace race, EnumGender gender, CharacterType characterType = CharacterType.InGame)
        // {
        //     this._logicalCharacter = logicalCharacter;
        //     this._gender = gender;
        //     this._race = race;
        //     
        //     _characterAnimation.Init();
        //     
        //     // 순서 중요...
        //     _characterEffectSound = GetComponent<CharacterEffectSound>();
        //     if (_characterEffectSound != null)
        //     {
        //         _characterEffectSound.Initialize(logicalCharacter, this, characterType);
        //     }
        //     else
        //     {
        //         Debug.LogError("Character ActionEffect not found");
        //         _characterEffectSound = this.transform.AddComponent<CharacterEffectSound>();
        //         _characterEffectSound.Initialize(logicalCharacter, this, characterType);
        //     }
        //     
        //     _animancerEvents.Initialize(this);
        // }


        // public void RefreshAppearance(AvatarData avatarData)
        // {
        //     _characterCustomizer.SetAvatarData(avatarData);
        //     _characterCustomizer.RefreshAppearance();
        // }
        //
        //
        // public void EquipClothes(PlayerEquipDto.PlayerEquipData topEquipData, PlayerEquipDto.PlayerEquipData bottomEquipData, PlayerEquipDto.PlayerEquipData shoesEquipData)
        // {
        //     EquipClothes(topEquipData, EnumParts.Top);
        //     EquipClothes(bottomEquipData, EnumParts.Bottom);
        //     EquipClothes(shoesEquipData, EnumParts.Shoes);
        // }
        //
        // private void EquipClothes(PlayerEquipDto.PlayerEquipData equipAvatarData, EnumParts enumParts, int lodLevel = 0)
        // {
        //     ItemDto.ItemData newEquippedAvatarItem = null;
        //     
        //     string modelPath = string.Empty;
        //     EnumRace itemRace = _race;
        //     
        //     if (equipAvatarData != null)
        //     {
        //         newEquippedAvatarItem = GameDataModel.Singleton.ItemData.GetItemData(equipAvatarData.Category, equipAvatarData.Kind, equipAvatarData.Serial);
        //         var avatarItemSO = newEquippedAvatarItem.GetItemDataSO<ItemDataAvatarSO>();
        //         if (avatarItemSO != null)
        //         {
        //             itemRace = avatarItemSO.Race;
        //             modelPath = lodLevel switch  // [suhlee] TODO: LOD별 렌더러 통합 시 파츠 3개(0,1,2) 한 번에 착용 필요
        //             {
        //                 0 => avatarItemSO.Item_ModelLOD0,
        //                 1 => avatarItemSO.Item_ModelLOD1,
        //                 2 => avatarItemSO.Item_ModelLOD2,
        //                 _ => string.Empty
        //             };                    
        //         }
        //     }
        //     if (modelPath == string.Empty)
        //     {
        //         newEquippedAvatarItem = null;
        //         modelPath = DataTable.Singleton.GetCharacterDefaultPartsName(_race, _gender, enumParts, lodLevel);
        //     }
        //     
        //     bool isSameItemEquipped = (newEquippedAvatarItem == prevEquippedAvatarItems[enumParts]);
        //     if (!isSameItemEquipped && CharacterPartsController.TryConvertToPartsType(enumParts, out var partsType))
        //     {
        //         var avatarItemModel = AssetManager.Singleton.InstantiateAvatarPartsModel(itemRace, modelPath);
        //         if (avatarItemModel != null)
        //         {
        //             _characterCustomizer.CharacterPartsController.ApplyEquipAndMagicaCloth(avatarItemModel.transform, partsType);
        //             Destroy(avatarItemModel);
        //             prevEquippedAvatarItems[enumParts] = newEquippedAvatarItem;
        //         }
        //     }
        // }
        
    }
}
