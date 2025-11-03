using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace REIW
{
    public class PlayerStandaloneController : PlayerController
    {
        [field: Header("Standalone Settings")]
        // [SerializeField] private EnumRace race;
        // [SerializeField] private EnumGender gender;

        protected override void OnDrawGizmos()
        {
            
        }

        protected override void Awake()
        {
            base.Awake();
            
            Initialize();
        }

        public override void Initialize()
        {
            LinkedCharacter = GetComponent<LocalCharacter>();
            // var cameraSystem = GetComponentInChildren<IngameCameraSystem>();
            // base.Construct(cameraSystem);
            //
            // var ownerPlayerNetObject = GetComponent<OwnerPlayerNetObject>();
            // ownerPlayerNetObject.ForceSetupGenderAndRace(this.gender, this.race);
            //
            // InputController.Singleton.Initialize();
            // InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.Player;
            // SoundManager.Singleton.Initialize();
            // GameDataModel.Singleton.Initialize();
            // UserDataModel.Singleton.PlayerInfoData.Race = race;
            // UserDataModel.Singleton.PlayerInfoData.Gender = gender;
            
            base.Initialize();
            
            //Destroy(ownerPlayerNetObject);
        }

        protected override void OnExecuteMount()
        {
            // if (!EquippedLocalMount)
            //     return;

            if (!CanChangeMountState())
            {
                LogUtil.Log("Can not Execute Mount.");
                return;
            }

            if (!isRidingMount)
            {
                isRidingMount = true;
                Mount();
            }
            else
            {
                isRidingMount = false;
                DeMount();
            }
        }
    }
}