namespace AI.BT.Runtime
{
    public enum ENodeState
    {
        ENS_Running,
        ENS_Success,
        ENS_Failure,
    }
    
    public enum BTNodeType
    {
        Root,
        Selector,
        Sequence,
        Condition,
        Action
    }

    public enum BTActionType
    {
        None,
        LookAt,
        Flee,
        Attack,
        Chase,
        KeepDefault
    }

    public enum BTConditionType
    {
        None,
        IsPlayerNear,
        IsPlayerVeryNear,
        CanMotion,
        CanFlee,
        CanAttack,
        CanChase,
        IsAttacking
    }
}