using UnityEngine;

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