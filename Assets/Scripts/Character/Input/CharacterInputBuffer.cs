using REIW.Animations.Character;
using UnityEngine;

namespace REIW
{
    using eStateType = CharacterAnimationEnums.eStateType;

    public enum eCharacterActionInputType
    {
        NONE = 0,
        JUMP,
        PARKOUR,
        DASH,
        SPRINT,
        GRAPPLE,
        MOUNT,
        INTERACTION,
        GATHERING,
        FISHING,
        RUN,
        WALK
    }

    public static class CharacterActionInputTypeExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnInitialize()
        {
            EnumUtils.Warm<eCharacterActionInputType,eStateType>();
        }

        public static eStateType ConvertStateType(this eCharacterActionInputType type)
        {
            switch (type)
            {
                case eCharacterActionInputType.JUMP:
                    return eStateType.JUMP;
                case eCharacterActionInputType.PARKOUR:
                    return eStateType.PARKOUR;
                case eCharacterActionInputType.DASH:
                    return eStateType.DASH;
                case eCharacterActionInputType.SPRINT:
                    return eStateType.SPRINT;
                case eCharacterActionInputType.GRAPPLE:
                    return eStateType.GRAPPLE;
                case eCharacterActionInputType.MOUNT:
                    return eStateType.MOUNT;
                case eCharacterActionInputType.INTERACTION:
                    return eStateType.INTERACTION;
                case eCharacterActionInputType.GATHERING:
                    return eStateType.GATHERING;
                case eCharacterActionInputType.FISHING:
                    return eStateType.FISHING;
            }

            LogUtil.LogError($"[ConvertStateType] {type} is undefined");
            return type.ConvertType<eCharacterActionInputType, eStateType>();
        }

        public static void SetInputState(this PlayerCharacterInputs inputs, eCharacterActionInputType InType, bool state)
        {
            switch (InType)
            {
                case eCharacterActionInputType.JUMP:
                    inputs.Jump = state;
                    break;
                case eCharacterActionInputType.PARKOUR:
                    inputs.Parkour = state;
                    break;
                case eCharacterActionInputType.DASH:
                    inputs.Dash = state;
                    break;
                case eCharacterActionInputType.SPRINT:
                    inputs.Sprint = state;
                    break;
                case eCharacterActionInputType.GRAPPLE:
                    inputs.Grapple = state;
                    break;
                case eCharacterActionInputType.MOUNT:
                    inputs.Mount = state;
                    break;
                case eCharacterActionInputType.INTERACTION:
                    break;
                case eCharacterActionInputType.GATHERING:
                    break;
                case eCharacterActionInputType.FISHING:
                    break;
            }
        }
    }

    [System.Serializable]
    public class CharacterActionInputBuffer : InputBuffer.Abstractions.InputBufferValueBase<eCharacterActionInputType>
    {
    }
}
