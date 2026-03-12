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
        private float fleeSpeed = 8f;
        
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
        
        // public INode.ENodeState DoFlee(Transform player)
        // {
        //     Debug.Log("도망");
        //
        //     Vector3 dir = (transform.position - player.position).normalized;
        //     float fleeDistance = 8f;
        //
        //     Vector3 randomOffset = Random.insideUnitSphere * 3f;
        //
        //     Vector3 target = transform.position + dir * fleeDistance + randomOffset;
        //
        //     NavMeshHit hit;
        //     if (NavMesh.SamplePosition(target, out hit, fleeDistance, NavMesh.AllAreas))
        //     {
        //         SetSpeed(fleeSpeed);
        //         agent.isStopped = false;
        //         agent.SetDestination(hit.position);
        //     }
        //
        //     // 아직 이동 중이면 Running 유지
        //     if (agent.pathPending)
        //         return INode.ENodeState.ENS_Running;
        //
        //     if (agent.remainingDistance > agent.stoppingDistance)
        //         return INode.ENodeState.ENS_Running;
        //
        //     // 도착했으면 성공
        //     SetSpeed(defaultSpeed);
        //     return INode.ENodeState.ENS_Success;
        // }

        private bool _hasFleeTarget;
        private Vector3 _currentFleeTarget;
        
        public ENodeState DoFlee(Transform player)
        {
            if (player == null)
                return ENodeState.ENS_Failure;

            if (!_hasFleeTarget) // false
            {
                if (!TryFindFleePoint(player.position, out _currentFleeTarget))
                    return ENodeState.ENS_Running;

                SetSpeed(fleeSpeed);
                agent.isStopped = false;
                agent.SetDestination(_currentFleeTarget);
                _hasFleeTarget = true;
            }

            if (agent.pathPending)
                return ENodeState.ENS_Running;

            if (agent.remainingDistance > agent.stoppingDistance)
                return ENodeState.ENS_Running;

            _hasFleeTarget = false;
            SetSpeed(defaultSpeed);
            return ENodeState.ENS_Success;
        }
        
        private bool TryFindFleePoint(Vector3 threatPos, out Vector3 bestPoint)
        {
            bestPoint = transform.position;

            Vector3 fleeDir = (transform.position - threatPos).normalized;
            float fleeDistance = 8f;

            float bestScore = float.MinValue;
            bool found = false;

            for (int i = 0; i < 12; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 4f;
                offset.y = 0f;

                Vector3 candidate = transform.position + fleeDir * fleeDistance + offset;

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    continue;

                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(hit.position, path))
                    continue;

                if (path.status != NavMeshPathStatus.PathComplete)
                    continue;

                // 플레이어에게서 멀어질수록 높은 점수
                float distFromThreat = Vector3.Distance(hit.position, threatPos);

                // 현재 위치와 너무 가까운 점은 제외
                float moveDist = Vector3.Distance(transform.position, hit.position);
                if (moveDist < 2f)
                    continue;

                if (distFromThreat > bestScore)
                {
                    bestScore = distFromThreat;
                    bestPoint = hit.position;
                    found = true;
                }
            }

            return found;
        }

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
        
        private bool isAttacking = false;
        private float attackDelay = 5f;
        private float attackTimer = 0f;

        // Selector children:
        // 1. IsAttacking -> Attack
        // 2. IsPlayerVeryNear -> CanAttack -> Attack
        // 3. IsPlayerNear -> CanChase -> Chase
        // 4. KeepDefault
        
        public ENodeState DoAttack(Transform target)
        {
            if (target == null) return ENodeState.ENS_Failure;

            if (!isAttacking)
            {
                // 1. 공격 시작 시점 (최초 1회)
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
    }
}