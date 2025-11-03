using UnityEngine;
using REIW.Animations.Character;
using Unity.Collections;

namespace REIW
{
    public partial class PlayerController
    {
        [field: Header("Mount Settings")]
        // [field: SerializeField, ReadOnly] public LocalMountBase EquippedLocalMount { get; private set; }
        // [field: SerializeField] private UDictionary<uint, LocalMountBase> PlayerMountPrefabs;
        [field: SerializeField] public UDictionary<uint, GameObject> PlayerMountList;
        private Transform demountTransform;
        private Transform _characterParent;
        
        private string spawnMountAnimationKey = "MountSummon";
        private string deMountAnimationKey = "DeMount";
        private string isEndPlayerSummonAnim = "IsEndPlayerSummon";
        
        
        // 탑승/하차 상태 플래그
        public bool IsMounting = false;
        public bool IsDemounting = false;

        protected virtual void OnExecuteMount()
        {
            // Mount 또는 Demount 중이면 입력 무시
            if (IsMounting || IsDemounting)
            {
                LogUtil.Log("탑승 입력 무시");
                return;
            }

            if (!CanChangeMountState())
            {
                LogUtil.Log("Can not Execute Mount.");
                return;
            }

            // 어떤 지형(Ground, Water 등) 위에 있는지 검사 후, 소환 가능한 타입으로 소환 진행
            // 우선 Riding만 고려
            // var playerMountData = UserDataModel.Singleton.PlayerMountData.TryGetEquippedMount(MountType.Riding);
            // EquippedLocalMount = PlayerMountList[playerMountData.Serial].GetComponent<LocalMountBase>();
            
            // if (EquippedLocalMount is null)
            // {
            //     LogUtil.LogError("Mount not found.");
            //     return;
            // }

            // if (!isRidingMount)
            // {
            //     // 현재 애니메이션 상태가 대시인지 체크 (Mount 상태는 허용)
            //     var currentState = LinkedCharacter?.CharacterAnimation?.StateMachine?.CurrentState;
            //     if (currentState is DashAnimationState)
            //     {
            //         LogUtil.LogWarning("대시 중에는 Mount를 소환할 수 없습니다.");
            //         return;
            //     }
            //     
            //     IsMounting = true;
            //     CurrentInputCommandModeType = eInputCommandModeType.Riding;
            //     CurrentExecuteActionTypeStateType = eStaminaActionType.Riding;
            //     OnMountSpawnEndCallback(false);
            //     ReNetworkClient.Singleton.REQ_FIELD_MOUNT_INSERT(MountType.Riding, () =>
            //         {
            //             var mountNetObj = EquippedLocalMount.GetComponent<MountNetObject>();
            //             if (mountNetObj == null)
            //             {
            //                 LogUtil.LogError("MountNetObject component not found.");
            //                 return;
            //             }
            //             mountNetObj.SetNetID(playerMountData.Category, playerMountData.Kind, playerMountData.Serial, playerMountData.DatabaseID);
            //             IngameFieldSubjectSystem.RegisterOwnerMount(mountNetObj);
            //             Mount();
            //         });
            // }
            // else
            // {
            //     IsDemounting = true;
            //     demountTransform = EquippedLocalMount.transform;
            //     ReNetworkClient.Singleton.REQ_FIELD_MOUNT_DELETE(playerMountData.Category, playerMountData.Kind,
            //         playerMountData.Serial, playerMountData.DatabaseID, DeMount);
            // }
        }

        protected void Mount()
        {
            // if (!EquippedLocalMount)
            //     return;
            //
            // isRidingMount = true;
            // LinkedCharacter.LockMoveInput = true;
            // EquippedLocalMount.ResetInputs();
            //
            // // 0) Mount 활성화 (월드 스냅은 Mount 쪽 transform만)
            // Quaternion rotation = Quaternion.Euler(0, LinkedCharacter.transform.rotation.eulerAngles.y, 0);
            // EquippedLocalMount.transform.SetPositionAndRotation(LinkedCharacter.transform.position, rotation);
            // EquippedLocalMount.gameObject.SetActive(true);
            // EquippedLocalMount.SetColliderParent(LinkedCharacter.transform);
            //
            // var motorBike = EquippedLocalMount as LocalVehicleBase;
            // Transform seat = (motorBike != null && motorBike.Seat != null) ? motorBike.Seat : EquippedLocalMount.transform;
            //
            // // 1) 탑승 전 KCC/Collider OFF
            // var local = LinkedCharacter;
            // var motor = local.Motor;
            //
            // if (motor.Capsule)
            //     motor.Capsule.enabled = false;
            //
            // // CharacterController의 속도도 리셋
            // EquippedLocalMount.GetComponent<CharacterController>().enabled = true;
            // if (EquippedLocalMount.GetComponent<CharacterController>() is var mountCC && mountCC != null)
            //     mountCC.Move(Vector3.zero); // Mount의 잔류 속도 제거
            //
            // LinkedCharacter.SetActiveCharacterPhysics(false);
            //
            // // 3) 부모 교체: 반드시 worldPositionStays = false
            // _characterParent = local.transform.parent;
            // local.transform.SetParent(seat, worldPositionStays: false);
            // local.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //
            // // 카메라/ UI
            // EquippedLocalMount.CameraTarget.transform.rotation = LinkedCharacter.CameraTarget.transform.rotation;
            // UpdateCamera(EquippedLocalMount.transform, EquippedLocalMount.CameraTarget);
            // UpdateUIState(eInputCommandModeType.Riding);
            //
            // // 애니메이션 상태 - Mount Serial 설정 후 상태 전환
            // EventBus?.Post<ICharacterBaseEventListener>(l => l.OnMountRequested());
            // var anim = LinkedCharacter?.CharacterAnimation;
            // var mountAnimState = LinkedCharacter?.CharacterAnimation?.StateMachine?.CurrentState as MountAnimationState;
            // if (mountAnimState != null)
            // {
            //     var playerMountData = UserDataModel.Singleton.PlayerMountData.TryGetEquippedMount(MountType.Riding);
            //     mountAnimState.SetMountSerial(playerMountData.Serial);
            // }
            // anim?.TryForceSetAnimationState<MountAnimationState>();
            // (EquippedLocalMount as LocalVehicleBase)?.MountSummon();
            // EquippedLocalMount.GetComponent<MountNetObject>().MountView.PlayOneShotSpawnEffect(() =>
            // {
            //     IsMounting = false;
            // });
        }
        
