using System;
using System.Collections;
using PixeLadder.EasyTransition;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
public class CutsceneEntry
{
    public string id;
    public TimelineAsset timeline;
}

public class CutsceneSystem : MonoBehaviour
{
    [SerializeField] private GameObject cutSceneCamera;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private CutsceneEntry[] cutscenes;

    private void Start()
    {
        cutSceneCamera.SetActive(false);
        director.Stop();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            PlayCutscene(cutscenes[0]);
    }
    
    [SerializeField] private TransitionEffect fadeEffect;
    [SerializeField] private Image transitionImage;
    
    public void PlayCutscene(CutsceneEntry entry)
    {
        StartCoroutine(PlayCutsceneRoutine(entry));
    }

    private IEnumerator PlayCutsceneRoutine(CutsceneEntry entry)
    {
        // 페이드 아웃
        yield return SceneTransitioner.Instance.PlayTransitionOut();

        // 컷씬 시작
        cutSceneCamera.SetActive(true);
        director.Stop();
        director.playableAsset = entry.timeline;
        director.time = 0;
        director.Play();

        // 페이드 인
        yield return SceneTransitioner.Instance.PlayTransitionIn();

        // 컷씬 끝날 때까지 대기
        yield return new WaitUntil(() => director.state != PlayState.Playing);

        // 컷씬 종료 페이드
        yield return SceneTransitioner.Instance.PlayTransitionOut();
        cutSceneCamera.SetActive(false);
        yield return SceneTransitioner.Instance.PlayTransitionIn();
    }
}