using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

 public class NotifyCategoryTracker : MonoBehaviour
    {
        [Serializable]
        private struct EnumCategoryEntry
        {
            [SerializeField] private ushort value;
#if UNITY_EDITOR
            [SerializeField, ReadOnly] private string preview;
#endif
            public ushort Value => value;

#if UNITY_EDITOR
            public void UpdatePreview()
            {
                var cast = (EnumCategory)value;
                preview = cast.ToString();
            }
#endif
        }

        [Header("Marker to show/hide")]
        [SerializeField] private GameObject notifyMarker;

        [Header("Category to track")]
        [SerializeField] private List<NotifyEntryCategory> notifyEntryCategories = new();
        [SerializeField] private List<EnumCategoryEntry> enumCategories = new();

        private void OnEnable()
        {
            OnNotifyStateChanged();
            NotifyEntryManager.Singleton.OnNotifyStateChanged += OnNotifyStateChanged;
        }

        private void OnDisable()
        {
            if (NotifyEntryManager.Singleton != null)
                NotifyEntryManager.Singleton.OnNotifyStateChanged -= OnNotifyStateChanged;
        }

        private void OnNotifyStateChanged()
        {
            if (notifyMarker == null)
            {
                Debug.LogWarning("NotifyCategoryTracker: notifyMarker is null");
                return;
            }

            // EnumCategory 기반 체크
            foreach (var category in enumCategories)
            {
                if (NotifyEntryManager.Singleton.Contains(category.Value))
                {
                    if (!notifyMarker.activeSelf) notifyMarker.SetActive(true);
                    return;
                }
            }

            // NotifyEntryCategory 기반 체크
            foreach (var category in notifyEntryCategories)
            {
                if (NotifyEntryManager.Singleton.Contains(category))
                {
                    if (!notifyMarker.activeSelf) notifyMarker.SetActive(true);
                    return;
                }
            }

            if (notifyMarker.activeSelf) notifyMarker.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateConvertedPreview();
        }

        private void UpdateConvertedPreview()
        {
            for (int i = 0; i < enumCategories.Count; i++)
            {
                var entry = enumCategories[i];
                entry.UpdatePreview();
                enumCategories[i] = entry;
            }
        }
#endif
    }