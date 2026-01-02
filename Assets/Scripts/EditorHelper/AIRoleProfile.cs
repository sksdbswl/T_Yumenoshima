using UnityEngine;

[CreateAssetMenu(menuName="AI/Role Profile")]
public class AIRoleProfile : ScriptableObject
{
    [Header("Movement")]
    public float patrolRadius = 5f;
    public float moveStopDistance = 1.5f;

    [Header("Perception")]
    public float detectionRange = 6f;   // 뭔가를 인지하는 거리 ( ex:추적 )

    [Header("Interaction")]
    public float interactionRange = 1.5f; // 타겟과 상호작용 가능한 거리 ( ex:공격 )
    public float interactionTime = 1.5f;// 상호작용 지속 시간
    
    public float interactionCooldown = 0.7f; // 숨고르기/딜레이 ex) 공격 후 딜레이 (직업마다 의미만 다름) 
}