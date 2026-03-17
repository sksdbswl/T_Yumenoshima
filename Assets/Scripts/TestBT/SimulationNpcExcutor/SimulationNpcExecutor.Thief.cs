using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        #region Thief : Chase/Attack

        private bool isAttacking = false;
        private float attackDelay = 5f;
        private float attackTimer = 0f;

        public ENodeState DoChase(NpcBlackboard target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            // 1. 공격 범위 안에 들어왔다면? 추적 성공 반환 -> 다음 공격 노드로
            if (target.isPlayerVeryNear)
            {
                agent.isStopped = true; 
                return ENodeState.ENS_Success; 
            }

            // 2. 아직 멀다면? 계속 이동하며 진행 중 반환
            Debug.Log("추적 중...");
            agent.isStopped = false;
            agent.SetDestination(target.player.position);
            return ENodeState.ENS_Running;
        }

        public ENodeState DoAttack(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            if (!isAttacking)
            {
                // 1. 공격 시작 시점 
                Debug.Log("공격 시작");
                if (agent != null) Stop(); 

                Vector3 lookDir = (target.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.forward = lookDir;

                // animator.SetTrigger("Attack");
                attackTimer = attackDelay;
                isAttacking = true;
            }
  
            attackTimer -= Time.deltaTime;
            
            if (attackTimer > 0f)
            {
                Debug.Log($"공격 중... ::{attackTimer}");
                // 2. 공격 진행 중
                
                return ENodeState.ENS_Running;
            }

            // 3. 공격 완료 시점
            Debug.Log("공격 끝");
            attackTimer = 0f;
            isAttacking = false;
            agent.isStopped = false;
            
            return ENodeState.ENS_Success;
        }

        public bool IsAttacking()
        {
            return isAttacking;
        }

        #endregion
    }
}