using System;
using System.Collections.Generic;
using System.Linq;
using AI.BT.Runtime;
using TestBT;
using UnityEngine;

public static class BTGraphRuntimeBuilder
{
    private delegate INode NodeFactory(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner);

    private static readonly Dictionary<BTNodeType, NodeFactory> NodeFactories = new()
    {
        { BTNodeType.Root, BuildRootNode },
        { BTNodeType.Selector, BuildSelectorNode },
        { BTNodeType.Sequence, BuildSequenceNode },
        { BTNodeType.Condition, BuildConditionNode },
        { BTNodeType.Action, BuildActionNode },
    };

    private static readonly Dictionary<BTConditionType, Func<SimulationNpcController, bool>> ConditionFactories = new()
    {
        { BTConditionType.IsPlayerNear, owner => owner.sensor.Blackboard.isPlayerNear },
        { BTConditionType.CanMotion, owner => owner.sensor.Blackboard.canMotion },
        { BTConditionType.CanFlee, owner => owner.sensor.Blackboard.canFlee },
        { BTConditionType.IsFleeing, owner => owner.executor.IsFleeing() },
        { BTConditionType.IsPlayerVeryNear, owner => owner.sensor.Blackboard.isPlayerVeryNear },
        { BTConditionType.CanAttack, owner => owner.sensor.Blackboard.canAttack },
        { BTConditionType.CanChase, owner => owner.sensor.Blackboard.canChase },
        { BTConditionType.IsAttacking, owner => owner.executor.IsAttacking() },
    };

    private static readonly Dictionary<BTActionType, Func<SimulationNpcController, ENodeState>> ActionFactories = new()
    {
        { BTActionType.LookAt, owner => owner.executor.DoLookAt(owner.sensor.Blackboard.player) },
        { BTActionType.Flee, owner => owner.executor.DoFlee(owner.sensor.Blackboard.player) },
        { BTActionType.Attack, owner => owner.executor.DoAttack(owner.sensor.Blackboard.player) },
        { BTActionType.Chase, owner => owner.executor.DoChase(owner.sensor.Blackboard) },
        { BTActionType.KeepDefault, owner => owner.executor.KeepDefault() },
    };

    public static INode Build(BTGraphAsset asset, SimulationNpcController owner)
    {
        if (asset == null) return null;
        if (owner == null) return null;
        
        if (owner.sensor == null || owner.sensor.Blackboard == null) return null;
        if (owner.executor == null) return null;
        if (asset.nodes == null || asset.nodes.Count == 0) return null;

        var nodeMap = asset.nodes.ToDictionary(n => n.guid, n => n);
        return BuildNode(asset.rootGuid, nodeMap, owner);
    }

    private static INode BuildNode(string guid, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        if (!nodeMap.TryGetValue(guid, out var data)) return null;
        
        // 1. Dictionary에서 등록된 '함수(참조)'를 꺼내고
        // 여기서 factory는 BuildRootNode나 BuildSelectorNode 같은 함수 그 자체를 가리킵니다.
        if (!NodeFactories.TryGetValue(data.nodeType, out var factory)) return null;
    
        // 2. 여기서 비로소 함수를 '실행'합니다! 
        // 이때 필요한 파라미터(data, nodeMap, owner)를 비로소 전달합니다.
        return factory(data, nodeMap, owner);
    }

    private static INode BuildRootNode(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        if (data.childrenGuids == null || data.childrenGuids.Count == 0)
        {
            Debug.LogWarning($"[BTBuilder] root node has no child. guid = {data.guid}");
            return null;
        }

        return BuildNode(data.childrenGuids[0], nodeMap, owner);
    }

    private static INode BuildSelectorNode(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        var children = BuildChildren(data, nodeMap, owner);
        return new BTSelectorNode(data.guid, children);
    }

    private static INode BuildSequenceNode(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        var children = BuildChildren(data, nodeMap, owner);
        return new BTSequenceNode(data.guid, children);
    }

    private static INode BuildConditionNode(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        return new BTConditionNode(data.guid, () => EvaluateCondition(data.conditionType, owner));
    }

    private static INode BuildActionNode(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        return new BTActionNode(data.guid, () => ExecuteAction(data.actionType, owner));
    }

    private static List<INode> BuildChildren(BTNodeData data, Dictionary<string, BTNodeData> nodeMap, SimulationNpcController owner)
    {
        var childGuids = data.childrenGuids ?? new List<string>();

        return childGuids
            .Select(childGuid => BuildNode(childGuid, nodeMap, owner))
            .Where(node => node != null)
            .ToList();
    }

    private static bool EvaluateCondition(BTConditionType conditionType, SimulationNpcController owner)
    {
        if (!ConditionFactories.TryGetValue(conditionType, out var evaluator))
        {
            Debug.LogWarning($"[BTBuilder] unsupported condition type: {conditionType}");
            return false;
        }

        return evaluator(owner);
    }

    private static ENodeState ExecuteAction(BTActionType actionType, SimulationNpcController owner)
    {
        if (!ActionFactories.TryGetValue(actionType, out var executor))
        {
            Debug.LogWarning($"[BTBuilder] unsupported action type: {actionType}");
            return ENodeState.ENS_Failure;
        }

        return executor(owner);
    }
}