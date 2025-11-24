using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private DialogSO _dialogSO; // JsonImporterSOEditor로 채워진 SO를 인스펙터에서 할당
    [SerializeField] private List<DialogData> _all;
    [SerializeField] private QuestMarkerUI _questMarkerUIPrefab;

    void Awake()
    {
        _all = LoadAllDialogs();
    }

    List<DialogData> LoadAllDialogs()
    {
        // CSV 대신 ScriptableObject에서 데이터를 읽어옴
        if (_dialogSO == null)
        {
            Debug.LogError("[DialogRepository] DialogSO is null! 인스펙터에 DialogSO를 할당하세요.");
            return new List<DialogData>();
        }

        if (_dialogSO.Values == null || _dialogSO.Values.Count == 0)
        {
            Debug.LogWarning("[DialogRepository] DialogSO.Values is empty.");
            return new List<DialogData>();
        }

        var list = new List<DialogData>(_dialogSO.Values.Count);

        foreach (var row in _dialogSO.Values)
        {
            // DialogSO 내부의 Row 데이터를 런타임용 DialogData로 복사
            list.Add(new DialogData
            {
                Category      = row.Category,
                Id            = row.Id,
                Key           = row.Key,
                Kor           = row.Kor,
                NPC           = row.NPC,
                IsStory       = row.IsStory,
                WorldStageMin = row.WorldStageMin,
                WorldStageMax = row.WorldStageMax,
                NpcStoryStage = row.NpcStoryStage,
                Order         = row.Order,
                Speaker       = row.Speaker
            });
        }

        Debug.Log($"[DialogRepository] Loaded {list.Count} lines from DialogSO.");
        return list;
    }

    /// <summary>
    /// 현재 NPC / 월드 스테이지 / NPC 스토리 스테이지 / Order 기준으로 다음 대사 한 줄 가져오기
    /// </summary>
    public DialogData PickNext(int npcId, int worldStage, int npcStoryStage)
    {
        if (_all == null)
        {
            Debug.LogError("[DialogRepository] _all is null! Check data load.");
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
    
    /// <summary>
    /// 현재 진행도 기준으로, 남아 있는 스토리 대사가 하나라도 있는지 체크
    /// (머리 위 퀘스트 마커 표시용)
    /// </summary>
    public bool HasStoryQuest(int npcId, int worldStage, int npcStoryStage, int currentOrder)
    {
        foreach (var line in _all)
        {
            // 1) 이 NPC 대사인가?
            if (line.Id != npcId)
                continue;

            // 2) 스토리 대사만 대상
            if (!line.IsStory)
                continue;

            // 3) 현재 월드 스테이지에서 열려 있는 대사인가?
            if (worldStage < line.WorldStageMin || worldStage > line.WorldStageMax)
                continue;

            // 4) 현재 NPC 스토리 스테이지와 일치?
            if (line.NpcStoryStage != npcStoryStage)
                continue;

            // 5) 아직 보지 않은 순번인가? (현재 order 이후)
            if (line.Order >= currentOrder)
            {
                return true; // 남은 스토리 대사 있음 → 퀘스트 마커 ON
            }
        }

        return false; // 남은 스토리 대사 없음 → 퀘스트 마커 OFF
    }

    public QuestMarkerUI SpawnMarker()
    {
        var marker = Instantiate(_questMarkerUIPrefab, Vector3.zero, Quaternion.identity, transform);
        return marker;
    }
}



// csv용 버전 -> json으로 변경

// using System.Collections.Generic;
// using System.Globalization;
// using System.IO;
// using System.Linq;
// using CsvHelper;
// using CsvHelper.Configuration;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class DialogRepository : SingletonBase<DialogRepository>
// {
//     [System.Serializable]
//     public class DialogData
//     {
//         public string Category { get; set; }
//         public int Id { get; set; }             // NPC Id
//         public string Key { get; set; }         // 외부용 라벨 (진행에는 안씀)
//         public string Kor { get; set; }         // 실제 대사
//         public string NPC { get; set; }         // NPC 이름
//         public bool IsStory { get; set; }       // true = 스토리, false = 일상
//
//         // 새 CSV 필드들
//         public int WorldStageMin { get; set; }  // 등장 가능한 최소 월드 스테이지
//         public int WorldStageMax { get; set; }  // 등장 가능한 최대 월드 스테이지
//         public int NpcStoryStage { get; set; }  // NPC 개인 스토리 스테이지
//
//         public int Order { get; set; }          // 해당 NpcStoryStage 내 순서
//         public string Speaker { get; set; }     // "NPC" or "Player"
//     }
//
//     private List<DialogData> _all;
//
//     void Awake()
//     {
//         _all = LoadAllDialogs();
//     }
//
//     List<DialogData> LoadAllDialogs()
//     {
//         try
//         {
//             var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
//             {
//                 Delimiter = ",",
//                 HasHeaderRecord = true,
//                 BadDataFound = null,
//                 MissingFieldFound = null,
//                 TrimOptions = TrimOptions.Trim,
//             };
//
//             string path = Path.Combine(Application.streamingAssetsPath, "Dialog.csv");
//             Debug.Log($"[DialogRepository] Load from: {path}");
//
//             using var sr = new StreamReader(path);
//             using var cr = new CsvReader(sr, cfg);
//             var list = cr.GetRecords<DialogData>().ToList();
//
//             Debug.Log($"[DialogRepository] Loaded {list.Count} lines.");
//             return list;
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"[DialogRepository] CSV load failed: {e}");
//             return new List<DialogData>(); // null 대신 빈 리스트
//         }
//     }
//
//     /// <summary>
//     /// 현재 NPC / 월드 스테이지 / NPC 스토리 스테이지 / Order 기준으로 다음 대사 한 줄 가져오기
//     /// </summary>
//     public DialogData PickNext(int npcId, int worldStage, int npcStoryStage)
//     {
//         if (_all == null)
//         {
//             Debug.LogError("[DialogRepository] _all is null! Check CSV load.");
//             return null;
//         }
//
//         int order = PlayerProgress.GetOrder(npcId, npcStoryStage);
//
//         // 1) Story에서 (Id, NpcStoryStage, Order, WorldStage범위) 딱 맞는 줄 찾기
//         var story = _all.FirstOrDefault(d =>
//             d.Id == npcId &&
//             d.IsStory &&
//             d.NpcStoryStage == npcStoryStage &&
//             d.Order == order &&
//             worldStage >= d.WorldStageMin &&
//             worldStage <= d.WorldStageMax
//         );
//
//         if (story != null)
//             return story;
//
//         // 2) 해당 월드 스테이지에서 사용 가능한 일상 대사 랜덤
//         var ambients = _all
//             .Where(d =>
//                 d.Id == npcId &&
//                 !d.IsStory &&
//                 worldStage >= d.WorldStageMin &&
//                 worldStage <= d.WorldStageMax)
//             .ToList();
//
//         if (ambients.Count == 0) return null;
//
//         return ambients[Random.Range(0, ambients.Count)];
//     }
//
//     /// <summary>
//     /// 이 NPC 스토리 스테이지의 Story가 모두 끝났는지 확인
//     /// nextOrder: 방금 대사(line.Order)를 처리한 후 다음 Order값
//     /// </summary>
//     public bool IsStageCleared(int npcId, int npcStoryStage, int nextOrder)
//     {
//         var storySet = _all
//             .Where(d => d.Id == npcId && d.IsStory && d.NpcStoryStage == npcStoryStage)
//             .ToList();
//
//         if (storySet.Count == 0)
//             return true; // 이 Stage에 스토리 자체가 없으면 자동 완료
//
//         int maxOrder = storySet.Max(s => s.Order);
//         return nextOrder > maxOrder; // 마지막 Order보다 크면 Stage 종료
//     }
// }
