//using Animancer;
using UnityEngine;
using System.Collections.Generic;

namespace REIW.Animations
{
    public class AnimancerSoundEvent : MonoBehaviour
    {
        void Awake() {
     
            // // 1) 중앙 이벤트 딕셔너리에 이름별 콜백 등록
            // // Transition에서 같은 이름을 쓰기만 하면 전부 이 콜백으로 모입니다.
            // foreach (var e in bank.entries) {
            //     animancer.Events.AddTo(e.eventName, HandleEvent); // Central Events
            // }                                                       // :contentReference[oaicite:6]{index=6}
            //
            // // // 간단한 오디오 풀
            // // for (int i = 0; i < poolSize; i++) {
            // //     var go = new GameObject($"SFX_{i}");
            // //     go.transform.SetParent(transform, false);
            // //     var src = go.AddComponent<AudioSource>();
            // //     src.playOnAwake = false;
            // //     _pool.Enqueue(src);
            // // }
        }
        
        // public void PlaySound(int type)
        // {
        //     if (type == 1)
        //     {
        //         SoundManager.Singleton.PlaySfx(type);
        //     }
        // }
        
    }
}
