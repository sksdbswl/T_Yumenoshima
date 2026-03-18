using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        /// <summary>
        /// 1. 아침, 점심 : 일반 시민으로 활동
        /// 3. 저녁 : 마주치면 돈 뺏김
        /// </summary>

        #region Thief : Chase/Steal

        private float stealDelay = 5f;
        private float stealTimer = 0f;
        
        public ENodeState DoSteal(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            Debug.Log("DoSteal");
            if (!isProgress)
            {
                // 1. 스틸 시작
                Debug.Log("스틸 시작");
                if (agent != null) Stop(); 

                Vector3 lookDir = (target.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.forward = lookDir;

                var player = target.GetComponent<PlayerStatus>();
                player?.TakeSteal(100000);
                
                // animator.SetTrigger("Attack"); 
                
                stealTimer = stealDelay;
                isProgress = true;
            }
  
            stealTimer -= Time.deltaTime;
            
            if (stealTimer > 0f)
            {
                Debug.Log($"스틸 중... ::{attackTimer}");
                // 2. 스틸 진행 중
                return ENodeState.ENS_Running;
            }

            // 3. 스틸 완료 시점
            Debug.Log("스틸 끝");
            stealTimer = 0f;
            isProgress = false;
            agent.isStopped = false;
            
            return ENodeState.ENS_Success;
        }
        
        #endregion
    }
}