// public class DialogTyper : MonoBehaviour
// {
//     public class DialogData
//     {
//         public string Category { get; set; }   // 기존 컬럼. 그대로 둬도 됨
//         public string Key { get; set; }
//         public string Kor { get; set; }
//
//         // 추가 3개
//         public string NPC { get; set; }        // 주민 ID/이름
//         public bool IsStory { get; set; }      // true=스토리, false=일상
//         public int Stage { get; set; }         // 스토리 단계(일상은 0)
//     }
//
//     public TMP_Text nameComponent;
//     public TMP_Text textComponent;
//
//     // 게임 어딘가에서 관리한다고 가정(간단히 public으로)
//     public string currentNpcId = "타누키";
//     public int currentStage = 1;
//
//     private Tween currentTween;
//     private IEnumerator currentCoroutine;
//
//     // 간단 저장: 본 적 있는 스토리 Key
//     private HashSet<string> seenStoryKeys = new HashSet<string>();
//
//     private List<DialogData> all; // TSV 전체 캐시
//
//     private void Awake()
//     {
//         all = LoadAllDialogs(); // 한 번만 로드
//     }
//
//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             if (currentCoroutine != null) StopCoroutine(currentCoroutine);
//             if (currentTween != null && currentTween.IsActive()) currentTween.Kill();
//
//             var line = PickNextLine(currentNpcId, currentStage);
//             if (line == null) return;
//
//             nameComponent.SetText(currentNpcId);
//             currentCoroutine = PlayDialog(new []{ line });
//             StartCoroutine(currentCoroutine);
//
//             if (line.IsStory) seenStoryKeys.Add(line.Key); // 스토리 소진 처리
//         }
//     }
//
//     // 최소 필터 로더
//     private List<DialogData> LoadAllDialogs()
//     {
//         var csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
//         {
//             Delimiter = "\t",
//             HasHeaderRecord = true,
//             Mode = CsvMode.NoEscape,
//             BadDataFound = null
//         };
//
//         using var sr = new StreamReader(Path.Combine(Application.streamingAssetsPath, "DialogTable.tsv"));
//         using var cr = new CsvReader(sr, csvConfig);
//         return cr.GetRecords<DialogData>().ToList();
//     }
//
//     // 핵심 선택기: 스토리 먼저, 없으면 일상 랜덤
//     private DialogData PickNextLine(string npcId, int stage)
//     {
//         // 1) 스토리
//         var story = all.FirstOrDefault(d =>
//             d.NPC == npcId &&
//             d.IsStory &&
//             d.Stage == stage &&
//             !seenStoryKeys.Contains(d.Key));
//         if (story != null) return story;
//
//         // 2) 일상
//         var ambientPool = all.Where(d => d.NPC == npcId && !d.IsStory).ToList();
//         if (ambientPool.Count == 0) return null;
//         return ambientPool[Random.Range(0, ambientPool.Count)];
//     }
//
//     public Tween PlayText(string text)
//     {
//         textComponent.SetText("");
//         currentTween = textComponent.DOText(text, text.Length * 0.04f).SetEase(Ease.Linear);
//         return currentTween;
//     }
//
//     IEnumerator PlayDialog(DialogData[] dialogs)
//     {
//         for (int i = 0; i < dialogs.Length; i++)
//         {
//             var tween = PlayText(dialogs[i].Kor);
//             yield return new WaitWhile(() => currentTween != null && currentTween.IsActive() && currentTween.IsPlaying());
//             yield return new WaitForSeconds(0.8f);
//         }
//     }
// }
