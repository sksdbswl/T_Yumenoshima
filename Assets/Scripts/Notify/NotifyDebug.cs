using UnityEngine;

public class NotifyDebug : MonoBehaviour
{
    private void Start()
    {
        // Item 카테고리(enum 1)에 ID 1001짜리 알림 추가
        NotifyEntryManager.Singleton.AddNotify((ushort)EnumCategory.Item, 1001);

        // 2초 후 제거 테스트
        //Invoke(nameof(Remove), 2f);
    }

    private void Remove()
    {
        NotifyEntryManager.Singleton.RemoveNotify((ushort)EnumCategory.Item, 1001);
    }
}