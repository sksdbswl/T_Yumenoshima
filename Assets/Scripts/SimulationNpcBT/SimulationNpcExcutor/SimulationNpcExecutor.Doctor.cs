using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        #region Doctor : Patrol/Healing

        private float healDelay = 3f;
        private float healTimer = 0f;

        private Npc currentNpcStatus;

        /// <summary>
        /// 가까운 아픈 Npc 찾기
        /// </summary>
        public Npc GetNearestPatient()
        {
            var npcs = GameManager.Singleton.SpawnedNpcStatuses;

            Npc nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < npcs.Count; i++)
            {
                // var controller = npcs[i];
                // if (controller == null) continue;
                // if (controller.transform == transform) continue;
                // if (npcs[i] == null) continue;
                
                if (npcs[i]._npcStatus.CurrentEmotion != Const.EEmotion.Tired) continue;
                
                float sqr = (npcs[i]._npcStatus.GetTransform().position - transform.position).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = npcs[i];
                }
            }

            return nearest;
        }

        public ENodeState DoFindPatient()
        {
            if (currentNpcStatus != null && currentNpcStatus._npcStatus.CurrentEmotion == Const.EEmotion.Tired)
                return ENodeState.ENS_Success;

            currentNpcStatus = GetNearestPatient();

            return currentNpcStatus != null
                ? ENodeState.ENS_Success
                : ENodeState.ENS_Failure;
        }

        public ENodeState DoMoveToPatient()
        {
            if (agent == null) return ENodeState.ENS_Failure;

            if (currentNpcStatus == null || currentNpcStatus._npcStatus.CurrentEmotion != Const.EEmotion.Tired)
            {
                currentNpcStatus = GetNearestPatient();
                if (currentNpcStatus == null)
                    return ENodeState.ENS_Failure;
            }

            Transform target = currentNpcStatus._npcStatus.GetTransform();
            if (target == null) return ENodeState.ENS_Failure;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= 2.0f)
            {
                agent.isStopped = true;
                return ENodeState.ENS_Success;
            }

            agent.isStopped = false;
            agent.SetDestination(target.position);
            return ENodeState.ENS_Running;
        }

        public ENodeState DoHeal()
        {
            if (currentNpcStatus == null) return ENodeState.ENS_Failure;
            if (currentNpcStatus._npcStatus.CurrentEmotion != Const.EEmotion.Tired)
            {
                currentNpcStatus = null;
                return ENodeState.ENS_Failure;
            }

            Transform target = currentNpcStatus._npcStatus.GetTransform();
            if (target == null)
            {
                currentNpcStatus = null;
                return ENodeState.ENS_Failure;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > 2.5f)
            {
                return ENodeState.ENS_Failure;
            }

            if (!isProgress)
            {
                if (agent != null) Stop();

                Vector3 lookDir = (target.position - transform.position).normalized;
                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                    transform.forward = lookDir;

                healTimer = healDelay;
                isProgress = true;

                Debug.Log($"[Doctor] 치료 시작 : {target.name}");
            }

            healTimer -= Time.deltaTime;

            if (healTimer > 0f)
            {
                return ENodeState.ENS_Running;
            }

            currentNpcStatus._npcStatus.ChangeEmotion(Const.EEmotion.Neutral);

            healTimer = 0f;
            isProgress = false;

            if (agent != null)
                agent.isStopped = false;

            Debug.Log($"[Doctor] 치료 완료 : {target.name}");

            currentNpcStatus.agent.isStopped = false;
            currentNpcStatus = null;
            return ENodeState.ENS_Success;
        }

        #endregion
    }
}