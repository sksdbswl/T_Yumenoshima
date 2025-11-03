using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(menuName = "REIW/Character/Create Character Avatar Bone Config")]
    public class CharacterAvatarBoneConfig : ScriptableObject
    {
        [field: SerializeField]
        public UDictionary<ReiwHumanBodyBones, string> AvatarBoneKeyValueMap { get; private set; } = new();

#if UNITY_EDITOR
        private static bool ShouldSkip(ReiwHumanBodyBones bone)
            => bone == ReiwHumanBodyBones.None || bone == ReiwHumanBodyBones.LastBone;

        private void OnValidate()
        {
            if (AvatarBoneKeyValueMap == null)
                AvatarBoneKeyValueMap = new UDictionary<ReiwHumanBodyBones, string>();

            // 1) 누락된 본 키 자동 추가 (값은 string.Empty)
            var allBones = (ReiwHumanBodyBones[])Enum.GetValues(typeof(ReiwHumanBodyBones));
            foreach (var bone in allBones)
            {
                if (ShouldSkip(bone)) continue;
                if (!AvatarBoneKeyValueMap.ContainsKey(bone))
                    AvatarBoneKeyValueMap[bone] = string.Empty; // 덮어쓰지 않고 "추가"만 수행
            }

            // 2) Enum에서 제거된 키 정리
            //    - enum 변경 시 더 이상 존재하지 않는 키를 삭제해 맵을 깨끗하게 유지
            var toRemove = new List<ReiwHumanBodyBones>();
            foreach (var key in AvatarBoneKeyValueMap.Keys)
            {
                // 키가 유효 enum이 아니거나 제외 대상이면 제거 대상으로 표시
                if (!Enum.IsDefined(typeof(ReiwHumanBodyBones), key) || ShouldSkip(key))
                    toRemove.Add(key);
            }

            foreach (var k in toRemove)
                AvatarBoneKeyValueMap.Remove(k);
        }

        // 편의 기능: 컨텍스트 메뉴에서 값만 초기화(키는 유지)
        [ContextMenu("Clear All Values (Keep Keys)")]
        private void ClearAllValues()
        {
            var keys = new List<ReiwHumanBodyBones>(AvatarBoneKeyValueMap.Keys);
            foreach (var k in keys)
            {
                if (ShouldSkip(k)) continue;
                AvatarBoneKeyValueMap[k] = string.Empty;
            }
        }
#endif
    }
}