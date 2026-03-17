public enum BTNodeState
{
    Success, // 할 일 하나 성공적으로 끝남
    Failure, // 지금 이 순간 실행 가능한 행동이 하나도 없음
    Running // 지금 이 행동을 계속 실행 중
}