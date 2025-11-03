using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    public class CharacterAvatarBoneMapper : MonoBehaviour
    {
        /// <summary> [string] Naming 기준 해당 Bone Transform 반환 </summary>
        public Transform GetBoneTransform(string boneName) => InternalGetBoneTransform(boneName);
        
        /// <summary> [ReiwHumanBodyBones] 타입 기준 해당 Bone Transform 반환 </summary>
        public Transform GetBoneTransform(ReiwHumanBodyBones boneType) => InternalGetBoneTransform(boneType);
        
        
        [Serializable]
        public struct AvatarBoneData
        {
            [field: SerializeField] public string BoneName { get; private set; }
            [field: SerializeField] public ReiwHumanBodyBones BoneType { get; private set; }
            [field: SerializeField] public Transform BoneTransform { get; private set; }
        }
        
        [field: Header("Avatar Bone Config")]
        [field: SerializeField] public Transform RootTransform { get; private set; }
        [field: SerializeField] public List<AvatarBoneData> AvatarBoneContainer { get; private set; } = new();
        
        [NonSerialized] private Dictionary<ReiwHumanBodyBones, Transform> _mapByType;
        [NonSerialized] private Dictionary<string, Transform> _mapByName;
        
        private Transform GetRoot() => RootTransform != null ? RootTransform : transform;
        private void BuildNameCacheIfNeeded()
        {
            if (_mapByName != null) return;

            var root = GetRoot();
            if (root == null) return;

            _mapByName = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

            var trs = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in trs)
            {
                _mapByName[t.name] = t;
            }
        }

        public Transform InternalGetBoneTransform(string boneName)
        {
            if (string.IsNullOrEmpty(boneName)) return null;

            // 0) 컨테이너에 이미 기록돼 있으면 우선 사용
            Transform exact = null, ignore = null, tail = null;
            for (int i = 0; i < AvatarBoneContainer.Count; i++)
            {
                var e = AvatarBoneContainer[i];
                if (e.BoneTransform == null || string.IsNullOrEmpty(e.BoneName)) continue;

                if (string.Equals(e.BoneName, boneName, StringComparison.Ordinal))
                {
                    exact = e.BoneTransform; break;
                }
                if (ignore == null && string.Equals(e.BoneName, boneName, StringComparison.OrdinalIgnoreCase))
                    ignore = e.BoneTransform;

                if (tail == null && e.BoneName.EndsWith(boneName, StringComparison.OrdinalIgnoreCase))
                    tail = e.BoneTransform;
            }
            if (exact != null) return exact;
            if (ignore != null) return ignore;
            if (tail != null) return tail;

            // 1) 이름 캐시에서 대/소문자 무시 정확 일치
            BuildNameCacheIfNeeded();
            if (_mapByName != null && _mapByName.TryGetValue(boneName, out var tCached))
                return tCached;

            // 2) 'mixamorig:RightHand' 같은 경우, 콜론 뒤 토큰으로 재시도
            int colon = boneName.LastIndexOf(':');
            if (colon >= 0 && colon < boneName.Length - 1)
            {
                var token = boneName.Substring(colon + 1);
                if (_mapByName != null && _mapByName.TryGetValue(token, out var tToken))
                    return tToken;
            }

            // 3) 최종 폴백: 계층 전체 스캔으로 접미 일치(EndsWith) 검색
            var rootTr = GetRoot();
            if (rootTr == null) return null;

            var all = rootTr.GetComponentsInChildren<Transform>(true);

            // 3-1) 먼저 정확(대/소문자 무시) 재확인
            foreach (var tr in all)
                if (string.Equals(tr.name, boneName, StringComparison.OrdinalIgnoreCase))
                    return tr;

            // 3-2) 접미 일치
            foreach (var tr in all)
                if (tr.name.EndsWith(boneName, StringComparison.OrdinalIgnoreCase))
                    return tr;

            // 3-3) 콜론 토큰으로 접미 일치
            if (colon >= 0 && colon < boneName.Length - 1)
            {
                var token = boneName.Substring(colon + 1);
                foreach (var tr in all)
                    if (tr.name.EndsWith(token, StringComparison.OrdinalIgnoreCase))
                        return tr;
            }

            return null;
        }

        public Transform InternalGetBoneTransform(ReiwHumanBodyBones boneType)
        {
            if (boneType == ReiwHumanBodyBones.None) return null;
            BuildCacheIfNeeded();
            return _mapByType.TryGetValue(boneType, out var t) ? t : null;
        }

        private void BuildCacheIfNeeded()
        {
            if (_mapByType != null) return;

            _mapByType = new Dictionary<ReiwHumanBodyBones, Transform>(128);
            if (AvatarBoneContainer == null) return;

            _mapByType[ReiwHumanBodyBones.Root] = this.transform;
            foreach (var e in AvatarBoneContainer)
            {
                if (e.BoneTransform == null) continue;
                if (e.BoneType == ReiwHumanBodyBones.None) continue;

                // 덮어쓰기(마지막 값 우선)
                _mapByType[e.BoneType] = e.BoneTransform;
            }
        }

        public void RebuildCache()
        {
            _mapByType = null;
            _mapByName = null;   // 이름 캐시도 함께 초기화
            BuildCacheIfNeeded();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 인스펙터 값 변경 시 캐시 재생성
            RebuildCache();
        }
#endif
    }
}
