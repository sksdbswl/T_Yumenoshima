using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    public enum eSfxAddressableFolder
    {
        None = 0,
            
        Character_Cynox_Action = 100,
        Character_Cynox_Combat,
        Character_Cynox_PlayerVoice,

            
        Common_Footsteps =200,
        Common_Cloth,
        Common_Gear,
        Common_Impact,
        Common_CommonPlayer,

        Common_Mount = 501,

        UI = 300,
         
        NonCharacter_nonPlayerVoice = 400,
            
        Mount_Foley = 500,

        
    }

    public partial class PooledSoundSource
    {
        static readonly Dictionary<eSfxAddressableFolder, string> FolderPath =
            new Dictionary<eSfxAddressableFolder, string>()
            {
                { eSfxAddressableFolder.None, string.Empty },
                { eSfxAddressableFolder.Character_Cynox_Action, "SFX/Character/Cynox/Action/" },
                { eSfxAddressableFolder.Character_Cynox_Combat, "SFX/Character/Cynox/Combat/" },
                { eSfxAddressableFolder.Character_Cynox_PlayerVoice, "SFX/Character/Cynox/PlayerVoice/" },
                { eSfxAddressableFolder.Common_Footsteps, "SFX/Common/Footsteps/" },
                { eSfxAddressableFolder.Common_Cloth, "SFX/Common/Cloth/" },
                { eSfxAddressableFolder.Common_Gear, "SFX/Common/Gear/" },
                { eSfxAddressableFolder.Common_Impact, "SFX/Common/Impact/" },
                { eSfxAddressableFolder.Common_CommonPlayer, "SFX/Common/CommonPlayer/" },

                { eSfxAddressableFolder.UI, "SFX/UI/" },
                { eSfxAddressableFolder.NonCharacter_nonPlayerVoice, "SFX/NonCharacter/nonPlayerVoice/" },
                { eSfxAddressableFolder.Mount_Foley, "SFX/Mount/Foley/" },
                { eSfxAddressableFolder.Common_Mount, "SFX/Common/Mount/" },
            };
    }
    public enum eKnownSfxSound 
    {
        None = 0,
        // foot sound
        SE_Footstep_Run_Normal =	1,  
        SE_Footstep_Run_Water  =	2,
        SE_Footstep_Run_Grass  =	3,
        SE_Footstep_Run_Metal  =	4,


        //Action Sound
        SE_Cynox_F_JumpStart       = 2000,
        SE_Cynox_F_Dash            = 2002,
        SE_Cynox_F_Glide = 2004,
        SE_Cynox_F_FallLand        = 2007,






        SE_Cynox_F_GrappleThrow =2019, 
        SE_Cynox_F_GrappleMoveReg = 2020,
        SE_Cynox_F_GrappleMoveSpin = 2021,
        SE_Cynox_F_GrappleMoveShortMid = 2023,
        SE_Cynox_F_GrappleMoveMediumMid = 2026,
        SE_Cynox_F_GrappleArriveShort = 2028,
        SE_Cynox_F_GrappleArrive = 2030,
        SE_Cynox_F_GrappleArriveWobble = 2032,
        SE_Cynox_F_GrappleArriveSpinLand = 2034,
        SE_Cynox_F_GrappleArriveLand = 2035,
        SE_Cynox_F_GrappleLandJump = 2036,
        SE_Cynox_F_GrappleSpinLandJump = 2037,
        SE_Cynox_F_GrappleFall = 2038,



         // Common Player Sound
        SE_Common_Fishing_Ready = 70000,
        SE_Common_Fishing_Casting = 70001,
        SE_Common_Fishing_Casting_Water = 70002,
        SE_Common_Fishing_Fighting  = 70003,
        SE_Common_Fishing_Reel = 70004,
        SE_Common_Fishing_Lifting = 70005,
        SE_Common_Fishing_Miss = 70006,
        // SE_Common_Gathering = 7008,
        // SE_Common_Felling = 70009,
        // SE_Common_Mining = 70010,
        // SE_Common_Shovel = 70011,
        // SE_Common_IceCarving1 = 70012,
        SE_Common_IceCarving2 = 70013,
        SE_Common_Hunting = 70014,
        // SE_Common_Shearing = 70015,
        // SE_Common_Petting1 = 70016,
        // SE_Common_Petting2 = 70017,
        SE_Common_Success = 70018,
        SE_GlideStart_Common = 70019,
        SE_Glide_Common = 2004,
        SE_GlideJump_Common = 70021,
        SE_GlideEnd_Common = 2006,
        SE_GlideStart_Glider_05 = 70022,
        SE_Glide_Glider_05 = 70023,
        SE_GlideJump_Glider_05 = 70024,
        SE_GlideEnd_Glider_05 = 2006,

        // Mount_1 Sound
        SE_M_MountSummon_Motor01 = 400000,
        SE_M_MountRideIdle_Motor01 = 400001,
        SE_M_MountRide_Motor01 = 400002,
        SE_M_MountRideBreak_Motor01 = 400003,
        SE_M_Demount_Motor01 = 400004,
        SE_M_MountTurn_Motor01 = 400005,
        SE_M_MountSprintStart_Motor01 = 400006,
        SE_M_MountSprint_Motor01 = 400007,
        SE_M_MountSprintTurn_Motor01 = 400008,
        SE_M_MountSprintBreak_Motor01 = 400009,





        //Impact Sound
        SE_Common_WoodChop = 1300,
        SE_Common_TreeFallDown = 1301,
        SE_Common_ShovelGround = 1302,
        SE_Common_RockBreak = 1303,
        SE_Common_SickleHarvest = 1304,
        SE_Common_IcePickHammer = 1305,
        SE_Common_IceSlice = 1306,
        SE_Common_SpearImpactFlesh = 1307,
        SE_Common_WoolCut = 1308,
        SE_Common_PetAnimal = 1309,
        SE_Common_ShoulderBump_BacK = 1310,
        SE_Common_ShoulderBump_FLR = 1311,
        SE_Common_ShovelGround_throw = 1312,

    }

    public enum eUIButtonSound
    {
        None = 0,
        
        // UI sound
        Normal =	218,
        Select =	215,
        BackButton =	223,
        TabChange_Main =	213,
        TabChange_Sub = 214,
        Slidebar = 224,
    }

    public enum eUISoundType
    {
        None = 0,
        
        // UI Popup/Panel Sound
        SE_FullPopup_Open_01 = 200,           // 메인 화면에서 다른 UI 화면 진입 시 재생 (공용)            
        SE_FullPopup_Open_02 = 201,           // 우편 메뉴 진입 시 재생            
        SE_FullPopup_Open_03 = 202,           // 상점 메뉴 진입 시 재생            
        SE_FullPopup_Open_04 = 203,           // 캐릭터창 메뉴 진입 시 재생            
        SE_FullPopup_Open_05 = 204,           // 탈 것 메뉴 진입 시 재생            
        SE_FullPopup_Close_01 = 205,          // UI 전체 화면에서 이탈 시 재생 (공용)             
        SE_FullPopup_Close_02 = 206,          // 우편 메뉴 이탈 시 재생             
        SE_FullPopup_Close_03 = 207,          // 상점 메뉴 이탈 시 재생             
        SE_FullPopup_Close_04 = 208,          // 캐릭터창 메뉴 이탈 시 재생             
        SE_FullPopup_Close_05 = 209,          // 탈 것 메뉴 이탈 시 재생             
        SE_SmallPopup_Open_01 = 210,          // 버튼 터치해서 작은 팝업 출력 시 재생             
        SE_SmallPopup_Close_01 = 211,         // 버튼 터치해서 작은 팝업 출력 해제 시 재생              
        SE_GetReward_01 = 212,                // 화면 중앙에 보상 획득 연출 재생되면서 들리는 sfx (UI 연출과 통일 필요)       
        SE_TabChange_Main_01 = 213,         // 대분류 탭 버튼 터치 시 출력되는 SFX              
        SE_TabChange_Sub_01 = 214,          // 소분류 탭 버튼 터치 시 출력되는 SFX             
        SE_Select_01 = 215,                   // 아이템 선택할 때 재생되는 SFX    
        SE_DetailPanel_Open_01 = 216,         // 정보창 출력할 때 재생되는 sfx              
        SE_DetailPanel_Close_01 = 217,        // 정보창 해제할 때 재생되는 sfx               
        //SE_ButtonTouch_Normal_01 = 218,     // 공용 버튼 터치 시 재생되는 SFX                  
        SE_LevelUp_01 = 219,                  // 플레이어 레벨업할 때 출력되는 SFX     
        SE_ExpUp_01 = 220,                    // 플레이어 경험치 얻을 때 출력되는 SFX   
        SE_Upgrade_01 = 221,                  // 강화 성공할 때 출력되는 SFX     
        SE_CollectPopup_01 = 222,             // 채집하거나 아이템 주울 때 좌측에 결과 팝업 출력 시의 사운드          
        SE_BackButton_01 = 223,               // 뒤로 가기 버튼 터치 사운드          
        SE_Slidebar_01 = 224,              // 좌우로 바 슬라이드 사운드
        SE_Acquisition_01 = 225,              //상점에서 구매 완료 시 나오는 사운드
        SE_Fishing_Minigame_Success_01 = 226, //낚시 릴감기 노란색 버튼 클릭 성공할 때마다 나오는 사운드
        SE_Fishing_Minigame_Fail_01 = 227,    //낚시 릴감기 노란색 버튼 클릭 실패할 때마다 나오는 사운드
        SE_Achivement_Message_01 = 228,       // 업적달성 메세지 
        

    }
}
