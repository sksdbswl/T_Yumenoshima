using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(fileName = "MakeupColorData", menuName = "ScriptableObject/MakeupColorData")]
    public class MakeupColorData : ScriptableObject
    {
        public List<MakeupColorTable> Items;
    }

    [Serializable] public class MakeupColorTable
    {
        [Tooltip("아트팀 이름 확인용")]
        public string Name; 
        
        [Tooltip("CustomizingData.Index")]
        public int Index;
        
        [Space(10)]
        [Tooltip("커스터마이징 UI 등에서 해당 Index의 화장 아이템 선택 시, 설정한 Color로 기본색 지정")]
        public Color Color;
    }
}
