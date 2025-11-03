using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(fileName = "HairPartsData", menuName = "ScriptableObject/HairPartsData")]
    public class HairPartsData : ScriptableObject
    {
        public List<HairPartsTable> Items;
    }

    [Serializable] public class HairPartsTable
    {
        [Tooltip("아트팀 이름 확인용")]
        public string Name; 
        
        [Tooltip("CustomizingData.Index")]
        public int Index;
        
        [Space(10)] [Tooltip("헤어 착용 시 후드 숨김 필요 여부")]
        public bool ShouldHideHood;
        
        [Space(10)]
        public Color MainColor = Color.red;
        
        [Space(5)]
        public Color GradationColor = Color.white;
        
        [Space(5)]
        public Color BridgeColor = Color.white;
    }
}
