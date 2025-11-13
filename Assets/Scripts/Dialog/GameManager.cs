using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    private int Stage = 0;

    private void Awake()
    {
        SpawnNpcForStage(Stage);
    }
    
    
    /// <summary>
    /// 스테이지 별 npc 스폰
    /// </summary>
    /// <param name="stage"></param>
    public void SpawnNpcForStage(int stage)
    {
        var table = AssetManager.Singleton.GetNpcDataSO();

        foreach (var npcData in table.Items.Values)
        {
            if (npcData.Stage != stage)
                continue;

            var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcData.Prefab);
            npcObj.transform.position = npcData.spawnPoint;

            npcObj.GetComponent<NpcInteraction>().npcSO = npcData;
        }
    }

    /// <summary>
    /// 특정 npc 스폰
    /// </summary>
    /// <param name="stage"></param>
    public void SpawnNpc(int id)
    {
        var table = AssetManager.Singleton.GetNpcDataSO();

        if (!table.Items.TryGetValue(id, out var npcSO))
        {
            Debug.LogError($"NPC ID {id} not found");
            return;
        }

        if (Stage != npcSO.Stage)
            return;

        var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcSO.Prefab);

        npcObj.transform.position = npcSO.spawnPoint;

        npcObj.GetComponent<NpcInteraction>().npcSO = npcSO;
    }

}