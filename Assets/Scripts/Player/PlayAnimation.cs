using Animancer;
using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private AnimationClip jumpClip;
    
    // private void Update()
    // {
    //     float move = Input.GetAxis("Vertical");
    //     if (move > 0.1f)
    //         animancer.Play(walkClip);
    //     else
    //         animancer.Play(idleClip);
    // }
}