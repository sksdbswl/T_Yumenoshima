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
        public int Id { get; set; }       // CSV의 Id 컬럼 (정수)
        public string Key { get; set; }   // 스토리/대사 고유키
        public string Kor { get; set; }
        public string NPC { get; set; }   // 이름 (문자열)
        public bool IsStory { get; set; }
        public int Stage { get; set; }
        public int Order { get; set; }
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
        try
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
            Debug.Log($"[DialogRepository] Load from: {path}");
        
            using var sr = new StreamReader(path);
            using var cr = new CsvReader(sr, cfg);
            var list = cr.GetRecords<DialogData>().ToList();
            Debug.Log($"[DialogRepository] Loaded {list.Count} lines.");
            return list;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogRepository] CSV load failed: {e}");
            return new List<DialogData>(); // null 대신 빈 리스트
        }
    }


    public bool HasStory(int npcId, int stage) =>
        _all.Any(d => d.Id == npcId &&
                      d.IsStory &&
                      d.Stage == stage &&
                      !PlayerProgress.IsStorySeen(d.Key)); // Key로 본 여부 체크

    public bool HasAmbient(int npcId) =>
        _all.Any(d => d.Id == npcId && !d.IsStory);

    // public DialogData PickNext(int npcId, int stage)
    // {
    //     // 1) 현재 단계 스토리 중 아직 안 본 것
    //     var story = _all
    //         .FirstOrDefault(d => d.Id == npcId && d.IsStory && d.Stage == stage && !PlayerProgress.IsStorySeen(d.Key));
    //     if (story != null) return story;
    //
    //     // 2) 일상 랜덤
    //     var ambientPool = _all.Where(d => d.Id == npcId && !d.IsStory).ToList();
    //     if (ambientPool.Count == 0) return null;
    //     return ambientPool[Random.Range(0, ambientPool.Count)];
    // }
    
    public DialogData PickNext(int npcId, int stage)
    {
        int order = PlayerProgress.GetOrder(npcId, stage);

        // 1) 현재 order 찾기
        var story = _all.FirstOrDefault(d =>
            d.Id == npcId && d.IsStory && d.Stage == stage && d.Order == order);

        if (story != null)
            return story;

        // 2) 스토리 끝 → 일상 랜덤
        var ambients = _all.Where(d => d.Id == npcId && !d.IsStory).ToList();
        if (ambients.Count == 0) return null;

        return ambients[Random.Range(0, ambients.Count)];
    }

    public bool IsStageCleared(int npcId, int stage, int nextOrder)
    {
        // 현재 스테이지의 모든 스토리 리스트
        var storySet = _all
            .Where(d => d.Id == npcId && d.IsStory && d.Stage == stage)
            .ToList();

        if (storySet.Count == 0)
            return true; // 스토리가 없다면 자동 완료

        // 마지막 스토리 Order (예: 0,1,2 중 2가 마지막)
        int maxOrder = storySet.Max(s => s.Order);

        // nextOrder가 maxOrder보다 크면 → 스테이지 끝
        return nextOrder > maxOrder;
    }
    
    // public bool IsStageCleared(int npcId, int stage)
    // {
    //     var set = _all.Where(d => d.Id == npcId && d.IsStory && d.Stage == stage).ToList();
    //     if (set.Count == 0) return true;
    //     return set.All(d => PlayerProgress.IsStorySeen(d.Key)); // Key 기준
    // }

}
