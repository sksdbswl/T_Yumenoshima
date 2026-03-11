using System.Collections.Generic;
using System.Linq;
using AI.BT.Runtime;
using TestBT;

public class BTGraphRuntimeBuilder
{
    public static INode Build(BTGraphAsset asset, SimulationNpcController owner)
    {
        var nodeMap = asset.nodes.ToDictionary(n => n.guid, n => n);
        return BuildNode(asset.rootGuid, nodeMap, owner);
    }

    private static INode BuildNode(string guid, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        var data = nodeMap[guid];
        var bb = owner.sensor.Blackboard;
        var executor = owner.executor;

        switch (data.nodeType)
        {
            case BTNodeType.Root:
            {
                if (data.childrenGuids == null || data.childrenGuids.Count == 0)
                    return null;

                return BuildNode(data.childrenGuids[0], nodeMap, owner);
            }

            case BTNodeType.Selector:
            {
                var children = data.childrenGuids
                    .Select(childGuid => BuildNode(childGuid, nodeMap, owner))
                    .Where(x => x != null)
                    .ToList();

                return new BTSelectorNode(data.guid, children);
            }

            case BTNodeType.Sequence:
            {
                var children = data.childrenGuids
                    .Select(childGuid => BuildNode(childGuid, nodeMap, owner))
                    .Where(x => x != null)
                    .ToList();

                return new BTSequenceNode(data.guid, children);
            }
            
            case BTNodeType.Condition:
            {
                return new BTConditionNode(data.guid, () => EvaluateCondition(data.conditionType, owner));
            }

            case BTNodeType.Action:
            {
                return new BTActionNode(data.guid, () => ExecuteAction(data.actionType, data.animationStateName, owner));
            }
        }

        return null;
    }

    private static bool EvaluateCondition(BTConditionType conditionType, SimulationNpcController owner)
    {
        var bb = owner.sensor.Blackboard;
        var executor = owner.executor;

        switch (conditionType)
        {
            case BTConditionType.IsPlayerNear:
                return bb.isPlayerNear;

            case BTConditionType.CanMotion:
                return bb.canMotion;

            case BTConditionType.CanFlee:
                return bb.canFlee;

            case BTConditionType.IsPlayerVeryNear:
                return bb.isPlayerVeryNear;

            case BTConditionType.CanAttack:
                return bb.canAttack;

            case BTConditionType.CanChase:
                return bb.canChase;

            case BTConditionType.IsAttacking:
                return executor.IsAttacking();
        }

        return false;
    }

    private static ENodeState ExecuteAction(BTActionType actionType, string animationStateName, SimulationNpcController owner)
    {
        var bb = owner.sensor.Blackboard;
        var executor = owner.executor;

        switch (actionType)
        {
            case BTActionType.LookAt:
                return executor.DoLookAt(bb.player);

            case BTActionType.Flee:
                return executor.DoFlee(bb.player);

            case BTActionType.Attack:
                return executor.DoAttack(bb.player);

            case BTActionType.Chase:
                return executor.DoChase(bb);

            case BTActionType.KeepDefault:
                return executor.KeepDefault();
        }

        return ENodeState.ENS_Failure;
    }
}