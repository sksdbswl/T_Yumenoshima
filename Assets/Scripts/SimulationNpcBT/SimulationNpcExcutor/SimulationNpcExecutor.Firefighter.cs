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

        private PlaceableInteraction currentFireTarget;

        /// <summary>
        /// 가장 가까운 불난 건물 탐색
        /// </summary>
        public PlaceableInteraction GetNearestFireBuilding()
        {
            var buildings = PlacementManager.Singleton.BuildingInstances;

            PlaceableInteraction nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b == null) continue;
                if (!b.gameObject.activeInHierarchy) continue;
                if (!b.IsOnFire) continue;

                float sqr = (b.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = b;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 불난 건물이 있는지 확인하고 현재 타겟 갱신
        /// </summary>
        public ENodeState DoFindFireTarget()
        {
            if (currentFireTarget != null && currentFireTarget.IsOnFire)
                return ENodeState.ENS_Success;

            currentFireTarget = GetNearestFireBuilding();

            return currentFireTarget != null
                ? ENodeState.ENS_Success
                : ENodeState.ENS_Failure;
        }

        /// <summary>
        /// 불난 건물 쪽으로 이동
        /// </summary>
        public ENodeState DoMoveToFire()
        {
            if (agent == null) return ENodeState.ENS_Failure;

            if (currentFireTarget == null || !currentFireTarget.IsOnFire)
            {
                currentFireTarget = GetNearestFireBuilding();
                if (currentFireTarget == null) return ENodeState.ENS_Failure;
            }

            // if (!currentFireTarget.gameObject.activeInHierarchy)
            // {
            //     currentFireTarget = null;
            //     return ENodeState.ENS_Failure;
            // }

            float distance = Vector3.Distance(transform.position, currentFireTarget.transform.position);
            
            if (distance <= 2.0f)
            {
                agent.isStopped = true;
                return ENodeState.ENS_Success;
            }

            agent.isStopped = false;
            agent.SetDestination(currentFireTarget.transform.position);
            SetSpeed(runSpeed);
            return ENodeState.ENS_Running;
        }

        /// <summary>
        /// 불 끄기
        /// </summary>
        public ENodeState DoExtinguish()
        {
            if (currentFireTarget == null) return ENodeState.ENS_Failure;
            if (!currentFireTarget.IsOnFire)
            {
                currentFireTarget = null;
                return ENodeState.ENS_Failure;
            }

            float distance = Vector3.Distance(transform.position, currentFireTarget.transform.position);
            if (distance > 3f) // 일정거리가 안되면 진압불가
            {
                return ENodeState.ENS_Failure;
            }

            if (!isProgress)
            {
                Stop();

                Vector3 lookDir = (currentFireTarget.transform.position - transform.position).normalized;
                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                    transform.forward = lookDir;

                extinguishTimer = extinguishDelay;
                isProgress = true;

                Debug.Log($"[Firefighter] 진화 시작 : {currentFireTarget.name}");
            }

            extinguishTimer -= Time.deltaTime;

            if (extinguishTimer > 0f)
            {
                return ENodeState.ENS_Running;
            }

            currentFireTarget.SetFire(false);

            extinguishTimer = 0f;
            isProgress = false;

            if (agent != null)
                agent.isStopped = false;

            Debug.Log($"[Firefighter] 진화 완료 : {currentFireTarget.name}");

            currentFireTarget = null;
            SetSpeed(defaultSpeed);
            return ENodeState.ENS_Success;
        }

        #endregion
    }
}