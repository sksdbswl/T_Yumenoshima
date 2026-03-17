using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
 
public class TimelineController : MonoBehaviour 
{
    public PlayableDirector playableDirector;
    public TimelineAsset timeline;

    private void Start()
    {
        Play();
    }
 
    public void Play()
    {
        // 현재 playableDirector에 등록되어 있는 타임라인을 실행
        playableDirector.Play();
    }
 
    public void PlayFromTimeline()
    {
        // 새로운 timeline을 시작
        playableDirector.Play (timeline);
    }
 
}