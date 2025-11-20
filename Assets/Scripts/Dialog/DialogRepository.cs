using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;
using Random = UnityEngine.Random;

public class DialogRepository : SingletonBase<DialogRepository>
{
    [System.Serializable]
    public class DialogData
    {
        public string Category { get; set; }
        public int Id { get; set; }             // NPC Id
        public string Key { get; set; }         // 외부용 라벨 (진행에는 안씀)
        public string Kor { get; set; }         // 실제 대사
        public string NPC { get; set; }         // NPC 이름
        public bool IsStory { get; set; }       // true = 스토리, false = 일상

        // 새 CSV 필드들
        public int WorldStageMin { get; set; }  // 등장 가능한 최소 월드 스테이지
        public int WorldStageMax { get; set; }  // 등장 가능한 최대 월드 스테이지
        public int NpcStoryStage { get; set; }  // NPC 개인 스토리 스테이지

        public int Order { get; set; }          // 해당 NpcStoryStage 내 순서
        public string Speaker { get; set; }     // "NPC" or "Player"
    }

    private List<DialogData> _all;

    void Awake()
    {
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

    /// <summary>
    /// 현재 NPC / 월드 스테이지 / NPC 스토리 스테이지 / Order 기준으로 다음 대사 한 줄 가져오기
    /// </summary>
    public DialogData PickNext(int npcId, int worldStage, int npcStoryStage)
    {
        if (_all == null)
        {
            Debug.LogError("[DialogRepository] _all is null! Check CSV load.");
            return null;
        }

        int order = PlayerProgress.GetOrder(npcId, npcStoryStage);

        // 1) Story에서 (Id, NpcStoryStage, Order, WorldStage범위) 딱 맞는 줄 찾기
        var story = _all.FirstOrDefault(d =>
            d.Id == npcId &&
            d.IsStory &&
            d.NpcStoryStage == npcStoryStage &&
            d.Order == order &&
            worldStage >= d.WorldStageMin &&
            worldStage <= d.WorldStageMax
        );

        if (story != null)
            return story;

        // 2) 해당 월드 스테이지에서 사용 가능한 일상 대사 랜덤
        var ambients = _all
            .Where(d =>
                d.Id == npcId &&
                !d.IsStory &&
                worldStage >= d.WorldStageMin &&
                worldStage <= d.WorldStageMax)
            .ToList();

        if (ambients.Count == 0) return null;

        return ambients[Random.Range(0, ambients.Count)];
    }

    /// <summary>
    /// 이 NPC 스토리 스테이지의 Story가 모두 끝났는지 확인
    /// nextOrder: 방금 대사(line.Order)를 처리한 후 다음 Order값
    /// </summary>
    public bool IsStageCleared(int npcId, int npcStoryStage, int nextOrder)
    {
        var storySet = _all
            .Where(d => d.Id == npcId && d.IsStory && d.NpcStoryStage == npcStoryStage)
            .ToList();

        if (storySet.Count == 0)
            return true; // 이 Stage에 스토리 자체가 없으면 자동 완료

        int maxOrder = storySet.Max(s => s.Order);
        return nextOrder > maxOrder; // 마지막 Order보다 크면 Stage 종료
    }
}
