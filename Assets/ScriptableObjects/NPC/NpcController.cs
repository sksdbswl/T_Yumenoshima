// using UnityEngine;
// using UnityEngine.AI;
//
// [RequireComponent(typeof(NavMeshAgent))]
// public class NpcController : NpcInteraction
// {
//     public int NpcId => npcSO.BuilderId;
//     public JobType JobType => npcSO.Job;
//     private NavMeshAgent _agent;
//     
//     //public PlaceableObject Home => npcSO.;
//     //private INpcJobBehaviour _jobBehaviour;
//
//     private void Awake()
//     {
//         _agent = GetComponent<NavMeshAgent>();
//     }
//
//     // public void Initialize(NpcSaveData data)
//     // {
//     //     NpcId = data.npcId;
//     //     JobType = data.jobType;
//     //
//     //     // 집 빌딩 인스턴스 연결
//     //     Home = PlaceableRegistry.Instance.GetById(data.homeBuilderId);
//     //
//     //     // 직업에 따라 Behaviour 할당
//     //     _jobBehaviour = NpcJobFactory.CreateJobBehaviour(JobType, this);
//     // }
//
//     private void Update()
//     {
//         // 직업이 있다면 작업 업데이트
//         //_jobBehaviour?.Tick();
//     }
//
//     // NavMesh 이동용 공용 함수
//     public void MoveTo(Vector3 worldPos)
//     {
//         if (_agent.enabled && _agent.isOnNavMesh)
//         {
//             _agent.SetDestination(worldPos);
//         }
//     }
//
//     public bool ReachedDestination(float threshold = 0.2f)
//     {
//         if (!_agent.pathPending && _agent.remainingDistance <= threshold)
//         {
//             return true;
//         }
//         return false;
//     }
//
//     // public Vector3 GetHomePosition()
//     // {
//     //     return Home != null ? Home.transform.position : transform.position;
//     // }
// }