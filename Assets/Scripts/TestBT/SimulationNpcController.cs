using System.Collections.Generic;
using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public class SimulationNpcController : MonoBehaviour
    {
        [SerializeField] private BTGraphAsset graphAsset;
        
        private BehaviorTreeRunner runner;
        
        [HideInInspector] public SimulationNpcSensor sensor;
        [HideInInspector] public SimulationNpcExecutor executor;
        
        private void Awake()
        {
            if (sensor == null)
                sensor = GetComponent<SimulationNpcSensor>();

            if (executor == null)
                executor = GetComponent<SimulationNpcExecutor>();

            runner = new BehaviorTreeRunner(BuildTree());
        }

        private void Update()
        {
            var player = FindObjectOfType<PlayerBT>();

            runner.Operate();
            sensor.Tick(player);
        }

        
        private INode BuildTree()
        {
            return BTGraphRuntimeBuilder.Build(graphAsset, this);
            
            // var bb = sensor.Blackboard;
            //
            // return new BTSelectorNode(
            //     new List<INode>()
            //     {
            //         // 1. 바라보기 시퀀스
            //         new BTSequenceNode(new List<INode>()
            //         {
            //             new BTConditionNode(() => bb.isPlayerNear),
            //             new BTConditionNode(() => bb.canMotion),
            //             new BTActionNode(() => executor.DoLookAt(bb.player)),
            //         }),
            //
            //         // 2. 도망 시퀀스
            //         new BTSequenceNode(new List<INode>()
            //         {
            //             new BTConditionNode(() => bb.isPlayerNear),
            //             new BTConditionNode(() => bb.canFlee),
            //             new BTActionNode(() => executor.DoFlee(bb.player)),
            //         }),
            //         
            //         // 1. 공격 중이거나 공격 범위면 공격
            //         new BTSequenceNode(new List<INode>()
            //         {
            //             new BTConditionNode(() => executor.IsAttacking() || bb.isPlayerVeryNear),
            //             new BTConditionNode(() => bb.canAttack),
            //             new BTActionNode(() => executor.DoAttack(bb.player)),
            //         }),
            //
            //         // 2. 공격 중이 아니고, 근처면 추적
            //         new BTSequenceNode(new List<INode>()
            //         {
            //             new BTConditionNode(() => !executor.IsAttacking()),
            //             new BTConditionNode(() => bb.isPlayerNear),
            //             new BTConditionNode(() => bb.canChase),
            //             new BTActionNode(() => executor.DoChase(bb)),
            //         }),
            //
            //         
            //         new BTActionNode(() => executor.KeepDefault()),
            //     }
            // );
        }
    }
}