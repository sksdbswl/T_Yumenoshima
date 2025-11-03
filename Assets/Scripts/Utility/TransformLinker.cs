using System.Collections;
using UnityEngine;

namespace REIW
{
    public class TransformLinker : MonoBehaviour
    {
        [SerializeField] private Transform _parentTarget;
        [SerializeField] private TextAsset _parentTargetNameText;
        [SerializeField] private string _parentTargetName;

        [SerializeField] private Transform _positionTarget;
        [SerializeField] private TextAsset _positionTargetNameText;
        [SerializeField] private string _positionTargetName;

        [SerializeField] private Transform _rotationTarget;
        [SerializeField] private TextAsset _rotationTargetNameText;
        [SerializeField] private string _rotationTargetName;

        [SerializeField] private Transform _scaleTarget;
        [SerializeField] private TextAsset _scaleTargetNameText;
        [SerializeField] private string _scaleTargetName;

        private Transform _originParent;
        private Vector3 _originLocalPosition;
        private Vector3 _originLocalEulerAngles;
        private Vector3 _originLocalScale;

        private void Awake()
        {
            ChangeParent();
        }

        private IEnumerator Start()
        {
            while (!FindTargets())
                yield return null;
        }

        private void LateUpdate()
        {
            if (_positionTarget)
                transform.position = _positionTarget.position;

            if (_rotationTarget)
                transform.rotation = _rotationTarget.rotation;

            if (_scaleTarget)
                transform.localScale = _scaleTarget.localScale;
        }

        private void SetOriginParent()
        {
            if (!_originParent)
            {
                _originParent = transform.parent;
                _originLocalPosition = transform.localPosition;
                _originLocalEulerAngles = transform.localEulerAngles;
                _originLocalScale = transform.localScale;
            }
        }

        private void ChangeParent(Transform InTarget)
        {
            SetOriginParent();

            if (transform.parent != InTarget)
                transform.parent = InTarget;
        }

        public void ChangeParent()
        {
            if (!_parentTarget)
                return;

            ChangeParent(_parentTarget);
        }

        public void RestoreParent()
        {
            SetOriginParent();
            ChangeParent(_originParent);
            transform.localPosition = _originLocalPosition;
            transform.localEulerAngles = _originLocalEulerAngles;
            transform.localScale = _originLocalScale;
        }

        private bool FindTargets()
        {
            _parentTarget = FindTarget(_parentTarget, _parentTargetName);
            if (!_parentTarget && _parentTargetNameText)
                _parentTarget = FindTarget(_parentTarget, _parentTargetNameText.text);

            _positionTarget = FindTarget(_positionTarget, _positionTargetName);
            if (!_positionTarget && _positionTargetNameText)
                _positionTarget = FindTarget(_positionTarget, _positionTargetNameText.text);

            _rotationTarget = FindTarget(_rotationTarget, _rotationTargetName);
            if (!_rotationTarget && _rotationTargetNameText)
                _rotationTarget = FindTarget(_rotationTarget, _rotationTargetNameText.text);

            _scaleTarget = FindTarget(_scaleTarget, _scaleTargetName);
            if (!_scaleTarget && _scaleTargetNameText)
                _scaleTarget = FindTarget(_scaleTarget, _scaleTargetNameText.text);

            return (_parentTarget || !_parentTargetNameText || string.IsNullOrEmpty(_parentTargetNameText.text) || string.IsNullOrEmpty(_parentTargetName)) &&
                   (_positionTarget || !_positionTargetNameText || string.IsNullOrEmpty(_positionTargetNameText.text) || string.IsNullOrEmpty(_positionTargetName)) &&
                   (_rotationTarget || !_rotationTargetNameText || string.IsNullOrEmpty(_rotationTargetNameText.text) || string.IsNullOrEmpty(_rotationTargetName)) &&
                   (_scaleTarget || !_scaleTargetNameText || string.IsNullOrEmpty(_scaleTargetNameText.text) || string.IsNullOrEmpty(_scaleTargetName));
        }

        private Transform FindTarget(Transform InTarget, string InTargetName)
        {
            if (InTarget || string.IsNullOrEmpty(InTargetName))
                return InTarget;

            return (transform.parent ?? transform).FindAllChild(InTargetName);
        }
    }
}
