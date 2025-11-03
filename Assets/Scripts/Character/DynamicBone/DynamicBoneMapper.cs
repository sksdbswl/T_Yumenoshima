using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace REIW
{
    public class DynamicBoneMapper : CacheMonoBehaviour
    {
        [SerializeField]
        private Transform _targetRoot;
        public Transform TargetRoot
        {
            set
            {
                if (_targetRoot == value)
                    return;
                
                _targetRoot = value;
                SetBoneFollow(_targetRoot);
            }
        }

        private SkinnedMeshRenderer[] _mySkinnedMeshRenderers;
        private List<BoneFollower> _bonefollowers = new List<BoneFollower>();
        
        private void Awake()
        {
            _mySkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        private void Start()
        {
            SetBoneFollow(_targetRoot);
        }

        private void OnDestroy()
        {
            RemoveFollowBones();
        }

        private void SetBoneFollow(Transform target)
        {
            if (target == null)
                return;

            Transform currentRootBone = FindRootBone(MyTransform);
            if (currentRootBone == null)
                return;

            Transform targetRootBone = target.FindAllChild(currentRootBone.name);
            if (targetRootBone == null)
                return;

            Dictionary<string, Transform> map = targetRootBone.GetComponentsInChildren<Transform>(true)
                    .GroupBy(t => t.name)
                    .ToDictionary(g => g.Key, g => g.First());

            RemoveFollowBones();
            
            foreach (var t in MyTransform.GetComponentsInChildren<Transform>(true))
            {
                if (map.ContainsKey(t.name) == false)
                    continue;

                BoneFollower bone = t.gameObject.GetorAddComponent<BoneFollower>();
                bone.Initilize(map[t.name], Vector3.zero, Quaternion.identity, Vector3.one);

                _bonefollowers.Add(bone);
            }
        }

        private void RemoveFollowBones()
        {
            foreach (BoneFollower bone in _bonefollowers)
                Destroy(bone);

            _bonefollowers.Clear();
        }
        
        private Transform FindRootBone(Transform targettrans)
        {
            foreach (Transform t in targettrans)
            {
                if (t.IsEmptyObject())
                    return t;
            }
            
            return null;
        }
    }

    public class BoneFollower : CacheMonoBehaviour
    {
        private Transform _targetTrans;
        
        private Vector3 _localPositionOffset;
        private Quaternion _localRotationOffset;
        private Vector3 _localScaleOffset;

        public void Initilize(Transform target, Vector3 localPositionOffset, Quaternion localRotationOffset, Vector3 localScaleOffset)
        {
            _targetTrans = target;
            _localPositionOffset = localPositionOffset;
            _localRotationOffset = localRotationOffset;
            _localScaleOffset = localScaleOffset;

            Update();
        }

        private void Awake()
        {
            _localPositionOffset = Vector3.zero;
            _localRotationOffset = Quaternion.identity;
            _localScaleOffset = Vector3.one;
        }

        public void Update()
        {
            if (_targetTrans == null)
                return;

            MyTransform.position = _targetTrans.position;
            MyTransform.rotation = _targetTrans.rotation;
            MyTransform.localScale = _targetTrans.lossyScale;
            
            MyTransform.localPosition += _localPositionOffset;
            MyTransform.localRotation *= _localRotationOffset;

            if (_localScaleOffset != Vector3.one)
            {
                Vector3 scale = MyTransform.localScale;
                scale.x = MyTransform.localScale.x * _localScaleOffset.x;
                scale.y = MyTransform.localScale.y * _localScaleOffset.y;
                scale.x = MyTransform.localScale.z * _localScaleOffset.z;
                MyTransform.localScale = scale;
            }
        }
    }
}
