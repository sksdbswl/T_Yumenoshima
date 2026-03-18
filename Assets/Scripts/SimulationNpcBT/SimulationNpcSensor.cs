using System;
using UnityEngine;

namespace TestBT
{
    /// <summary>
    /// 매 프레임/주기마다(현재 상황을 파악) 블랙보드를 업데이트하는 용도
    /// 실제 실행부는 SimulationNpcExecutor에서 실행 
    /// </summary>
    public class SimulationNpcSensor : MonoBehaviour
    {
        [HideInInspector] public NpcSO npcSO;
        [SerializeField] private float nearDistance = 4f;
        [SerializeField] private float veryNearDistance = 2f;
        
        public NpcBlackboard Blackboard { get; private set; } = new NpcBlackboard();

        private void Awake()
        {
            Blackboard.canFlee = npcSO.canFlee;
            Blackboard.canChase = npcSO.canChase;
            Blackboard.canAttack = npcSO.canAttack;
            Blackboard.canSteal = npcSO.canSteal;
        }

        public void Tick(PlayerBT player)
        {
            Blackboard.player = player.transform;
            
            if (player == null)
            {
                Blackboard.distanceToPlayer = float.MaxValue;
                Blackboard.isPlayerNear = false;
                Blackboard.isPlayerVeryNear = false;
                return;
            }

            float dist = Vector3.Distance(transform.position, player.transform.position);
            
            Blackboard.distanceToPlayer = dist;
            Blackboard.isPlayerNear = dist <= nearDistance;
            Blackboard.isPlayerVeryNear = dist <= veryNearDistance;
        }
        
        private void OnDrawGizmos()
        {
            Vector3 pos = transform.position;

            Gizmos.color = Color.cadetBlue;
            Gizmos.DrawWireSphere(pos, nearDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pos, veryNearDistance);

            if (Blackboard != null && Blackboard.player != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pos, Blackboard.player.position);
            }
        }
    }
}