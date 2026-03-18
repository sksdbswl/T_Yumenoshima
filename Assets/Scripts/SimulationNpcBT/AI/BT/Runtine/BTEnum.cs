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

    public enum BTConditionType
    {
        None,
        IsPlayerNear,
        IsPlayerVeryNear,
        IsProgress,
        CanFlee,
        IsFleeing,
        CanAttack,
        CanChase,
        IsAttacking,
        CanSteal,
        CanHome,
    }
    
    public enum BTActionType
    {
        None,
        LookAt,
        Flee,
        Attack,
        Chase,
        Steal,
        GoHome,
        KeepDefault
    }
}