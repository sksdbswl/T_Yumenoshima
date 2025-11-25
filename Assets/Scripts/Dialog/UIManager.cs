using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public partial class UIManager : SingletonBase<UIManager>
{
    void Awake()
    {
        base.Awake();
        PlacementSaveManager.Singleton.Load();
        Initialize();
    }

    public void OnPlacementSave()
    {
        PlacementSaveManager.Singleton.Save();
    }

    public void OnPlacementReset()
    {
        PlacementSaveManager.Singleton.ClearAll();

        var objects = FindObjectsOfType<PlaceableObject>();
        foreach (var obj in objects)
            Destroy(obj.gameObject);

        PlacementSystem placement = FindObjectOfType<PlacementSystem>();
        placement.RebuildFromSave(PlacementSaveManager.Singleton.PlacedObjects);
    }

    public void OnPlacementReload()
    {
        PlacementSaveManager.Singleton.Load();
    }

    public async void OnClickGameStart()
    {
        // 버튼에서 직접 호출하고 싶다면 여기를 사용
        // await GameManager.Singleton.EnterIngameAsync();
    }

    public static void ShowIngameFieldUI()
    {
    }

    public static void HideIngameFieldUI()
    {
    }

    public static void Release<T>(T instance, UIList uiName) where T : UIBase =>
        Singleton.ReleaseUI(instance, uiName);

    /// <summary>
    /// UI 표시 (Addressables 로딩 + 캐싱 후 Show 호출)
    /// </summary>
    public static async UniTask<T> Show<T>(UIList ui, UnityAction showCallback = null, bool isReload = false)
        where T : UIBase
    {
        // GetUI 가 Addressables 로부터 로드 + Dictionary 캐싱까지 처리
        var newUI = await Singleton.GetUI<T>(ui, isReload);
        if (!newUI)
            return null;

        newUI.Show(showCallback);

        if (!Singleton.activatedUIs.Contains(newUI))
        {
            Singleton.activatedUIs.Add(newUI);
        }

        if (Singleton.IsNeedVisibleCursor)
        {
            // InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.UI;
        }

        if (newUI.IsNeedOnlyOneRenderUI)
        {
            HideActivatedUIAndRemember(new GameObject[] { newUI.gameObject });
        }

        return newUI;
    }

    /// <summary>
    /// 이미 로드되어 캐싱된 UI를 숨김. (Addressables 새 로드는 하지 않음)
    /// </summary>
    public static T Hide<T>(UIList ui, UnityAction hideCallback = null) where T : UIBase
    {
        // Panel / Popup 컨테이너 선택
        var container =
            ui is > UIList.POPUP_START and < UIList.POPUP_MAX
                ? Singleton.popups
                : Singleton.panels;

        if (!container.TryGetValue(ui, out var uiBase) || uiBase == null)
            return null;

        if (uiBase is not T targetTyped)
            return null;

        Hide(targetTyped, hideCallback);
        return targetTyped;
    }

    private static void Hide(UIBase targetUI, UnityAction hideCallback = null)
    {
        var isSuccess = targetUI.Hide(hideCallback);
        if (isSuccess)
        {
            FinalizeHide(targetUI);
        }
        else
        {
            var capturedTarget = targetUI;

            void Handler(UIBase _, UnityAction cb)
            {
                // 애니메이션용 이벤트를 쓰고 싶으면 이 부분을 열어서 사용
                // capturedTarget.OnHideAnimationCompleted -= Handler;
                FinalizeHide(capturedTarget);
            }

            // targetUI.OnHideAnimationCompleted += Handler;
        }
    }

    private static void FinalizeHide(UIBase targetUI)
    {
        Singleton.activatedUIs.Remove(targetUI);

        // if (!Singleton.IsNeedVisibleCursor)
        //     InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.Player;

        // if (targetUI.IsNeedOnlyOneRenderUI)
        // {
        //     ShowCachedActivatedUI();
        //     Singleton.cachedUIsForOnlyOneRender.Clear();
        // }
    }

    public static void HideActivatedUIAndRemember(GameObject[] skipObjects = null)
    {
        Singleton.cachedUIsForOnlyOneRender.Clear();
        foreach (Transform child in Singleton.popupRoot.transform)
        {
            if (skipObjects != null && skipObjects.Contains(child.gameObject))
                continue;

            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                Singleton.cachedUIsForOnlyOneRender.Add(child.gameObject);
            }
        }

        foreach (Transform child in Singleton.panelRoot.transform)
        {
            if (skipObjects != null && skipObjects.Contains(child.gameObject))
                continue;

            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                Singleton.cachedUIsForOnlyOneRender.Add(child.gameObject);
            }
        }
    }

    public static void ShowCachedActivatedUI()
    {
        Singleton.cachedUIsForOnlyOneRender.ForEach(go => go.SetActive(true));
    }
}
