using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

 // 게임에서 쓰는 실제 EnumCategory 대신, 더미용 간단 버전
public enum EnumCategory : ushort
{
    None = 0,
    Item = 1,
    Mail = 2,
    Achievement = 3,
}

public class NotifyEntry
{
    public ushort Category;
    public ulong ID; // DatabaseID or ContentID
    public long ObtainedTicks;

    public static NotifyEntry Create(ushort category, ulong id) => new NotifyEntry
    {
        Category = category,
        ID = id,
        ObtainedTicks = DateTime.UtcNow.Ticks
    };

    public void Touch() => ObtainedTicks = DateTime.UtcNow.Ticks;
}

public enum NotifyEntryCategory
{
    None,
    Item,
    Mail,
    Achievement,

    Account_Start = 1000,
    AccountMail,
    Account_End
}

/// <summary>
/// 네트워크/파일 저장 없는 더미 NotifyEntryManager
/// </summary>
public class NotifyEntryManager : SingletonBase<NotifyEntryManager>
{
    private NotifyEntryManager() { }

    // 카테고리별 데이터
    private class CategoryData
    {
        public readonly Dictionary<ulong /* ID */, NotifyEntry> EntryMap = new();
        private readonly Dictionary<ushort /* EnumCategory */, int /* Count */> enumCategoryCountMap = new();

        public void AddEntry(NotifyEntry entry)
        {
            EntryMap[entry.ID] = entry;
            if (enumCategoryCountMap.TryGetValue(entry.Category, out var count))
                enumCategoryCountMap[entry.Category] = count + 1;
            else
                enumCategoryCountMap[entry.Category] = 1;
        }

        public bool RemoveEntry(ulong id)
        {
            if (EntryMap.Remove(id, out var entry))
            {
                if (enumCategoryCountMap.TryGetValue(entry.Category, out var count))
                    enumCategoryCountMap[entry.Category] = Mathf.Max(0, count - 1);
                return true;
            }
            return false;
        }

        public void ClearEntries()
        {
            EntryMap.Clear();
            enumCategoryCountMap.Clear();
        }

        public bool ContainsEnumCategory(ushort enumCategory)
        {
            return enumCategoryCountMap.TryGetValue(enumCategory, out var count) && count > 0;
        }
    }

    private readonly Dictionary<NotifyEntryCategory, CategoryData> _categories = new();

    public event Action<ushort /*EnumCategory*/, ulong /*DatabaseId*/> OnNotifyAdded;
    public event Action<ushort /*EnumCategory*/, ulong /*DatabaseId*/> OnNotifyRemoved;
    public event Action OnNotifyStateChanged;

    // ------------------------------------------------------------
    // 내부 유틸
    // ------------------------------------------------------------

    private CategoryData GetCategoryData(NotifyEntryCategory notifyEntryCategory)
    {
        if (!_categories.TryGetValue(notifyEntryCategory, out var data))
        {
            data = new CategoryData();
            _categories[notifyEntryCategory] = data;
        }
        return data;
    }

    // 원래는 EnumCategory 범위로 매핑했는데, 더미에선 단순 매핑
    private bool TryMapEnumCategory(ushort input, out NotifyEntryCategory result)
    {
        var cat = (EnumCategory)input;
        result = cat switch
        {
            EnumCategory.Item        => NotifyEntryCategory.Item,
            EnumCategory.Mail        => NotifyEntryCategory.Mail,
            EnumCategory.Achievement => NotifyEntryCategory.Achievement,
            _                        => NotifyEntryCategory.None
        };
        return result != NotifyEntryCategory.None;
    }

    // ------------------------------------------------------------
    // Public API - Add / Remove
    // ------------------------------------------------------------

    public void AddNotify(ushort enumCategory, ulong databaseId)
    {
        if (!TryMapEnumCategory(enumCategory, out var notifyEntryCategory))
        {
            Debug.LogWarning($"[NotifyEntryManager] Unsupported EnumCategory {enumCategory}");
            return;
        }

        var categoryData = GetCategoryData(notifyEntryCategory);
        if (categoryData.EntryMap.TryGetValue(databaseId, out var notifyEntry))
        {
            notifyEntry.Touch();
            return;
        }

        categoryData.AddEntry(NotifyEntry.Create(enumCategory, databaseId));

        OnNotifyAdded?.Invoke(enumCategory, databaseId);
        OnNotifyStateChanged?.Invoke();
    }

    public bool RemoveNotify(ushort enumCategory, ulong databaseId)
    {
        if (!TryMapEnumCategory(enumCategory, out var notifyEntryCategory)) return false;
        if (!_categories.TryGetValue(notifyEntryCategory, out var categoryData)) return false;
        if (!categoryData.RemoveEntry(databaseId)) return false;

        OnNotifyRemoved?.Invoke(enumCategory, databaseId);
        OnNotifyStateChanged?.Invoke();
        return true;
    }

    public bool RemoveNotify(ulong databaseId)
    {
        foreach (var kvp in _categories)
        {
            if (kvp.Value.RemoveEntry(databaseId))
            {
                // enumCategory 정보는 Entry에 있었는데 여기선 알 수 없으니 이벤트는 생략하거나,
                // 필요하면 CategoryData.RemoveEntry에서 out entry 하도록 구조를 바꿔도 됨
                OnNotifyStateChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    // ------------------------------------------------------------
    // Public API - Query
    // ------------------------------------------------------------

    public bool Contains(ushort enumCategory, ulong databaseId)
    {
        if (!TryMapEnumCategory(enumCategory, out var notifyEntryCategory)) return false;
        return _categories.TryGetValue(notifyEntryCategory, out var categoryData)
               && categoryData.EntryMap.ContainsKey(databaseId);
    }

    public bool Contains(ushort enumCategory)
    {
        if (!TryMapEnumCategory(enumCategory, out var notifyEntryCategory)) return false;
        if (!_categories.TryGetValue(notifyEntryCategory, out var categoryData)) return false;
        return categoryData.ContainsEnumCategory(enumCategory);
    }

    public bool Contains(NotifyEntryCategory notifyEntryCategory)
    {
        if (!_categories.TryGetValue(notifyEntryCategory, out var categoryData)) return false;
        return categoryData.EntryMap.Count > 0;
    }

    public bool TryGetEntries(ushort enumCategory, out List<NotifyEntry> entries)
    {
        entries = null;
        if (!TryMapEnumCategory(enumCategory, out var notifyEntryCategory)) return false;
        if (!_categories.TryGetValue(notifyEntryCategory, out var categoryData)) return false;

        entries = categoryData.EntryMap.Values.Where(e => e.Category == enumCategory).ToList();
        return true;
    }

    public bool TryGetEntries(NotifyEntryCategory notifyEntryCategory, out List<NotifyEntry> entries)
    {
        entries = null;
        if (!_categories.TryGetValue(notifyEntryCategory, out var categoryData)) return false;
        entries = categoryData.EntryMap.Values.ToList();
        return true;
    }
}