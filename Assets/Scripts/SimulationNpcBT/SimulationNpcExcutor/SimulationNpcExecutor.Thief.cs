using System.Collections;
using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        /// <summary>
        /// 1. 아침, 점심 : 일반 시민으로 활동
        /// 2. 저녁 : 마주치면 돈 뺏김
        ///
        /// 현재는 그냥 마주치면 스틸
        /// </summary>
        #region Thief : Chase/Steal

        private float stealDelay = 5f;
        private float stealTimer = 0f;
        
        public bool IsRestricted = false;
        private Coroutine restrictedCoroutine;
        
        public ENodeState DoSteal(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            if (!isProgress)
            {
                // 1. 스틸 시작
                //Debug.Log("스틸 시작");
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
                //Debug.Log($"스틸 중... ::{attackTimer}");
                // 2. 스틸 진행 중
                return ENodeState.ENS_Running;
            }

            // 3. 스틸 완료 시점
            //Debug.Log("스틸 끝");
            stealTimer = 0f;
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