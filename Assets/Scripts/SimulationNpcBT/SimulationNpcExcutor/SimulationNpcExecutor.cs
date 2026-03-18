using AI.BT.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace TestBT
{
    /// <summary>
    /// Action 노드에서 실행될 실제 행동
    /// </summary>
    public partial class SimulationNpcExecutor : MonoBehaviour
    {
        [HideInInspector] public NpcSO npcSO;
        private NavMeshAgent agent;
        private float defaultSpeed = 3f;
        private float runSpeed = 10f;
        private bool isProgress = false;
        
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            SetSpeed(defaultSpeed);
        }
        
        public void MoveTo(Vector3 target)
        {
            agent.isStopped = false;
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
        
        #region Basic : Move, Hide, Jump, Chase

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

        public ENodeState DoHide()
        {
            Debug.Log("숨기");
            
            agent.isStopped = true;
            return ENodeState.ENS_Success;
        }
        
        public void DoJump()
        {
            agent.isStopped = false;
            agent.SetDestination(agent.transform.position + agent.transform.up * 10);
        }
        
        public bool IsProgressing()
        {
            return isProgress;
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
            
            Stop();
            
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
                transform.forward = dir.normalized;

            return ENodeState.ENS_Success;
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
            //Debug.Log("추적 중...");
            agent.isStopped = false;
            agent.SetDestination(target.player.position);
            return ENodeState.ENS_Running;
        }
        
        #endregion
        
        // ───────────────── 도망 ─────────────────
        private bool _hasFleeTarget = false; // 좌표 설정 값
        private Vector3 _currentFleeTarget;
        
        public ENodeState DoFlee(Transform player)
        {
            if (player == null) return ENodeState.ENS_Failure;
            if (!isProgress) isProgress = true;
            
            return DoDistanceRandomMove(player);
        }
        
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
                        SetSpeed(runSpeed);
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
            isProgress = false;
            
            return ENodeState.ENS_Success;
        }
        
        // ───────────────── 귀가 ─────────────────
        private bool _hasHomeTarget = false;
        private Vector3 _homeTarget;
        
        public ENodeState GoHome()
        {
            // 1. 집 찾기
            var house = PlacementManager.Singleton.GetByBuilderId(npcSO.BuilderId);
            if (house == null)
                return ENodeState.ENS_Failure;

            // 2. 목표 설정 (한 번만)
            if (!_hasHomeTarget)
            {
                _homeTarget = house.transform.position;

                agent.isStopped = false;
                SetSpeed(defaultSpeed);
                agent.SetDestination(_homeTarget);

                _hasHomeTarget = true;

                return ENodeState.ENS_Running;
            }

            // 3. 이동 중
            if (agent.pathPending)
                return ENodeState.ENS_Running;

            if (agent.remainingDistance > agent.stoppingDistance)
                return ENodeState.ENS_Running;

            // 4. 도착
            _hasHomeTarget = false;

            return ENodeState.ENS_Success;
        }
    }
}