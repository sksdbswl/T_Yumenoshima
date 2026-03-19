using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        #region Police : Patrol/Catch

        private float catchDelay = 3f;
        private float catchTimer = 0f;

        private Npc currentThiefTarget;

        public Npc GetNearestThief()
        {
            var npcs = GameManager.Singleton.SpawnedNpcStatuses;

            Npc nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < npcs.Count; i++)
            {
                
                var npc = npcs[i];
                if (npc == null) continue;
                if (npc.transform == transform) continue;
                if(npc.npcSO.Job != Const.JobType.Thief) continue;
                if (npc.executor.IsRestricted) continue; // 이미 제압 중인 도둑이면 스킵
                
                float sqr = (npc.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = npc;
                }
            }

            return nearest;
        }

        public ENodeState DoFindThief()
        {
            if (currentThiefTarget != null)
                return ENodeState.ENS_Success;

            currentThiefTarget = GetNearestThief();

            return currentThiefTarget != null
                ? ENodeState.ENS_Success
                : ENodeState.ENS_Failure;
        }

        public ENodeState DoMoveToThief()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                return ENodeState.ENS_Failure;

            CheckIsAnomaly();
            
            if (currentThiefTarget == null)
            {
                currentThiefTarget = GetNearestThief();
                if (currentThiefTarget == null)
                    return ENodeState.ENS_Failure;
            }

            float distance = Vector3.Distance(transform.position, currentThiefTarget.transform.position);

            if (distance <= 2.0f)
            {
                agent.isStopped = true;
                agent.ResetPath();
                return ENodeState.ENS_Success;
            }

            agent.isStopped = false;
            agent.SetDestination(currentThiefTarget.transform.position);
            return ENodeState.ENS_Running;
        }

        /// <summary>
        /// 잡기
        /// </summary>
        public ENodeState DoCatch()
        {
            if (currentThiefTarget == null)
                return ENodeState.ENS_Failure;

            float distance = Vector3.Distance(transform.position, currentThiefTarget.transform.position);
            if (distance > 2.5f)
            {
                return ENodeState.ENS_Failure;
            }

            if (!isProgress)
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }

                Vector3 lookDir = (currentThiefTarget.transform.position - transform.position).normalized;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                    transform.forward = lookDir;

                catchTimer = catchDelay;
                isProgress = true;

                Debug.Log($"[Police] 제압 시작 : {currentThiefTarget.name}");
            }

            catchTimer -= Time.deltaTime;

            if (catchTimer > 0f)
                return ENodeState.ENS_Running;

            currentThiefTarget.executor.SetRestricted(10f); // 10초 동안 훔치기 금지

            catchTimer = 0f;
            isProgress = false;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.isStopped = false;

            Debug.Log($"[Police] 제압 완료 : {currentThiefTarget.name}");

            currentThiefTarget = null;
            return ENodeState.ENS_Success;
        }

        #endregion
    }
}