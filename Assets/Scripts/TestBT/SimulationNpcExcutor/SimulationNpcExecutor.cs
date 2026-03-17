using AI.BT.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace TestBT
{
    /// <summary>
    /// Action 노드에서 실행될 실제 행동
    /// </summary>
    public class SimulationNpcExecutor : MonoBehaviour
    {
        private NavMeshAgent agent;
        
        private float defaultSpeed = 2f;
        private float fleeSpeed = 10f;
        
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            SetSpeed(defaultSpeed);
        }
        
        public void MoveTo(Vector3 target)
        {
            agent.SetDestination(target);
        }
        
        public void Stop()
        {
            agent.isStopped = true;
        }
        
        public void SetSpeed(float speed)
        {
            agent.speed = speed;
        }

        public void DoJump()
        {
            agent.isStopped = false;
            agent.SetDestination(agent.transform.position + agent.transform.up * 10);
        }

        public ENodeState DoHide()
        {
            Debug.Log("숨기");
            
            agent.isStopped = true;
            return ENodeState.ENS_Success;
        }
        
        #region Default/Move

        public ENodeState KeepDefault()
        {
            DoRandomMove();
            return ENodeState.ENS_Success;
        }
        
        public void DoRandomMove()
        {
            float radius = 15f;

            Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                if (!agent.hasPath || agent.remainingDistance < 0.5f)
                {
                    SetSpeed(defaultSpeed);
                    agent.SetDestination(hit.position);
                }
            }
        }

        #endregion

        #region LookAt

        /// <summary>
        /// 대상의 높낮이(Y축)까지 포함하여 바라봅니다.
        /// 만약 상대방이 나보다 높은 곳에 있다면 고개가 위로 꺾일 수 있습니다.
        /// tip. 만약 캐릭터가 뚜벅뚜벅 걸어가는 적을 자연스럽게 계속 쳐다보게 하고 싶다면 
        /// </summary>
        public ENodeState DoLookAt()
        {
            Debug.Log("기본 모션 : 바라보기");
            
            transform.LookAt(agent.transform);
            
            return ENodeState.ENS_Running;
        }
        
        /// <summary>
        /// 대상이 어디 있든 **수평(바닥과 평행)**하게 몸만 돌립니다.
        /// 캐릭터가 위아래로 기우뚱해지는 것을 방지하는 로직
        /// tip. 대화 이벤트가 시작되어 NPC가 플레이어를 한 번 슥 쳐다보게 하고 싶다면
        /// </summary>
        public ENodeState DoLookAt(Transform target)
        {
            if (target == null)
                return ENodeState.ENS_Failure;

            Debug.Log("기본 모션 : 바라보기");
            Stop();
            
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
                transform.forward = dir.normalized;

            return ENodeState.ENS_Success;
        }

        #endregion

        #region Chase/Attack

        private bool isAttacking = false;
        private float attackDelay = 5f;
        private float attackTimer = 0f;

        public ENodeState DoChase(NpcBlackboard target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            // 1. 공격 범위 안에 들어왔다면? 추적 성공 반환 -> 다음 공격 노드로
            if (target.isPlayerVeryNear)
            {
                agent.isStopped = true; 
                return ENodeState.ENS_Success; 
            }

            // 2. 아직 멀다면? 계속 이동하며 진행 중 반환
            Debug.Log("추적 중...");
            agent.isStopped = false;
            agent.SetDestination(target.player.position);
            return ENodeState.ENS_Running;
        }

        public ENodeState DoAttack(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            if (!isAttacking)
            {
                // 1. 공격 시작 시점 
                Debug.Log("공격 시작");
                if (agent != null) Stop(); 

                Vector3 lookDir = (target.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.forward = lookDir;

                // animator.SetTrigger("Attack");
                attackTimer = attackDelay;
                isAttacking = true;
            }
  
            attackTimer -= Time.deltaTime;
            
            if (attackTimer > 0f)
            {
                Debug.Log($"공격 중... ::{attackTimer}");
                // 2. 공격 진행 중
                
                return ENodeState.ENS_Running;
            }

            // 3. 공격 완료 시점
            Debug.Log("공격 끝");
            attackTimer = 0f;
            isAttacking = false;
            agent.isStopped = false;
            
            return ENodeState.ENS_Success;
        }

        public bool IsAttacking()
        {
            return isAttacking;
        }

        #endregion

        #region Flee

        private bool isFleeing = false; // 도망 상태 값
        private bool _hasFleeTarget = false; // 좌표 설정 값
        private Vector3 _currentFleeTarget;
        
        public ENodeState DoFlee(Transform player)
        {
            if (player == null) return ENodeState.ENS_Failure;
            if (!isFleeing) isFleeing = true;
            
            return DoDistanceRandomMove(player);
        }
        // public ENodeState DoDistanceRandomMove(Transform player)
        // {
        //     float minDistance = 10f;
        //     float maxDistance = 15f;
        //
        //     if (!_hasFleeTarget)
        //     {
        //         Vector3 awayFromPlayer = transform.position - player.position;
        //         awayFromPlayer.y = 0f;
        //
        //         if (awayFromPlayer.sqrMagnitude < 0.001f)
        //             awayFromPlayer = transform.forward;
        //
        //         awayFromPlayer.Normalize();
        //
        //         for (int i = 0; i < 10; i++)
        //         {
        //             float angle = Random.Range(-60f, 60f);
        //             Vector3 randomDir = Quaternion.Euler(0f, angle, 0f) * awayFromPlayer;
        //
        //             float randomDistance = Random.Range(minDistance, maxDistance);
        //             Vector3 randomPoint = transform.position + randomDir * randomDistance;
        //
        //             if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        //                 continue;
        //
        //             float currentDistToPlayer = Vector3.Distance(transform.position, player.position);
        //             float newDistToPlayer = Vector3.Distance(hit.position, player.position);
        //
        //             if (newDistToPlayer <= currentDistToPlayer)
        //                 continue;
        //
        //             agent.isStopped = false;
        //             SetSpeed(fleeSpeed);
        //             _currentFleeTarget = hit.position;
        //             agent.SetDestination(_currentFleeTarget);
        //             _hasFleeTarget = true;
        //
        //             return ENodeState.ENS_Running;
        //         }
        //
        //         return ENodeState.ENS_Running;
        //     }
        //
        //     if (agent.pathPending)
        //         return ENodeState.ENS_Running;
        //
        //     if (agent.remainingDistance > agent.stoppingDistance)
        //         return ENodeState.ENS_Running;
        //
        //     _hasFleeTarget = false;
        //     isFleeing = false;
        //
        //     return ENodeState.ENS_Success;
        // }
        
        public ENodeState DoDistanceRandomMove(Transform player)
        {
            float minDistance = 10f;
            float maxDistance = 15f;

            if (!_hasFleeTarget)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector3 randomDir = Random.insideUnitSphere;
                    randomDir.y = 0f;
                    randomDir.Normalize();

                    float randomDistance = Random.Range(minDistance, maxDistance);
                    Vector3 randomPoint = transform.position + randomDir * randomDistance;

                    if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    {
                        float currentDistToPlayer = Vector3.Distance(transform.position, player.position); // 두 위치 사이의 거리 계산
                        float newDistToPlayer = Vector3.Distance(hit.position, player.position); // NPC가 그 위치로 이동했을 때 플레이어와의 거리

                        if (newDistToPlayer <= currentDistToPlayer) // 플레이어에게 가까워지는 방향이면 버린다
                            continue;

                        agent.isStopped = false;
                        SetSpeed(fleeSpeed);
                        _currentFleeTarget = hit.position;
                        agent.SetDestination(_currentFleeTarget);
                        _hasFleeTarget = true;

                        return ENodeState.ENS_Running;
                    }
                    
                    // if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    // {
                    //     agent.isStopped = false;
                    //     SetSpeed(fleeSpeed);
                    //     _currentFleeTarget = hit.position;
                    //     agent.SetDestination(_currentFleeTarget);
                    //     _hasFleeTarget = true;
                    //
                    //     return ENodeState.ENS_Running;
                    // }
                }

                return ENodeState.ENS_Running;
            }

            if (agent.pathPending)
                return ENodeState.ENS_Running;

            if (agent.remainingDistance > agent.stoppingDistance)
                return ENodeState.ENS_Running;

            _hasFleeTarget = false;
            isFleeing = false;
            
            return ENodeState.ENS_Success;
        }
        
        public bool IsFleeing()
        {
            return isFleeing;
        }

        #endregion

        public void GoHome()
        {
            agent.isStopped = false;
            agent.SetDestination(transform.position);
        }
    }
}