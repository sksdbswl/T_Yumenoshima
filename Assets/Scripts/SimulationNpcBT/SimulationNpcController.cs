using System.Collections.Generic;
using AI.BT.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestBT
{
    public class SimulationNpcController : MonoBehaviour
    {
        //[SerializeField] private BTGraphAsset graphAsset;
        [HideInInspector] public NpcSO npcSO;
        [HideInInspector] public SimulationNpcSensor sensor;
        [HideInInspector] public SimulationNpcExecutor executor;
        
        private BehaviorTreeRunner runner;
        
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

            sensor.Tick(player);
            runner.Operate();
        }
        
        public INode BuildTree()
        {
            return BTGraphRuntimeBuilder.Build(npcSO.jobBT, this);
        }
        
        public void ChangeCitizenTree()
        {
            executor.ResetState();
            sensor.Blackboard.init();
            
            var newTree = BTGraphRuntimeBuilder.Build(npcSO.citizenBT, this);
            runner.ChangeTree(newTree);
        }
    }
}