using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    public BTTree treeAsset;

    private void Awake()
    {
        if (treeAsset == null)
        {
            Debug.LogWarning("[BT] treeAsset is null on " + name);
            return;
        }

        // 에셋 그대로 사용 + runner 주입
        foreach (var node in treeAsset.nodes)
        {
            if (node is IsPlayerInRangeNode cond)
            {
                cond.runner = this;
                Debug.Log("[BT] cond.runner = this; " + cond.name);
            }
            if (node is PatrolNode patrol)
                patrol.runner = this;

            if (node is ChasePlayerNode chase)
                chase.runner = this;

            if (node is AttackPlayerNode attack)
                attack.runner = this;
        }
    }

    private void Update()
    {
        if (treeAsset != null)
            treeAsset.Update();
    }
}