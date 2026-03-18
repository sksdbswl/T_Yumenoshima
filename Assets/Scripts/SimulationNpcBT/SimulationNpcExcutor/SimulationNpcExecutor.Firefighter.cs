using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        /// <summary>
        /// 1. 아침 : 일반 시민처럼 기본 활동
        /// 2. 점심 : 순찰 -> 불 발견 시 진화
        /// 3. 저녁 : 귀가
        /// </summary>
        #region Firefighter : Patrol/Extinguish

        private float extinguishDelay = 3f;
        private float extinguishTimer = 0f;

        private Vector3 patrolPoint;
        private bool hasPatrolPoint = false;
        
        /// <summary>
        /// 불난 건물 쪽으로 이동
        /// </summary>
        public ENodeState DoMoveToFire(PlaceableInteraction fireTarget)
        {
            if (fireTarget == null) return ENodeState.ENS_Failure;
            if (agent == null) return ENodeState.ENS_Failure;

            if (!fireTarget.gameObject.activeInHierarchy)
                return ENodeState.ENS_Failure;

            float distance = Vector3.Distance(transform.position, fireTarget.transform.position);

            if (distance <= 2.0f)
            {
                agent.isStopped = true;
                return ENodeState.ENS_Success;
            }

            agent.isStopped = false;
            agent.SetDestination(fireTarget.transform.position);
            return ENodeState.ENS_Running;
        }

        /// <summary>
        /// 불 끄기
        /// </summary>
        public ENodeState DoExtinguish(PlaceableInteraction fireTarget)
        {
            if (fireTarget == null) return ENodeState.ENS_Failure;

            // 네 프로젝트 쪽에서 실제 불 상태를 이렇게 들고 있다고 가정
            // if (!fireTarget.IsOnFire)
            //     return ENodeState.ENS_Failure;

            if (!isProgress)
            {
                if (agent != null) Stop();

                Vector3 lookDir = (fireTarget.transform.position - transform.position).normalized;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                    transform.forward = lookDir;

                extinguishTimer = extinguishDelay;
                isProgress = true;

                Debug.Log($"[Firefighter] 진화 시작 : {fireTarget.name}");
            }

            extinguishTimer -= Time.deltaTime;

            if (extinguishTimer > 0f)
            {
                return ENodeState.ENS_Running;
            }

            //fireTarget.SetFire(false);

            extinguishTimer = 0f;
            isProgress = false;

            if (agent != null)
                agent.isStopped = false;

            Debug.Log($"[Firefighter] 진화 완료 : {fireTarget.name}");

            return ENodeState.ENS_Success;
        }

        // private Vector3 GetRandomPatrolPoint(Vector3 center, float radius)
        // {
        //     Vector2 random2D = Random.insideUnitCircle * radius;
        //     Vector3 point = new Vector3(center.x + random2D.x, center.y, center.z + random2D.y);
        //     return point;
        // }

        #endregion
    }
}