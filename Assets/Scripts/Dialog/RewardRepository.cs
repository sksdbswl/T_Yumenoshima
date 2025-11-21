using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public class RewardRepository : SingletonBase<RewardRepository>
{
    [System.Serializable]
    public class RewardData
    {
        public int NpcId { get; set; }
        public int NpcStoryStage { get; set; }
        public string RewardId { get; set; }
        public int Gold { get; set; }
        public int Exp { get; set; }
        public string ItemId { get; set; }
        public int ItemCount { get; set; }
    }

    private Dictionary<(int npcId, int stage), RewardData> _dict;

    void Awake()
    {
        LoadAllRewards();
    }

    void LoadAllRewards()
    {
        try
        {
            var cfg = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
            };

            string path = Path.Combine(Application.streamingAssetsPath, "Reward.csv");
            using var sr = new StreamReader(path);
            using var cr = new CsvReader(sr, cfg);
            var list = cr.GetRecords<RewardData>().ToList();

            _dict = list.ToDictionary(
                r => (r.NpcId, r.NpcStoryStage),
                r => r
            );

            Debug.Log($"[RewardRepository] Loaded {list.Count} rewards.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RewardRepository] CSV load failed: {e}");
            _dict = new Dictionary<(int, int), RewardData>();
        }
    }

    public RewardData GetReward(int npcId, int npcStoryStage)
    {
        if (_dict != null && _dict.TryGetValue((npcId, npcStoryStage), out var data))
            return data;
        return null;
    }
}