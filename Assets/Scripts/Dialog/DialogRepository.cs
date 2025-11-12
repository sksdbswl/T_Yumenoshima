using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;
using Random = UnityEngine.Random;

public class DialogRepository : MonoBehaviour
{
    public static DialogRepository I { get; private set; }

    [System.Serializable]
    public class DialogData
    {
        public string Category { get; set; }
        public int Id { get; set; }
        public int Key { get; set; }
        public string Kor { get; set; }
        public string NPC { get; set; }
        public bool IsStory { get; set; }
        public int Stage { get; set; }
        // 필요하면 public int Order { get; set; }
    }

    private List<DialogData> _all;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        _all = LoadAllDialogs();
    }

    List<DialogData> LoadAllDialogs()
    {
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
        };
        string path = Path.Combine(Application.streamingAssetsPath, "Dialog.csv");
        using var sr = new StreamReader(path);
        using var cr = new CsvReader(sr, cfg);
        return cr.GetRecords<DialogData>().ToList();
    }

    public bool HasStory(int npcId, int stage) =>
        _all.Any(d => int.Parse(d.NPC) == npcId && d.IsStory && d.Stage == stage && !PlayerProgress.IsStorySeen(int.Parse(d.NPC)));

    public bool HasAmbient(int npcId) =>
        _all.Any(d => int.Parse(d.NPC) == npcId && !d.IsStory);

    public DialogData PickNext(int npcId, int stage)
    {
        // 1) 현재 단계 스토리 중 아직 안 본 것
        var story = _all
            // .OrderBy(d => d.Order)
            .FirstOrDefault(d => int.Parse(d.NPC) == npcId && d.IsStory && d.Stage == stage && !PlayerProgress.IsStorySeen(int.Parse(d.NPC)));
        if (story != null) return story;

        // 2) 일상 랜덤
        var ambientPool = _all.Where(d => int.Parse(d.NPC) == npcId && !d.IsStory).ToList();
        if (ambientPool.Count == 0) return null;
        return ambientPool[Random.Range(0, ambientPool.Count)];
    }

    public bool IsStageCleared(int npcId, int stage)
    {
        var stageStories = _all.Where(d => int.Parse(d.NPC) == npcId && d.IsStory && d.Stage == stage).ToList();
        if (stageStories.Count == 0) return true;
        return stageStories.All(d => PlayerProgress.IsStorySeen(int.Parse(d.NPC)));
    }
}
