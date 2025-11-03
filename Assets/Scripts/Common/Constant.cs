using FLATBUFFERS;
using UnityEngine;

namespace REIW
{
    public class Constant
    {
        public enum RunEnvironment
        {
            PC,
            Mobile,
            QA,
            Release
        }

#if UNITY_EDITOR
        public const RunEnvironment RuntimeEnvironment = RunEnvironment.PC;
#elif UNITY_ANDROID || UNITY_IOS
        public const RunEnvironment RuntimeEnvironment = RunEnvironment.Mobile;
#endif

        public const string Define_Symbol_Dev = "REIW_DEV";
        public const string Define_Symbol_QA = "REIW_QA";
        
        
        public static readonly float SnapShotInterval = 0.04f; // 샘플링 주기
        public static float SnapShotFlushInterval { get; private set; } = 0.25f; // 최대 지연 (패킷 Send )
        public static void SetSnapShotFlushInterval(uint value) => SnapShotFlushInterval = value * 0.001f;
    }

    public class Layer
    {
        public static readonly int LAYER_Default = LayerMask.NameToLayer("Default"); //0
        public static readonly int LAYER_TransparentFX = LayerMask.NameToLayer("TransparentFX"); //1
        public static readonly int LAYER_IGNORE_RAYCAST = LayerMask.NameToLayer("Ignore Raycast"); //2
        public static readonly int LAYER_WATER = LayerMask.NameToLayer("Water"); //4
        public static readonly int LAYER_UI = LayerMask.NameToLayer("UI");  //5
        public static readonly int LAYER_PLAYER = LayerMask.NameToLayer("Player"); //6 Player (Local)
        public static readonly int LAYER_OTHER_PLAYER = LayerMask.NameToLayer("OtherPlayer"); //7 다른 유저
        public static readonly int LAYER_NPC = LayerMask.NameToLayer("NPC"); //8 NPC
        public static readonly int LAYER_MOUNT = LayerMask.NameToLayer("Mount"); //9 Mount
        
        public static readonly int LAYER_WALL = LayerMask.NameToLayer("WallClimb"); //11  벽타기 가능여부 체크 
        public static readonly int LAYER_GROUND = LayerMask.NameToLayer("Ground"); //12 그라운드 점프 가능
        public static readonly int LAYER_GRAPPLE_POINT = LayerMask.NameToLayer("GrapplePoint"); //13 그래플 포인트
        public static readonly int LAYER_SNAP_POINT = LayerMask.NameToLayer("SnapPoint"); //14 하우징 (스냅 포인트)
        public static readonly int LAYER_BG_WALL = LayerMask.NameToLayer("BGWall"); //15 배경에서 못가는 곳
    }
    
    public static class ReIWTags
    {
        public static readonly TagHandle Player;
        
        public static readonly TagHandle Ground;
        public static readonly TagHandle Water;
        public static readonly TagHandle Grass;
        public static readonly TagHandle Metal;

        // 도메인 로드 시 1회 호출
        static ReIWTags()
        {
            Player =TagHandle.GetExistingTag("Player");
            
            Ground =TagHandle.GetExistingTag("Ground");
            Water = TagHandle.GetExistingTag("Water");
            Grass = TagHandle.GetExistingTag("Grass");
            Metal = TagHandle.GetExistingTag("Metal");
        }
    }

    public enum eObjectType
    {
        None,
        Character,
        Npc,
        Pet,
        Prop,
    }
    
    public enum eStaminaActionType
    {
        // 1. FLATBUFFERS.ENUM_USER_STAMINA
        Normal,
        Dash,
        Sprint,
        Swim,
        SwimSprint,
        Glide,
        GlideJump,
        WallClimb,
        Grapple,
        
        Dashing,
        Grappling,
        
        // 2. FLATBUFFERS.ENUM_MOUNT_STAMINA
        Summon,
        Riding,
        Surfing,
        Flying,
        FlyWalk,
        FlyUp,
        FlyDown,
        RidingSprint,
        SurfingSprint,
        FlyingSprint,
        Trick,
    }

    public enum eInputCommandModeType
    {
        Character,
        Riding,
        Flying,
        Surfing,
        // Diving, // 추후 기획 예정.
        IgnoreMovement, // 이동 스냅샷을 보내지 않아야 할 때 사용
    }
    
    // 탈 것의 종류
    public enum eMountType
    {
        Riding,
        Flying,
        Surfing,
        // Diving, // 추후 기획 예정.
    }

    public enum eCraftJobType : ulong
    {
        Chef = 4294967299,
        Alchemist = 8589934595,
        Engineer = 12884901891,
        Mechanic = 17179869187,
        Tamer = 21474836483,
        FashionDesigner = 25769803779,
    }

    public enum eCraftStatus
    {
        Trial,
        Enable,
        Disable,
    }
    
    public enum eMountAnimationStringName
    {
        MountSummon,
        DeMount,
    }
    
    // CraftJob.1: 4294967299
    // CraftJob.2: 8589934595
    // CraftJob.3: 12884901891
    // CraftJob.4: 17179869187
    // CraftJob.5: 21474836483
    // CraftJob.6: 25769803779
}