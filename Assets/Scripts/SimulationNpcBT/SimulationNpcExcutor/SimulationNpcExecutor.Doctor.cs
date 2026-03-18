using AI.BT.Runtime;
using UnityEngine;

namespace TestBT
{
    public partial class SimulationNpcExecutor
    {
        #region Doctor : Patrol/Healing

        private float healDelay = 3f;
        private float healTimer = 0f;

        private INpcStatus currentNpcStatus;

        /// <summary>
        /// 가까운 아픈 Npc 찾기
        /// </summary>
        /// <returns></returns>
        public INpcStatus GetNearestPatient()
        {
            var npcs = GameManager.Singleton.SpawnedNpcStatuses;

            INpcStatus nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < npcs.Count; i++)
            {
                var controller = npcs[i];
                if (controller == null) continue;
                if (controller.transform == transform) continue;

                var npc = controller.GetComponent<INpcStatus>();
                if (npc == null) continue;
                if (npc.CurrentEmotion != Const.EEmotion.Tired) continue;
                
                float sqr = (npc.GetTransform().position - transform.position).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = npc;
                }
            }

            return nearest;
        }

        public ENodeState DoFindPatient()
        {
            if (currentNpcStatus != null && currentNpcStatus.CurrentEmotion == Const.EEmotion.Tired)
                return ENodeState.ENS_Success;

            currentNpcStatus = GetNearestPatient();

            return currentNpcStatus != null
                ? ENodeState.ENS_Success
                : ENodeState.ENS_Failure;
        }

        public ENodeState DoMoveToPatient()
        {
            if (agent == null) return ENodeState.ENS_Failure;

            if (currentNpcStatus == null || currentNpcStatus.CurrentEmotion != Const.EEmotion.Tired)
            {
                currentNpcStatus = GetNearestPatient();
                if (currentNpcStatus == null)
                    return ENodeState.ENS_Failure;
            }

            Transform target = currentNpcStatus.GetTransform();
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
            if (currentNpcStatus.CurrentEmotion != Const.EEmotion.Tired)
            {
                currentNpcStatus = null;
                return ENodeState.ENS_Failure;
            }

            Transform target = currentNpcStatus.GetTransform();
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

            //currentNpcStatus.Heal(100);
            currentNpcStatus.ChangeEmotion(Const.EEmotion.Neutral);

            healTimer = 0f;
            isProgress = false;

            if (agent != null)
                agent.isStopped = false;

            Debug.Log($"[Doctor] 치료 완료 : {target.name}");

            currentNpcStatus = null;
            return ENodeState.ENS_Success;
        }

        #endregion
    }
}