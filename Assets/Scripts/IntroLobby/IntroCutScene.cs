using UnityEngine;
using UnityEngine.Playables;

public class IntroCutscene : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject lobbyUI;
    public GameObject playerController;   // 캐릭터 조작 스크립트 달린 오브젝트

    void Start()
    {
        director.stopped += OnIntroEnd;
        director.Play();   // Play On Awake 꺼두고 여기서 재생해도 OK
    }

    void OnIntroEnd(PlayableDirector d)
    {
        lobbyUI.SetActive(true);
        playerController.SetActive(true);
    }
}