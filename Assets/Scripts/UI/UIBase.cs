using System;
using UnityEngine;
using UnityEngine.Events;

public class UIBase : MonoBehaviour
{
    /// <summary> UI가 켜졌을 때 마우스 커서가 필요한 경우 </summary>
    public virtual bool IsNeedVisibleCursor => false;

    /// <summary> Overlay UI (ScreenSpaceCamera 등) 처리용 </summary>
    public virtual bool IsDepthUI => false;

    public virtual bool IsNeedOnlyOneRenderUI => false;
    
    /// <summary> Hide 애니메이션을 사용하는 경우 완료 시 호출됨 </summary>
    // public event Action<UIBase, UnityAction> OnHideAnimationCompleted;
    //
    // protected void NotifyHideAnimationCompleted(UnityAction callback = null)
    // {
    //     OnHideAnimationCompleted?.Invoke(this, callback);
    // }

    /// <summary>
    /// UI 표시 (필요 시 오버라이드)
    /// </summary>
    public virtual void Show(UnityAction callback = null)
    {
        gameObject.SetActive(true);
        callback?.Invoke();
    }

    /// <summary>
    /// UI 숨김 (애니메이션 있으면 false 반환 → 콜백에서 Notify 호출)
    /// </summary>
    public virtual bool Hide(UnityAction callback = null)
    {
        gameObject.SetActive(false);
        callback?.Invoke();
        return true;
    }

    /// <summary> 현재 UI의 Canvas sorting order 조회 </summary>
    public int GetCanvasSortingOrder()
    {
        var canvas = GetComponent<Canvas>();
        return canvas ? canvas.sortingOrder : 0;
    }

    /// <summary> Canvas sorting order 변경 </summary>
    public void SetCanvasSortingOrder(int sortingOrder)
    {
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = sortingOrder;
    }
}