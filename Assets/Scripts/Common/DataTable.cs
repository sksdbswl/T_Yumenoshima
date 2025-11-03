// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace REIW
// {
//     public class DataTable : SingletonBase<DataTable>
//     {
//         [Header("Database")]
//         public BGMAudioPathSO bgmAudioPathSO;
//         public SfxDatabaseSO sfxDatabaseSO;
//         public EffectDatabaseSO effectDatabaseSO;
//         public DyeColorDatabaseSO dyeColorDatabaseSO;
//         public CharacterPrefabSO characterPrefabSO;
//         public InteractionDatabaseSO interactionDatabaseSo;
//         public NpcDialogDatabaseSO npcDialogDatabaseSO;
//
//         public override void Init()
//         {
//             if (characterPrefabSO != null)
//             {
//             }
//
//             if (dyeColorDatabaseSO != null)
//             {
//                 foreach (var item in dyeColorDatabaseSO.items)
//                 {
//                     if (!_dyeColorEntries.ContainsKey(item.DyeIndex))
//                     {
//                         _dyeColorEntries.Add(item.DyeIndex, item);
//                     }
//                     else
//                     {
//                         Debug.LogWarning("Duplicate DyeIndex: " + item.DyeIndex);
//                     }
//                 }
//             }
//
//             // 순서 중요
//             if (sfxDatabaseSO != null)
//             {
//                 SoundManager.Singleton.Initialize();
//
//                 _categories.Clear();
//                 foreach (var cat in sfxDatabaseSO.categories)
//                 {
//                     if (!_categories.ContainsKey(cat.id))
//                     {
//                         _categories.Add(cat.id, cat);
//
//                         SoundManager.Singleton._sfxManager.CreatePool(cat.id, cat);
//
//                         // // 초기 프리워밍이 필요한 경우에만 미리 풀 생성
//                         // if (cat.prewarmVoices > 0)
//                         // {
//                         //     SoundManager.Singleton._sfxManager.GetOrCreatePool(cat.id, cat);
//                         // }
//                     }
//                     else
//                     {
//                         Debug.LogWarning("Duplicate _categories index: " + cat.id);
//                     }
//                 }
//
//                 _items.Clear();
//                 foreach (var item in sfxDatabaseSO.items)
//                 {
//                     item.SetCategory(_categories[item.category]);
//                     if (!_items.ContainsKey(item.index))
//                     {
//                         _items.Add(item.index, item);
//                     }
//                     else
//                     {
//                         Debug.LogWarning("Duplicate Sfx index: " + item.index);
//                     }
//                 }
//             }
//
//             if (effectDatabaseSO != null)
//             {
//                 foreach (var item in effectDatabaseSO.Items)
//                 {
//                     if (!_defs.TryAdd(item.EffectIndex, item))
//                     {
//                         Debug.LogWarning("Duplicate Effect index: " + item.EffectIndex);
//                     }
//                 }
//             }
//
//             if (interactionDatabaseSo != null)
//             {
//                 foreach (var item in interactionDatabaseSo.Items)
//                 {
//                     if (!_interactions.TryAdd(item.index, item))
//                     {
//                         Debug.LogWarning("Duplicate Interaction index: " + item.index);
//                     }
//                 }
//             }
//         }
//
//         #region DyeColor
//
//         readonly Dictionary<int, DyeColorDatabaseSO.DyeColorEntry> _dyeColorEntries = new();
//
//         public DyeColorDatabaseSO.DyeColorEntry GetDyeColorEntry(int dyeIndex)
//         {
//             return _dyeColorEntries[dyeIndex];
//         }
//
//         #endregion
//
//         #region BGM
//
//         public BGMSoundData GetTitleBGMPath()
//         {
//             if (bgmAudioPathSO == null || bgmAudioPathSO.TitleBGMData == null)
//             {
//                 Debug.LogWarning("[BGM] TitleBGMData 가 설정되어 있지 않습니다.");
//                 return null;
//             }
//
//             return bgmAudioPathSO.TitleBGMData;
//         }
//
//         public BGMSoundData GetBGMSoundData(ulong fieldID, ulong zoneID)
//         {
//             return bgmAudioPathSO.GetBGM(fieldID, zoneID);
//         }
//
//         #endregion
//
//         #region SFX
//
//         readonly Dictionary<int, SfxDatabaseSO.SfxEntry> _items = new();
//         readonly Dictionary<SfxCategory, SfxDatabaseSO.Category> _categories = new();
//
//         public SfxDatabaseSO.SfxEntry GetEntry(int index)
//         {
//             if (_items.ContainsKey(index))
//                 return _items[index];
// #if UNITY_EDITOR
//             LogUtil.LogError("Is Not Contain Entry:" + index);
// #endif
//             return null;
//         }
//
//         public SfxDatabaseSO.Category GetCategory(SfxCategory category)
//         {
//             return _categories[category];
//         }
//
//         #endregion
//
//         #region Effect
//
//         readonly Dictionary<int, EffectDatabaseSO.EffectEntry> _defs = new();
//
//         public EffectDatabaseSO.EffectEntry GetEffectEntry(int effectIndex)
//         {
//             if (_defs.ContainsKey(effectIndex))
//             {
//                 return _defs[effectIndex];
//             }
//
//             return null;
//         }
//
//         #endregion
//
//         #region Interaction
//
//         private readonly Dictionary<InteractionType, InteractionDatabaseSO.InteractionData> _interactions = new();
//
//         public string GetInteractionIcon(InteractionType interactionType)
//         {
//             return _interactions.TryGetValue(interactionType, out var data) ? data.InteractionIcon : "";
//         }
//
//         public string GetInteractionDesc(InteractionType interactionType)
//         {
//             return _interactions.TryGetValue(interactionType, out var data) ? data.InteractionDesc : "";
//         }
//
//         #endregion
//
//         #region characterPrefabSO
//
//         public String GetCharacterBoneAddress(EnumRace race, EnumGender gender)
//         {
//             return characterPrefabSO.GetAddress(race, gender);
//         }
//
//         public string GetCharacterDefaultPartsName(EnumRace race, EnumGender gender, EnumParts parts, int lodLevel)
//         {
//             return characterPrefabSO.GetDefaultPartsName(race, gender, parts, lodLevel);
//         }
//
//         #endregion
//
//         #region NpcDialog
//
//         // NPC Start Dialog 요청
//         
//         /// <summary>
//         /// NPC Start Dialog 요청
//         /// </summary>
//         /// <param name="DialogGroupID"></param>
//         /// <returns> NpcDialog (대화) </returns>
//         public NpcDialogDatabaseSO.NpcDialog GetNpcStartDialog(int dialogGroupID)
//         {
//             NpcDialogDatabaseSO.NpcDialogSelectInfo info = GetNpcSelectDialogSelectInfo(dialogGroupID);
//             if (info == null)
//             {
//                 Debug.LogError("FUNC GetNpcStartDialog Info Is Null. DialogGroupID-" + dialogGroupID);
//                 return null;
//             }
//             int dialogID = info.GetStartDialog();
//             return GetNpcDialogData(dialogID);
//         }
//         
//         /// <summary>
//         /// NPC Start 이후에 나오는 인터렉션 리스트들.. 
//         /// </summary>
//         /// <param name="DialogGroupID"></param>
//         /// <returns>  </returns>
//         public List<NpcDialogDatabaseSO.NpcDialogInteraction> GetNpcInteractionInfo(int dialogGroupID)
//         {
//             NpcDialogDatabaseSO.NpcDialogSelectInfo info = GetNpcSelectDialogSelectInfo(dialogGroupID);
//             if (info == null)
//             {
//                 Debug.LogError("FUNC GetNpcDialog Info Is Null. DialogGroupID-" + dialogGroupID);
//                 return null;
//             }
//             List<int> dialogIDs = info.GetNpcDialogs();
//             
//             List<NpcDialogDatabaseSO.NpcDialogInteraction> interactions = new();
//             for (int i = 0; i < dialogIDs.Count; i++)
//             {
//                 NpcDialogDatabaseSO.NpcDialogInteraction interaction = GetNpcDialogInteraction(dialogIDs[i]);
//                 if(interaction != null)
//                     interactions.Add(interaction);
//             }
//             return interactions;
//         }
//         
//         
//         /// <summary>
//         /// 선택 관련 대화가 나왓을때 호출해서... 선택지를 방아감..
//         /// </summary>
//         /// <param name="dialogID">대화 ID</param>
//         /// <returns>선택지 정보들이 들어가있는 타입</returns>
//         public NpcDialogDatabaseSO.NpcDialogChoice GetNpcDialogChoice( int dialogID)
//         {
//             foreach (var item in npcDialogDatabaseSO.NpcDialogChoices)
//             {
//                 if (item.DialogID == dialogID)
//                 {
//                     return item;
//                 }
//             }
//             return null;
//         } 
//         
//         /// <summary>
//         /// 인터렉션 리스트에서 선택된 다이어로그 또는 선택지에서 선택된 다이어로그 데이터 호출 
//         /// </summary>
//         /// <param name="dialogID">선택된 다이어 로그 ID</param>
//         public NpcDialogDatabaseSO.NpcDialog OnClickNpcDialogInteraction(int dialogID)
//         {
//             return GetNpcDialogData(dialogID);
//         }
//         
//         private NpcDialogDatabaseSO.NpcDialogSelectInfo GetNpcSelectDialogSelectInfo( int DialogGroupID)
//         {
//             foreach (var item in npcDialogDatabaseSO.NpcDialogSelectInfos)
//             {
//                 if (item.DialogGroupID == DialogGroupID)
//                 {
//                     return item;
//                 }
//             }
//             return null;
//         }
//
//         private NpcDialogDatabaseSO.NpcDialog GetNpcDialogData( int DialogID)
//         {
//             foreach (var item in npcDialogDatabaseSO.NpcDialogs)
//             {
//                 if (item.DialogID == DialogID)
//                 {
//                     return item;
//                 }
//             }
//             return null;
//         }
//         
//         private NpcDialogDatabaseSO.NpcDialogInteraction GetNpcDialogInteraction( int DialogID)
//         {
//             foreach (var item in npcDialogDatabaseSO.NpcDialogInteractions)
//             {
//                 if (item.DialogID == DialogID)
//                 {
//                     return item;
//                 }
//             }
//             return null;
//         }
//         #endregion
//         
//     }
//     
//     //임시
//     public enum eNpcDialogeLineType //int 
//     {
//         Continue   = 0,
//         
//         Choice = 1,
//         End  = 2,
//     }
// }