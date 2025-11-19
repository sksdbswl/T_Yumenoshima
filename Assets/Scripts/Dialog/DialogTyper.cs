using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogTyper:SingletonBase<DialogTyper>
{
    [Header("UI")]
    public GameObject DialogUI;
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
