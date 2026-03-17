using System.Collections.Generic;
using UnityEngine;

namespace AI.BT.Runtime
{
    //그래프 전체를 저장할 ScriptableObject
    [CreateAssetMenu(fileName = "BTGraph", menuName = "AI/NPC/Behavior Tree Graph")]
    public class BTGraphAsset : ScriptableObject
    {
        public string rootGuid;
        public List<BTNodeData> nodes = new List<BTNodeData>();
    }
}