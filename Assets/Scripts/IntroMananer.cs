using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class IntroMananer : MonoBehaviour
{
    public PlayableDirector introDirector;

    void Start()
    {
        introDirector.Play();
    }
}
