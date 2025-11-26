using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Condition/Is Player In Range")]
public class IsPlayerInRangeNode : ConditionNode
{
    public float range = 5f;
    [System.NonSerialized] public BehaviourTreeRunner runner;

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
        Debug.Log($"[{name}] dist={dist:F2}, range={range}, inside={inside}");
        return inside;
    }
}