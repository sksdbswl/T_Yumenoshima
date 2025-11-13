using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Npc/NpcData", fileName = "NpcData")]
public class NpcDataSO : ScriptableObject, ISerializationCallbackReceiver
{
        public List<NpcSO> Values = new();          // 인스펙터에서 채움
        public Dictionary<int, NpcSO> Items = new();// Id로 빠르게 조회용

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
                Items.Clear();
                foreach (var npc in Values)
                {
                        if (npc == null) continue;
                        Items.TryAdd(npc.Id, npc);
                }
        }
}
