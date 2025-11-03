using UnityEngine;
using System.Collections.Generic;

namespace REIW
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class DynamicSkinnedMeshRebinder : DynamicSkinnedMeshBase
    {
        [SerializeField]
        private SkinnedMeshRenderer _targetSkinnedMeshRenderer = null;

        private SkinnedMeshRenderer _mySkinnedMeshRenderer = null;
        [SerializeField]
        private SaveInfo _defaultsaveInfo;
        [SerializeField]
        private SaveInfo _lastsaveInfo;
        public CharacterPartsController.PartsType CharacterPartsType { set; get; }
        private Transform _parentTrans;
        private CharacterPartsController _characterPartsController;
        private List<Transform> _newBones = new List<Transform>();

        private void Awake()
        {
            ResetMySkinnedMeshRenderer();
        }
        
        private void DestroyNewBones()
        {
            foreach (Transform trans in _newBones)
            {
                if (trans == null)
                    continue;

                if (_characterPartsController?.RemoveCacheTransform(trans) ?? false)
                    continue;
                
                DestroyImmediate(trans.gameObject);
            }

            _newBones.Clear();
        }

        private void ResetMySkinnedMeshRenderer()
        {
            if (_mySkinnedMeshRenderer != null)
                return;

            _mySkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            _defaultsaveInfo.Save(_mySkinnedMeshRenderer);
        }

        [ContextMenu("Restore")]
        public override void Restore()
        {
            base.Restore();
            _defaultsaveInfo.Restore();
            DestroyNewBones();
        }

        public void RestoreLast()
        {
            _lastsaveInfo.Restore();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Restore();
        }

        public void Initialize(CharacterPartsController controller, Transform parentTrans, SkinnedMeshRenderer src, CharacterPartsController.PartsType type)
        {
            ResetMySkinnedMeshRenderer();
            DestroyNewBones();
            
            _characterPartsController = controller;
            _lastsaveInfo.Save(_mySkinnedMeshRenderer);
            _targetSkinnedMeshRenderer = src;
            _parentTrans = parentTrans;

            int oldcount = _characterPartsController.NewBonesCount;
            SetSkinnedMeshRebind(_parentTrans, _targetSkinnedMeshRenderer);
            if (oldcount != _characterPartsController.NewBonesCount)
                AnimatorRebind();
        }

        private void AnimatorRebind()
        {
            Animator animator = _parentTrans.GetComponentInChildren<Animator>(true);
            if (animator == null)
                return;

            animator.Rebind();
        }

        private void FindRootTrans(SkinnedMeshRenderer src, out Transform srcRootTrans, out Transform srcParentTrans)
        {
            Animator srcAnimator = src.GetComponentInParent<Animator>(true);
            srcParentTrans = srcAnimator?.transform ?? src.transform.parent;
            foreach (Transform childtrans in srcParentTrans)
            {
                if (childtrans.IsEmptyObject())
                {
                    srcRootTrans = childtrans;
                    return;
                }
            }

            srcRootTrans = null;
        }

        private void SetSkinnedMeshRebind(Transform parentTrans, SkinnedMeshRenderer src)
        {
            if (_mySkinnedMeshRenderer == null || src == null)
                return;

            FindRootTrans(src, out Transform srcRootTrans, out Transform srcParentTrans);

            Transform newroot = parentTrans.FindAllChild(srcRootTrans.name);
            if (newroot == null)
            {
                newroot = _characterPartsController.GetOrCreateRootTransform(parentTrans, srcRootTrans, srcParentTrans, (x) =>
                {
                    List<Transform> list = x.FindListAllChild();
                    foreach (Transform trans in list)
                    {
                        _newBones.Add(trans);
                        Debug.LogError("Create : " + trans.name);
                    }
                    
                });
            }

            var srcBones = src.bones;
            var newBones = _characterPartsController.GetNewBones(srcBones, newroot, (x) =>
            {
                List<Transform> list = x.FindListAllChild();
                foreach (Transform trans in list)
                {
                    _newBones.Add(trans);
                    Debug.LogError($"Create : {trans.name}, id :{trans.GetInstanceID()}");
                }
            });

            if (src.sharedMesh != null && src.sharedMesh.bindposeCount != newBones.Length)
            {
                Debug.LogError($"bindposes({src.sharedMesh.bindposeCount}) != bones({newBones.Length})");
                return;
            }

            bool issameSharedMesh = ReferenceEquals(_mySkinnedMeshRenderer.sharedMesh, src.sharedMesh);
            _mySkinnedMeshRenderer.enabled = false;
            if (issameSharedMesh == false)
                _mySkinnedMeshRenderer.sharedMesh = null;

            _mySkinnedMeshRenderer.rootBone = _characterPartsController.FindMatchingTransform(src.rootBone, newroot);
            _mySkinnedMeshRenderer.bones = newBones;

            if (issameSharedMesh == false)
                _mySkinnedMeshRenderer.sharedMesh = src.sharedMesh;

            _mySkinnedMeshRenderer.sharedMaterials = src.sharedMaterials;

            Matrix4x4 m = GetSkinnedMeshRendererTrans(_mySkinnedMeshRenderer).worldToLocalMatrix * GetSkinnedMeshRendererTrans(src).localToWorldMatrix;
            _mySkinnedMeshRenderer.localBounds = TransformBounds(src.localBounds, m);
            _mySkinnedMeshRenderer.enabled = true;

            Transform GetSkinnedMeshRendererTrans(SkinnedMeshRenderer smr) => smr.rootBone ? smr.rootBone : smr.transform;

#if JIN_TEST
            bool checkduplicate = false;

            Dictionary<string, List<Transform>> map = _characterPartsController.CacheTransformDictionary;
            foreach (var t in map.Keys)
            {
                if (map[t].Count <= 1)
                    continue;

                checkduplicate = true;
                Debugging.LogGreen($"prefabsname : {parentTrans.name}, target : {this.name} - t : {t}, count : {map[t].Count}");

                foreach (Transform child in map[t])
                {
                    System.Text.StringBuilder debugpath = new System.Text.StringBuilder();

                    Transform loopchild = child;
                    while (loopchild != parentTrans && loopchild != null)
                    {
                        if (debugpath.Length > 0) debugpath.Insert(0, "/");
                        debugpath.Insert(0, loopchild.name);
                        loopchild = loopchild.parent;
                    }

                    if (loopchild == parentTrans)
                    {
                        if (debugpath.Length > 0) debugpath.Insert(0, "/");
                        debugpath.Insert(0, parentTrans.name);
                    }

                    Debugging.LogGreen($"debugpath : {debugpath.ToString()}");
                }
            }

            if (checkduplicate)
                Debugging.LogGreen($"");
#endif
            void DecomposeTRS(Matrix4x4 m, out Vector3 pos, out Quaternion rot, out Vector3 scale)
            {
                pos = m.GetColumn(3);
                var x = new Vector3(m.m00, m.m01, m.m02);
                var y = new Vector3(m.m10, m.m11, m.m12);
                var z = new Vector3(m.m20, m.m21, m.m22);
                scale = new Vector3(x.magnitude, y.magnitude, z.magnitude);
                if (scale.x != 0) x /= scale.x;
                if (scale.y != 0) y /= scale.y;
                if (scale.z != 0) z /= scale.z;
                rot = Quaternion.LookRotation(z, y);
            }
            
            Bounds TransformBounds(Bounds b, Matrix4x4 m)
            {
                // extents는 회전/스케일만
                Matrix4x4 mRS = m;
                mRS.SetColumn(3, new Vector4(0, 0, 0, 1));

                Vector3 e = b.extents;
                Vector3 ax = mRS.MultiplyVector(new Vector3(e.x, 0, 0));
                Vector3 ay = mRS.MultiplyVector(new Vector3(0, e.y, 0));
                Vector3 az = mRS.MultiplyVector(new Vector3(0, 0, e.z));

                Vector3 newExt = new(
                    Mathf.Abs(ax.x) + Mathf.Abs(ay.x) + Mathf.Abs(az.x),
                    Mathf.Abs(ax.y) + Mathf.Abs(ay.y) + Mathf.Abs(az.y),
                    Mathf.Abs(ax.z) + Mathf.Abs(ay.z) + Mathf.Abs(az.z)
                );
                return new Bounds(b.center, newExt * 2f);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("OnDynamicAction")]
        private void OnDynamicAction()
        {
            SetSkinnedMeshRebind(_parentTrans, _targetSkinnedMeshRenderer);
        }
#endif
    }
}
