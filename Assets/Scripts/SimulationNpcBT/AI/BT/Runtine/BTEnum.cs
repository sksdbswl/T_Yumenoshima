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
        CanAttack,
        CanChase,
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
        
        // 도둑
        Steal,
        
        // 소방관
        FindFireTarget,
        MoveToFire,
        Extinguish,
        
        // 의사
        FindTiredTarget,
        MoveToTiredTarget,
        Heal,
        
        // 경찰
        FindThiefTarget,
        MoveToThief,
        Catch,
        
        // 기본
        GoHome,
        KeepDefault
    }
}