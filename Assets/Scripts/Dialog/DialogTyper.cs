using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogTyper : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameComponent;
    public TMP_Text textComponent;

    [Header("타자 효과")]
    [Tooltip("한 글자 당 걸리는 시간(초)")]
    public float secondsPerChar = 0.04f;
    [Tooltip("문장 사이 딜레이(초)")]
    public float lineGap = 0.8f;

    private Tween currentTween;
    private Coroutine currentRoutine;
    private string playingFullText = "";
    private readonly Queue<string> _queue = new Queue<string>();
    private string _currentSpeaker = "";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsTyping()) CompleteTypingImmediately();
            else PlayNextFromQueue();
        }
    }

    public void PlayLine(string speakerName, string text)
    {
        _queue.Clear();
        _currentSpeaker = speakerName;
        nameComponent.SetText(_currentSpeaker);
        _queue.Enqueue(text);
        PlayNextFromQueue();
    }

    public void PlayLines(string speakerName, IEnumerable<string> lines)
    {
        _queue.Clear();
        _currentSpeaker = speakerName;
        nameComponent.SetText(_currentSpeaker);
        foreach (var l in lines) _queue.Enqueue(l);
        PlayNextFromQueue();
    }

    public bool IsBusy() => IsTyping() || _queue.Count > 0;

    void PlayNextFromQueue()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        if (currentTween != null && currentTween.IsActive()) currentTween.Kill();

        if (_queue.Count == 0) { textComponent.text = ""; return; }

        var line = _queue.Dequeue();
        currentRoutine = StartCoroutine(PlayDialogLine(line));
    }

    IEnumerator PlayDialogLine(string text)
    {
        PlayText(text);
        yield return new WaitWhile(IsTyping);
        yield return new WaitForSeconds(lineGap);
        currentRoutine = null;

        if (_queue.Count > 0) PlayNextFromQueue();
    }

    public Tween PlayText(string text)
    {
        playingFullText = text;
        textComponent.text = "";

        float duration = Mathf.Max(0.0001f, text.Length * secondsPerChar);

        currentTween = DOTween.To(() => 0f, x =>
        {
            int charCount = Mathf.FloorToInt(x * text.Length);
            textComponent.text = text.Substring(0, Mathf.Clamp(charCount, 0, text.Length));
        }, 1f, duration).SetEase(Ease.Linear);

        return currentTween;
    }

    bool IsTyping() =>
        currentTween != null && currentTween.IsActive() && currentTween.IsPlaying();

    void CompleteTypingImmediately()
    {
        if (IsTyping())
        {
            currentTween.Kill();
            textComponent.text = playingFullText;
        }
    }
}

// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Globalization;
// using System.IO;
// using System.Linq;
// using CsvHelper;
// using CsvHelper.Configuration;
// using DG.Tweening;
// using TMPro;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class DialogTyper : MonoBehaviour
// {
//     [Serializable]
//     public class DialogData
//     {
//         public string Category { get; set; }   // 선택적으로 사용
//         public string Key { get; set; }
//         public string Kor { get; set; }
//         public string NPC { get; set; }        // 주민 ID/이름
//         public bool IsStory { get; set; }      // true=스토리, false=일상
//         public int Stage { get; set; }         // 스토리 단계(일상은 0)
//     }
//
//     [Header("UI")]
//     public TMP_Text nameComponent;
//     public TMP_Text textComponent;
//
//     [Header("현재 상태")]
//     public string currentNpcId = "타누키";
//     public int currentStage = 1;
//
//     [Header("타자 효과")]
//     [Tooltip("한 글자 당 걸리는 시간(초)")]
//     public float secondsPerChar = 0.04f;
//     [Tooltip("문장 사이 딜레이(초)")]
//     public float lineGap = 0.8f;
//
//     private Tween currentTween;
//     private Coroutine currentRoutine;
//
//     // 간단 저장: 본 적 있는 스토리 Key (세션 유지용)
//     private HashSet<string> seenStoryKeys = new HashSet<string>();
//
//     private List<DialogData> all; // CSV 전체 캐시
//
//     // 진행 중 문장 캐시 (스킵/완료 처리용)
//     private string playingFullText = "";
//
//     private void Awake()
//     {
//         all = LoadAllDialogs();
//     }
//
//     private void Update()
//     {
//         // 스페이스 입력:
//         // 1) 타이핑 중이면 즉시 완성
//         // 2) 이미 완성/대기 중이면 다음 라인 재생
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             if (IsTyping())
//             {
//                 // 즉시 완성
//                 CompleteTypingImmediately();
//             }
//             else
//             {
//                 PlayNextOne();
//             }
//         }
//     }
//
//     private void PlayNextOne()
//     {
//         if (currentRoutine != null) StopCoroutine(currentRoutine);
//         if (currentTween != null && currentTween.IsActive()) currentTween.Kill();
//
//         var line = PickNextLine(currentNpcId, currentStage);
//         if (line == null) return;
//
//         nameComponent.SetText(currentNpcId);
//         currentRoutine = StartCoroutine(PlayDialog(new[] { line }));
//
//         if (line.IsStory)
//         {
//             seenStoryKeys.Add(line.Key); // 스토리 소진 처리(세션 한정)
//             // 영구 저장 원하면 아래 PlayerPrefs 사용:
//             // PlayerPrefs.SetInt($"seen_{line.Key}", 1);
//         }
//     }
//
//     // CSV 로더 (콤마 구분)
//     private List<DialogData> LoadAllDialogs()
//     {
//         var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
//         {
//             Delimiter = ",",
//             HasHeaderRecord = true,
//             BadDataFound = null,
//             MissingFieldFound = null,
//             TrimOptions = TrimOptions.Trim,
//         };
//
//         string path = Path.Combine(Application.streamingAssetsPath, "Dialog.csv");
//
// #if UNITY_ANDROID && !UNITY_EDITOR
//         // Android의 StreamingAssets 접근은 파일로 직접 못 읽을 수 있어 UnityWebRequest 사용이 안전함
//         // 여기서는 간단히 동기 파일 경로만 예시로 제공. 필요시 코루틴으로 UWR 로드로 대체하세요.
//         Debug.LogWarning("Android에서는 StreamingAssets를 동기 파일로 읽기 어려울 수 있습니다. UnityWebRequest로 로드하도록 수정하세요.");
// #endif
//
//         using var sr = new StreamReader(path);
//         using var cr = new CsvReader(sr, csvConfig);
//         return cr.GetRecords<DialogData>().ToList();
//     }
//
//     // 핵심 선택기: 스토리 먼저, 없으면 일상 랜덤
//     private DialogData PickNextLine(string npcId, int stage)
//     {
//         // 1) 스토리 (아직 보지 않은)
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
//         playingFullText = text;
//         textComponent.text = "";
//
//         float duration = Mathf.Max(0.0001f, text.Length * secondsPerChar);
//
//         currentTween = DOTween.To(() => 0f, x =>
//         {
//             int charCount = Mathf.FloorToInt(x * text.Length);
//             textComponent.text = text.Substring(0, Mathf.Clamp(charCount, 0, text.Length));
//         }, 1f, duration).SetEase(Ease.Linear);
//
//         return currentTween;
//     }
//
//     private bool IsTyping()
//     {
//         return currentTween != null && currentTween.IsActive() && currentTween.IsPlaying();
//     }
//
//     private void CompleteTypingImmediately()
//     {
//         if (IsTyping())
//         {
//             currentTween.Kill();
//             textComponent.text = playingFullText;
//         }
//     }
//
//     private IEnumerator PlayDialog(DialogData[] dialogs)
//     {
//         for (int i = 0; i < dialogs.Length; i++)
//         {
//             var tween = PlayText(dialogs[i].Kor);
//             // 타이핑 끝날 때까지 대기 (스페이스로 즉시완성 가능)
//             yield return new WaitWhile(() => IsTyping());
//             yield return new WaitForSeconds(lineGap);
//         }
//
//         currentRoutine = null;
//     }
// }
