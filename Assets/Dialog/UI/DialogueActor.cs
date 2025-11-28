using UnityEngine;
using Animancer;

public class DialogueActor : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancer;

    public void PlayClip(AnimationClip clip)
    {
        if (animancer == null || clip == null) return;
        animancer.Play(clip);
    }
}