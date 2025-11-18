using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Npc/NpcData", fileName = "NpcData")]
public class NpcDataSO : ScriptableObject
{
        public List<NpcSO> Values = new();
        public Dictionary<int, NpcSO> Items = new();

        // Addressables 로드 직후 이 함수를 반드시 호출해야 한다.
        public void BuildDictionary()
        {
                Items.Clear();
                foreach (var npc in Values)
                {
                        if (npc == null) continue;
                        Items[npc.BuilderId] = npc;
                }
        }
}
