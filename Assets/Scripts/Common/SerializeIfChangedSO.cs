using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace REIW
{
    public class SerializeIfChangedSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized] private bool _changedThisCycle;

#if UNITY_EDITOR
        private static readonly HashSet<UnityEngine.Object> Scheduled = new();

        protected new void SetDirty()
        {
            EditorUtility.SetDirty(this);
        }
#endif

        public void OnBeforeSerialize()
        {
            if (!this)
                return;

#if UNITY_EDITOR
            if (!EditorUtility.IsDirty(this))
                return;

            _changedThisCycle = true;

            if (Scheduled.Add(this))
            {
                EditorApplication.delayCall += () =>
                {
                    if (this && Scheduled.Remove(this))
                        RunOnceIfChanged();
                };
            }
#else
            BeforeSerialize();
#endif
        }

        public void OnAfterDeserialize()
        {
            AfterDeserialize();
        }

        protected virtual void BeforeSerialize()
        {
        }

        protected virtual void AfterDeserialize()
        {
        }

        private void RunOnceIfChanged()
        {
            if (!_changedThisCycle)
                return;

            _changedThisCycle = false;
            BeforeSerialize();

#if UNITY_EDITOR
            EditorUtility.ClearDirty(this);
#endif
        }
    }
}
