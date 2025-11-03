using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(menuName = "REIW/Character Generator Config")]
    public class GenerateConfig : ScriptableObject
    {
        [Header("Root 오브젝트에 붙을 스크립트들")]
        public List<MonoScript> rootScripts;

        [Header("FBX 캐릭터(Animator가 붙은 곳)에 붙을 스크립트들")]
        public List<MonoScript> characterScripts;
    }
}
