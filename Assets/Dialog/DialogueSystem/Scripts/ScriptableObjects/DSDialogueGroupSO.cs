using UnityEngine;

namespace DS.ScriptableObjects
{
    // 대화 그룹(카테고리)
    public class DSDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}