using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        /// <summary>
        /// 1. 아침,점심 : 추격, 공격
        /// 2. 저녁 : 잠자기 or Home
        /// </summary>
        #region Monster : Chase/Attack

        private float attackDelay = 5f;
        private float attackTimer = 0f;

        public ENodeState DoAttack(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            if (!isProgress)
            {
                // 1. 공격 시작 시점 
                //Debug.Log("공격 시작");
                if (agent != null) Stop(); 

                Vector3 lookDir = (target.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.forward = lookDir;

                var player = target.GetComponent<PlayerStatus>();
                player?.TakeDamage(10);
                
                // animator.SetTrigger("Attack"); // 피격 애니메이션 처리
                
                attackTimer = attackDelay;
                isProgress = true;
            }
  
            attackTimer -= Time.deltaTime;
            
            if (attackTimer > 0f)
            {
                //Debug.Log($"공격 중... ::{attackTimer}");
                // 2. 공격 진행 중
                
                return ENodeState.ENS_Running;
            }

            // 3. 공격 완료 시점
            //Debug.Log("공격 끝");
            attackTimer = 0f;
            isProgress = false;
            agent.isStopped = false;
            
            return ENodeState.ENS_Success;
        }

        #endregion
    }
}