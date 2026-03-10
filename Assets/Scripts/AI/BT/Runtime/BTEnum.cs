namespace AI.BT.Runtime
{
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