using System.Collections.Generic;
using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public class SimulationNpcController : MonoBehaviour
    {
        [SerializeField] private BTGraphAsset graphAsset;
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

            runner.Operate();
            sensor.Tick(player);
        }
        
        private INode BuildTree()
        {
            return BTGraphRuntimeBuilder.Build(graphAsset, this);
        }
    }
}