using UnityEngine;

namespace TestBT
{
    /// <summary>
    /// Condition 노드가 여기만 보게 만드는 게 핵심
    /// 데이터만 들고 있도록
    /// </summary>
    public class NpcBlackboard
    {
        public Transform player;
        public float distanceToPlayer;

        public bool isPlayerNear;
        public bool isPlayerVeryNear;
        
        // 기본 설정
        public bool canHome; // 귀가
        
        // npc so에서 설정
        public bool canChase; // 추격
        public bool canAttack; // 공격
        public bool canFlee; // 도망
    }
}