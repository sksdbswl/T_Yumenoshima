using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    public class CharacterVisualAttachment : MonoBehaviour
    {
        [SerializeField, ReadOnly] private ReiwHumanBodyBones targetSocket;
        [SerializeField, ReadOnly] private GameObject attachedVisual;
        public GameObject AttachedVisual => attachedVisual;
        
        private CharacterAvatarBoneMapper _avatarBoneMapper;
        private bool isValidate = false;

        private void Awake()
        {
            _avatarBoneMapper = GetComponent<CharacterAvatarBoneMapper>();
            isValidate = _avatarBoneMapper;
            LogUtil.Assert(isValidate);
        }

        
        private Dictionary<ReiwHumanBodyBones, AttachObject> _attachObjects = new Dictionary<ReiwHumanBodyBones, AttachObject>();
        
        public void Attach(ReiwHumanBodyBones socket, ref GameObject model, Vector3? localPosition = null, Quaternion? localRotation = null, Vector3? localScale = null)
        {
            // LogUtil.Assert(isValidate);

            if (attachedVisual)
                Destroy(attachedVisual.gameObject);

            targetSocket = socket;
            Transform targetTransform = _avatarBoneMapper.GetBoneTransform(targetSocket);
            attachedVisual = model;
            attachedVisual.transform.SetParent(targetTransform);
            attachedVisual.transform.SetLocalPositionAndRotation(localPosition ?? Vector3.zero, localRotation ?? Quaternion.identity);
            attachedVisual.transform.localScale = localScale ?? Vector3.one;

            if (_attachObjects.ContainsKey(socket))
            {
                _attachObjects.Remove(socket);
            }
            
            _attachObjects.Add(socket, model.GetComponent<AttachObject>());
        }

        public AttachObject GetAttachObject(ReiwHumanBodyBones socket)
        {
            return _attachObjects[socket];
        }

        public void Detach(ReiwHumanBodyBones socket)
        {
            // LogUtil.Assert(isValidate);

            if (targetSocket == socket)
            {
                targetSocket = ReiwHumanBodyBones.None;
                if (attachedVisual)
                {
                    //AssetManager.Singleton.ReleaseAsset(attachedVisual);
                    
                    // Clone을 로드한 경우 -> Destroy
                    Destroy(attachedVisual);
                    attachedVisual = null;
                }
            }
            
            if (_attachObjects.ContainsKey(socket))
            {
                _attachObjects.Remove(socket);
            }
        }

        public void SetParent(ReiwHumanBodyBones socket)
        {
            LogUtil.Assert(isValidate);

            if (!attachedVisual) return;

            Transform targetTransform = _avatarBoneMapper.GetBoneTransform(socket);
            attachedVisual.transform.SetParent(targetTransform);
            attachedVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public Transform GetAttackBoneTransform(ReiwHumanBodyBones socket) => _avatarBoneMapper.GetBoneTransform(socket);
    }
}