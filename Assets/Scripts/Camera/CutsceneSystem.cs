using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
            PlayCutscene("0");
    }
    
    public void PlayCutscene(string id)
    {
        cutSceneCamera.SetActive(true);
        
        foreach (var entry in cutscenes)
        {
            if (entry.id == id)
            {
                director.Stop();
                director.playableAsset = entry.timeline;
                director.time = 0;
                director.Play();
                return;
            }
        }

        Debug.LogWarning($"컷씬을 찾을 수 없음: {id}");
    }
}