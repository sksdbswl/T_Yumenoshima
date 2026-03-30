using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        #region Police : Patrol/Catch
        
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
            if (currentNpc != null)
                return ENodeState.ENS_Success;

            currentNpc = GetNearestThief();

            return currentNpc != null
                ? ENodeState.ENS_Success
                : ENodeState.ENS_Failure;
        }

        public ENodeState DoMoveToThief()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                return ENodeState.ENS_Failure;
            if(TryStopIfAnomaly()) return ENodeState.ENS_Failure;
            
            // 활동 불가 상태
            if (GameManager.Singleton.CurrentRoutine == RoutineState.Night) return ENodeState.ENS_Failure; //저녁엔 활동 불가
            if (currentNpc.executor.isAnomaly) return ENodeState.ENS_Failure; // 상대가 이상상태일땐 따로 제압하지 않아도됨
            
            if (currentNpc == null)
            {
                currentNpc = GetNearestThief();
                if (currentNpc == null)
                    return ENodeState.ENS_Failure;
            }

            float distance = Vector3.Distance(transform.position, currentNpc.transform.position);

            if (distance <= 2.0f)
            {
                agent.isStopped = true;
                agent.ResetPath();
                return ENodeState.ENS_Success;
            }

            agent.isStopped = false;
            agent.SetDestination(currentNpc.transform.position);
            return ENodeState.ENS_Running;
        }

        /// <summary>
        /// 잡기
        /// </summary>
        public ENodeState DoCatch()
        {
            if (currentNpc == null)
                return ENodeState.ENS_Failure;

            float distance = Vector3.Distance(transform.position, currentNpc.transform.position);
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

                Vector3 lookDir = (currentNpc.transform.position - transform.position).normalized;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                    transform.forward = lookDir;

                delayTimer = defaultActionTimer;
                isProgress = true;

                Debug.Log($"[Police] 제압 시작 : {currentNpc.name}");
            }

            delayTimer -= Time.deltaTime;

            if (delayTimer > 0f)
                return ENodeState.ENS_Running;

            currentNpc.executor.SetRestricted(10f); // 10초 동안 훔치기 금지

            delayTimer = 0f;
            isProgress = false;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.isStopped = false;

            Debug.Log($"[Police] 제압 완료 : {currentNpc.name}");

            currentNpc = null;
            return ENodeState.ENS_Success;
        }

        #endregion
    }
}