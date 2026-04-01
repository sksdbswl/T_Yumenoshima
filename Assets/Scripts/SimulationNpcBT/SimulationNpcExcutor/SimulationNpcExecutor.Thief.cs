using System.Collections;
using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        /// <summary>
        /// 1. 아침, 점심 : 플레이어 돈 스틸 -> 경찰이 주기적으로 제압 ( 일정 쿨타임 만큼 스틸 불가 )
        /// 2. 저녁 : 플레이어 돈 스틸 -> 경찰이 귀가 후에는 제압 불가, 도망 다녀야함  
        /// </summary>
        #region Thief : Chase/Steal

        private float stealDelay = 5f;
        
        public bool IsRestricted = false;
        private Coroutine restrictedCoroutine;
        
        public ENodeState DoSteal(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;
            
            var player = target.GetComponent<PlayerStatus>();
            
            // 플레이어가 경찰이면 스틸 불가
            if(player._playerStatus.CurrentJobType == Const.JobType.Police) return ENodeState.ENS_Failure;            
            
            if (!isProgress)
            {
                // 1. 스틸 시작
                //Debug.Log("스틸 시작");
                if (agent != null) Stop(); 

                Vector3 lookDir = (target.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.forward = lookDir;

                
                player?.TakeSteal(100000);
                
                // animator.SetTrigger("Attack"); 
                
                delayTimer = stealDelay;
                isProgress = true;
            }
  
            delayTimer -= Time.deltaTime;
            
            if (delayTimer > 0f)
            {
                //Debug.Log($"스틸 중... ::{attackTimer}");
                // 2. 스틸 진행 중
                return ENodeState.ENS_Running;
            }

            // 3. 스틸 완료 시점
            //Debug.Log("스틸 끝");
            delayTimer = 0f;
            isProgress = false;
            agent.isStopped = false;
            
            return ENodeState.ENS_Success;
        }
        
        // 훔치기 가능 상태 체크
        public void SetRestricted(float duration)
        {
            if (restrictedCoroutine != null)
                StopCoroutine(restrictedCoroutine);

            restrictedCoroutine = StartCoroutine(RestrictedCoroutine(duration));
        }

        private IEnumerator RestrictedCoroutine(float duration)
        {
            IsRestricted = true;
            Debug.Log($"{name} 제압 상태 시작 ({duration}초)");

            yield return new WaitForSeconds(duration);

            IsRestricted = false;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }

            restrictedCoroutine = null;

            Debug.Log($"{name} 제압 상태 해제");
        }
        
        #endregion
    }
}