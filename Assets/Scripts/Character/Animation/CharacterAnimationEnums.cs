namespace REIW.Animations.Character
{
    public static class CharacterAnimationEnums
    {
        /// <summary>
        /// eAnimationType 타입 추가 : eStateType의 state 이름 기준으로 각 state별 애니메이션 타입 이름 적용
        /// eAnimationType 타입 값 : eStateType의 state 값 * 1000으로 애니메이션 타입의 값 적용
        /// </summary>
        public enum eAnimationType : uint
        {
            NONE = 0,

            /// <summary>
            /// IDLE
            /// </summary>
            IDLE_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.IDLE,
            IDLE,
            IDLE_TYPE_END,

            /// <summary>
            /// WALK
            /// </summary>
            WALK_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.WALK,
            WALK,
            WALK_TURN_LEFT,
            WALK_TURN_RIGHT,
            WALK_STAND_STOP,
            WALK_MOVE_STOP,
            WALK_TYPE_END,

            /// <summary>
            /// RUN
            /// </summary>
            RUN_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.RUN,
            RUN,
            RUN_START,
            RUN_TURN_LEFT,
            RUN_TURN_RIGHT,
            RUN_QUICK_TURN_LEFT,
            RUN_QUICK_TURN_RIGHT,
            RUN_STAND_STOP,
            RUN_MOVE_STOP,
            RUN_TYPE_END,

            /// <summary>
            /// SPRINT
            /// </summary>
            SPRINT_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.SPRINT,
            SPRINT,
            SPRINT_QUICK_TURN_LEFT,
            SPRINT_QUICK_TURN_RIGHT,
            SPRINT_STAND_STOP,
            SPRINT_MOVE_STOP,
            SPRINT_TYPE_END,

            /// <summary>
            /// DASH
            /// </summary>
            DASH_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.DASH,
            DASH,
            DASH_STOP,
            DASH_TYPE_END,

            /// <summary>
            /// AIRBORNE
            /// </summary>
            AIRBORNE_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.AIRBORNE,
            AIRBORNE_FALL,
            AIRBORNE_LANDIND,
            AIRBORNE_TYPE_END,

            /// <summary>
            /// JUMP
            /// </summary>
            JUMP_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.JUMP,
            JUMP_STANDING,
            JUMP_LEFT_FOOT,
            JUMP_RIGHT_FOOT,
            JUMP_STANDING_LANDIND,
            JUMP_WALK_LEFT_FOOT_LANDING,
            JUMP_WALK_RIGHT_FOOT_LANDING,
            JUMP_RUN_LEFT_FOOT_LANDING,
            JUMP_RUN_RIGHT_FOOT_LANDING,
            JUMP_SPRINT_LEFT_FOOT_LANDING,
            JUMP_SPRINT_RIGHT_FOOT_LANDING,
            JUMP_TYPE_END,

            /// <summary>
            /// PARKOUR
            /// </summary>
            PARKOUR_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.PARKOUR,
            PARKOUR_VAULT_OVER_LEFT,
            PARKOUR_VAULT_OVER_RIGHT,
            PARKOUR_VAULT_ON,
            // PARKOUR_JUMP
            PARKOUR_JUMP_TYPE_START,
            PARKOUR_JUMP_LEFT_FOOT,
            PARKOUR_JUMP_RIGHT_FOOT,
            PARKOUR_JUMP_LONG,
            // PARKOUR_CLIMB
            PARKOUR_CLIMB_TYPE_START,
            PARKOUR_CLIMB_JUMP,
            PARKOUR_CLIMB_UP_LEDGE,
            PARKOUR_CLIMB_OFF_LEDGE,
            // PARKOUR_WALL_RUN
            PARKOUR_WALL_RUN_TYPE_START,
            PARKOUR_WALL_RUN_LEFT,
            PARKOUR_WALL_RUN_RIGHT,
            PARKOUR_TYPE_END,

            /// <summary>
            /// GRAPPLE
            /// </summary>
            GRAPPLE_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.GRAPPLE,
            GRAPPLE_THROW_UP,
            GRAPPLE_THROW,
            GRAPPLE_THROW_DOWN,
            GRAPPLE_THROW_AIR_UP,
            GRAPPLE_THROW_AIR,
            GRAPPLE_THROW_AIR_DOWN,
            GRAPPLE_MOVE_SHORT,
            GRAPPLE_MOVE_MEDIUM,
            GRAPPLE_MOVE_REG,
            GRAPPLE_MOVE_SPIN,
            GRAPPLE_ARRIVE_SHORT,
            GRAPPLE_ARRIVE,
            GRAPPLE_ARRIVE_WOBBLE,
            GRAPPLE_ARRIVE_SPIN_LANDING,
            GRAPPLE_LAUNCH,
            GRAPPLE_LAUNCH_SPIN,
            GRAPPLE_FALL,
            GRAPPLE_LANDING,
            GRAPPLE_TYPE_END,

            /// <summary>
            /// MOUNT
            /// </summary>
            MOUNT_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.MOUNT,
            MOUNT_SUMMON,           // 3000번 MotorBike 소환
            MOUNT_IDLE,             // 3000번 MotorBike 대기
            MOUNT_RIDE,             // 3000번 MotorBike 이동
            MOUNT_SPRINT,           // 3000번 MotorBike 가속
            MOUNT_BREAK,            // 3000번 MotorBike 정지
            MOUNT_DEMOUNT,          // 3000번 MotorBike 하차
            MOUNT_IDLE_SCOOTER,     // 1001번 Scooter 단일 애니메이션
            MOUNT_TYPE_END,

            /// <summary>
            /// INTERACTION
            /// </summary>
            INTERACTION_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.INTERACTION,
            INTERACTION_TYPE_END,

            /// <summary>
            /// GATHERING
            /// </summary>
            GATHERING_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.GATHERING,
            GATHERING_FELLING,
            GATHERING_GATHERING,
            GATHERING_HUNTING,
            GATHERING_MINING,
            GATHERING_SHEEP_SHEARING,
            GATHERING_TO_SHOVEL,
            GATHERING_ICECARVING_1,
            GATHERING_ICECARVING_2,
            GATHERING_PETTING_1,
            GATHERING_PETTING_2,
            GATHERING_SUCCESS,
            GATHERING_TYPE_END,

            /// <summary>
            /// MOVE_FAIL
            /// </summary>
            MOVE_FAIL_START_TYPE = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.MOVE_FAIL,
            MOVE_FAIL_WALL,
            MOVE_FAIL_END_TYPE,

            /// <summary>
            /// FISHING
            /// </summary>
            FISHING_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.FISHING,
            FISHING_CASTING,
            FISHING_FIGHTING_LEFT,
            FISHING_FIGHTING,
            FISHING_FIGHTING_RIGHT,
            FISHING_LIFTING,
            FISHING_READY,
            FISHING_READY_LOOP,
            FISHING_REEL,
            FISHING_MISS,
            FISHING_TYPE_END,

            /// <summary>
            /// GLIDING
            /// </summary>
            GLIDING_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.GLIDING,
            GLIDING_START,
            GLIDING_PLAYING,
            GLIDING_JUMP,
            GLIDING_TYPE_END,
            
            /// <summary>
            /// CHARACTER_CUSTOMIZING
            /// </summary>
            CHARACTER_CUSTOMIZING_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.CHARACTER_CUSTOMIZING,
            CHARACTER_CUSTOMIZING_IDLE,
            CHARACTER_CUSTOMIZING_TYPE_END,
            
            /// <summary>
            /// CHARACTER_STAGE
            /// </summary>
            CHARACTER_STAGE_TYPE_START = AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT * eStateType.CHARACTER_STAGE,
            CHARACTER_STAGE_IDLE,
            CHARACTER_STAGE_AVATAR,
            CHARACTER_STAGE_IDLE_TO_AVATAR,
            CHARACTER_STAGE_AVATAR_TO_IDLE,
            CHARACTER_STAGE_TYPE_END,
            
            TYPE_END
        }

        public enum eStateType
        {
            NONE = 0,
            IDLE,
            WALK,
            RUN,
            SPRINT,
            DASH,
            AIRBORNE,
            JUMP,
            PARKOUR,
            GRAPPLE,
            MOUNT,
            INTERACTION,
            GATHERING,
            MOVE_FAIL,
            FISHING,
            GLIDING,
            PLAY_TARGET_ANIMATION,
            CHARACTER_CUSTOMIZING,
            CHARACTER_STAGE,
            NETWORK,

            STATE_TYPE_END
        }

        public enum eMoveType
        {
            STAND = 0,
            WALK,
            RUN,
            SPRINT,
            DASH,
            AIRBORNE,
        }

        public enum eTurnDirection
        {
            NONE,
            LEFT,
            RIGHT
        }

        public static readonly int ANIMATION_TYPE_BIT_DIGITS = Utilities.BitsForValue((int)eAnimationType.TYPE_END);

        public static eAnimationType SetDontChangeAnimationNetObject(this eAnimationType InAnimationType)
        {
            return EnumUtils.PackFlag(InAnimationType, ANIMATION_TYPE_BIT_DIGITS);
        }
    }
}
