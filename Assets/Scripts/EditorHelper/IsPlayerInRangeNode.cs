using UnityEngine;

/// <summary>
/// 행동 여부 확인 노드 : 지금 이 행동을 해도 되나요? 라는 질문에 Yes/No만 반환하고 Sequence Node에서 다음 행동처리
/// </summary>
[CreateAssetMenu(menuName = "AI/Nodes/Condition/Is Player In Range")]
public class IsPlayerInRangeNode : ConditionNode
{
    public float range = 5f;
    [System.NonSerialized] public BehaviourTreeRunner runner; // Enemy

    // inside == true : BTNodeState.Success
    // inside == false : BTNodeState.Failure
    protected override bool CheckCondition()
    {
        if (runner == null)
        {
            Debug.Log($"[{name}] runner is null");
            return false;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log($"[{name}] player not found (Tag 'Player'?)");
            return false;
        }

        float dist = Vector3.Distance(runner.transform.position, player.transform.position);
        bool inside = dist <= range;
        
        return inside;
    }
}