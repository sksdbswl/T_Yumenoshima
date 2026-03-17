using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BT.Runtime
{
    //노드 하나가 저장해야 하는 정보
    [Serializable]
    public class BTNodeData
    {
        public string guid;
        public string nodeName;
        public BTNodeType nodeType;
        public Vector2 position;

        // Condition node
        public BTConditionType conditionType;

        // Action node
        public BTActionType actionType;
        public string animationStateName;

        // 연결 정보
        public List<string> childrenGuids = new List<string>();
    }
}