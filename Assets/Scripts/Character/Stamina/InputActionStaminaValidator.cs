// using System;
// using UnityEngine;
//
// namespace REIW
// {
//     /// <summary>
//     /// 이 class는 캐릭터 액션에 대한 스태미나 실행/검증(사용 가/불)/차감을 담당합니다.
//     /// </summary>
//     public class InputActionStaminaValidator //: IInputActionValidator
//     {
//         //==========================================================================================
//         // 스태미나 사용 성공 callback - 성공 시, 수행이 필요한 이벤트를 구독해주세요.
//         public static event Action<eStaminaActionType, float> OnStaminaUserSuccess;
//         // 스태미나 사용 실패 callback - 실패 시, 수행이 필요한 이벤트를 구독해주세요.
//         public static event Action<eStaminaActionType, float> OnStaminaUseFailed;
//         // 스태미나 수치가 0이 되었을 때 callback - 0이 되었을 때, 수행이 필요한 이벤트를 구독해주세요.
//         public static event Action OnStaminaDepleted;
//         //==========================================================================================
//         /// <summary>
//         /// 스태미나 액션 실행 가능 여부 검증 및 차감
//         /// </summary>
//         /// <param name="actionType">실행하려는 액션 타입</param>
//         /// <returns>true: 실행 가능 및 차감 완료, false: 실행 불가</returns>
//         public bool TryUseStamina(eStaminaActionType actionType)
//         {
//             // 탈것 탑승 중인지 확인
//             bool isRiding = PlayerController.Instance.IsRidingMount;
//             
//             // 현재 스태미나와 필요 스태미나 계산
//             float currentStamina = isRiding 
//                 ? UserDataModel.Singleton.PlayerInfoData.StatData.MountStamina
//                 : UserDataModel.Singleton.PlayerInfoData.StatData.Stamina;
//             float staminaCost = GameDataModel.Singleton.StaminaActionData.GetStaminaCost(actionType);
//             float resultStamina = currentStamina - staminaCost;
//             
//             // 스태미나 부족 체크
//             if (currentStamina < staminaCost)
//             {
//                 // 스태미나 부족 이벤트 발생
//                 OnStaminaUseFailed?.Invoke(actionType, resultStamina);
//                 return false;
//             }
//             
//             // 스태미나 차감
//             if (isRiding)
//                 UserDataModel.Singleton.PlayerInfoData.StatData.MountStamina = resultStamina;
//             else
//                 UserDataModel.Singleton.PlayerInfoData.StatData.Stamina = resultStamina;
//             
//             // 스태미나 사용 성공 이벤트
//             OnStaminaUserSuccess?.Invoke(actionType, staminaCost);
//             
//             return true;
//         }
//         
//         /// <summary>
//         /// 액션 실행 가능 여부만 확인 (차감하지 않음)
//         /// </summary>
//         public bool CanStaminaAction(eStaminaActionType actionType)
//         {
//             if (PlayerController.Instance.IsStandalone)
//                 return true;
//
//             bool isRiding = PlayerController.Instance.IsRidingMount;
//             
//             float currentStamina = isRiding 
//                 ? UserDataModel.Singleton.PlayerInfoData.StatData.MountStaminaa
//                 : UserDataModel.Singleton.PlayerInfoData.StatData.Stamina;
//                 
//             float requiredStamina = GameDataModel.Singleton.StaminaActionData.GetStaminaCost(actionType);
//             
//             return currentStamina >= requiredStamina;
//         }
//         
//         // /// <summary>
//         // /// 액션 실행 가능 여부만 확인 (차감하지 않음)
//         // /// </summary>
//         // public bool CanExecute(eStaminaActionType actionType)
//         // {
//         //     if (PlayerController.Instance.IsStandalone)
//         //         return true;
//         //
//         //     if (!TryMapInputToStaminaAction(actionType, out var staminaAction))
//         //         return false; // 매핑되지 않은 액션은 항상 비허용
//         //
//         //     var actionData = GameDataModel.Singleton.StaminaActionData.GetStaminaActionData(staminaAction);
//         //     if (actionData is null)
//         //         return false;
//         //
//         //     var currentStamina = GetCurrentStamina(staminaAction);
//         //
//         //     // 1. 1회성 소모 판단
//         //     if (actionData.InstanceUse > 0f)
//         //         return currentStamina >= actionData.InstanceUse;
//         //
//         //     // 2. 지속 소모 판단
//         //     if (actionData.ConsumptionPerSecond > 0f)
//         //     {
//         //         // LogUtil.Log("currentStamina:" + currentStamina);
//         //         // LogUtil.Log("actionData.ConsumptionPerSecond * Constant.SnapShotInterval:" + actionData.ConsumptionPerSecond * Constant.SnapShotInterval);
//         //         return currentStamina >= actionData.ConsumptionPerSecond * Constant.SnapShotInterval;
//         //     }
//         //
//         //     // 3. 소모 없음
//         //     return true;
//         // }
//
//         private bool TryMapInputToStaminaAction(eStaminaActionType inputAction, out eStaminaActionType eStaminaActionType)
//         {
//             eStaminaActionType = inputAction switch
//             {
//                 // Character
//                 eStaminaActionType.Dash => eStaminaActionType.Dash,
//                 eStaminaActionType.Sprint => eStaminaActionType.Sprint,
//                 eStaminaActionType.Swim => eStaminaActionType.Swim,
//                 eStaminaActionType.SwimSprint => eStaminaActionType.SwimSprint,
//                 eStaminaActionType.Glide => eStaminaActionType.Glide,
//                 eStaminaActionType.GlideJump => eStaminaActionType.GlideJump,
//                 eStaminaActionType.WallClimb => eStaminaActionType.WallClimb,
//                 eStaminaActionType.Grapple => eStaminaActionType.Grapple,
//
//                 // Riding
//                 eStaminaActionType.Summon => eStaminaActionType.Summon,
//                 eStaminaActionType.Riding => eStaminaActionType.Riding,
//                 eStaminaActionType.Surfing => eStaminaActionType.Surfing,
//                 eStaminaActionType.Flying => eStaminaActionType.Flying,
//                 eStaminaActionType.FlyWalk => eStaminaActionType.FlyWalk,
//                 eStaminaActionType.FlyUp => eStaminaActionType.FlyUp,
//                 eStaminaActionType.FlyDown => eStaminaActionType.FlyDown,
//                 eStaminaActionType.RidingSprint => eStaminaActionType.RidingSprint,
//                 eStaminaActionType.SurfingSprint => eStaminaActionType.SurfingSprint,
//                 eStaminaActionType.FlyingSprint => eStaminaActionType.FlyingSprint,
//                 eStaminaActionType.Trick => eStaminaActionType.Trick,
//                 
//                 _ => eStaminaActionType.Normal // Default
//             };
//
//             return eStaminaActionType != eStaminaActionType.Normal; // Default
//         }
//
//         private float GetCurrentStamina(eStaminaActionType actionType)
//         {
//             var stat = UserDataModel.Singleton.PlayerInfoData.StatData;
//
//             return actionType switch
//             {
//                 // Normal 스태미나 사용 액션
//                 eStaminaActionType.Dash or eStaminaActionType.Sprint or eStaminaActionType.Swim or
//                 eStaminaActionType.SwimSprint or eStaminaActionType.Glide or eStaminaActionType.GlideJump or
//                 eStaminaActionType.WallClimb or eStaminaActionType.Grapple
//                     => stat.Stamina,
//
//                 // Riding 스태미나 사용 액션
//                 eStaminaActionType.Summon  or eStaminaActionType.Riding or eStaminaActionType.Surfing or 
//                 eStaminaActionType.Flying or eStaminaActionType.FlyWalk or eStaminaActionType.FlyUp or eStaminaActionType.FlyDown or
//                 eStaminaActionType.RidingSprint or eStaminaActionType.SurfingSprint or eStaminaActionType.FlyingSprint or eStaminaActionType.Trick 
//                     => stat.MountStamina,
//
//                 _ => 0f,
//             };
//         }
//         //==========================================================================================
//     }
// }