        protected void DeMount()
        {
            // isRidingMount = false;
            // LinkedCharacter.ResetInputs();
            //
            // EquippedLocalMount.GetComponent<CharacterController>().enabled = false;
            //
            // UpdateUIState(eInputCommandModeType.Character);
            // EventBus?.Post<ICharacterBaseEventListener>(l => l.OnMountReleased());
            //
            // var local = LinkedCharacter;
            // var motor = local.Motor;
            //
            // // 1) Seat의 월드 포즈를 먼저 캐시
            // Vector3 worldPos = EquippedLocalMount.transform.position;
            // Quaternion worldRot = Quaternion.Euler(EquippedLocalMount.transform.rotation.eulerAngles.x, EquippedLocalMount.transform.rotation.eulerAngles.y, EquippedLocalMount.transform.rotation.eulerAngles.z);
            //
            // // 2) 부모 해제: 스케일 변경되지 않도록 차단
            // local.transform.SetParent(_characterParent, worldPositionStays: false);
            //
            // // CharacterController의 속도도 리셋
            // if (EquippedLocalMount.GetComponent<CharacterController>() is var mountCC && mountCC != null) 
            //     mountCC.Move(Vector3.zero); // Mount의 잔류 속도 제거
            //
            // // 3) Transform이 아니라 KCC Motor로 스냅
            // LinkedCharacter.SetActiveCharacterPhysics(true);
            // motor.SetPositionAndRotation(worldPos, worldRot);
            //
            // // 4) 충돌/모터 복구
            // if (motor.Capsule)
            //     motor.Capsule.enabled = true;
            //
            // // 카메라/입력 마무리
            // LinkedCharacter.CameraTarget.transform.rotation = EquippedLocalMount.CameraTarget.transform.rotation;
            // UpdateCamera(LinkedCharacter.transform, LinkedCharacter.CameraTarget);
            //
            // // 5) Mount 비활성화는 캐릭터 복구 이후에
            // var mountAnimState = LinkedCharacter?.CharacterAnimation?.StateMachine?.CurrentState as MountAnimationState;
            // if (mountAnimState != null)
            // {
            //     // MountAnimationState가 하차 처리 완료를 알 수 있도록 설정
            //     mountAnimState.StartDeMountProcess();
            // }
            //
            // (EquippedLocalMount as LocalVehicleBase)?.DeMount();
            // EquippedLocalMount.GetComponent<Animator>().SetTrigger(deMountAnimationKey);
            // EquippedLocalMount.GetComponent<MountNetObject>().MountView.PlayOneShotDissolveEffect(() =>
            // {
            //     EquippedLocalMount.gameObject.SetActive(false);
            //     EquippedLocalMount.SetColliderParent();
            //     LinkedCharacter.LockMoveInput = false;
            //     IsDemounting = false; // Demount 완료 시 해제
            // });
        }

        void UpdateCamera(Transform worldUpTarget, Transform followTarget)
        {
            cameraPivot = followTarget;
            // mainCamera = cameraSystem.MainCamera;
            // cameraSystem.FollowTarget = cameraPivot;
            // cameraSystem.Brain.WorldUpOverride = worldUpTarget;
        }

        // PlayerController.cs
        // void UpdateUIState(eInputCommandModeType mode)
        // {
        //     PlayerControlHUD.Instance.EInputCommandModeType = mode;
        //
        //     // 애니/상태머신이 참조하는 소스 동기화
        //     if (LinkedCharacter is not null)
        //         LinkedCharacter.SetCommandModeType(mode);
        // }

        protected bool CanChangeMountState()
        {
            if (LinkedCharacter is null)
                return false;

            // 1) 현재 탑승 가능한 애니메이션 상태인지 판단 - 스태미나를 사용하지 않는 경우에만 탑승 가능.
            var sm = LinkedCharacter.CharacterAnimation?.StateMachine;
            var cur = sm?.CurrentState;
            bool canChangeState = cur != null &&
                                  (cur.StateType == CharacterAnimationEnums.eStateType.IDLE ||
                                   cur.StateType == CharacterAnimationEnums.eStateType.RUN ||
                                   cur.StateType == CharacterAnimationEnums.eStateType.WALK ||
                                   cur.StateType == CharacterAnimationEnums.eStateType.DASH ||
                                   cur.StateType == CharacterAnimationEnums.eStateType.SPRINT ||
                                   cur.StateType == CharacterAnimationEnums.eStateType.MOUNT ||
                                   cur.StateType == CharacterAnimationEnums.eStateType.GATHERING);
            if (!canChangeState)
                return false;

            if (LinkedCharacter.IsEventLockType(REIW.EventLock.eEventLockType.CharacterMount))
                return false;

            return true;
        }
        
        public void OnMountSpawnEndCallback(bool isSuccess)
        {
            //EquippedLocalMount.GetComponent<Animator>().SetBool(isEndPlayerSummonAnim, isSuccess);
        }
    }
}