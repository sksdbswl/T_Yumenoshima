using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public partial class UIManager:SingletonBase<UIManager>
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
        await GameManager.Singleton.EnterIngameAsync();
    }
    
    public static void ShowIngameFieldUI()
    {
    }

    public static void HideIngameFieldUI()
    {
    }

    /// <summary>
    /// 항상 새로운 인스턴스를 생성해서 사용하는 UI (예: 임시 팝업 등)
    /// </summary>
    public static T ShowNewInstanceUI<T>(UIList uiName, UnityAction showCallback = null) where T : UIBase
    {
        // PANEL
        if (uiName is > UIList.PANEL_START and < UIList.PANEL_MAX)
        {
            if (!AssetManager.Singleton.InstantiateUIPrefabSync(uiName, out GameObject loadedUI) || loadedUI == null)
                return null;

            loadedUI.transform.SetParent(Singleton.panelRoot, false);

            if (loadedUI.TryGetComponent(out T result))
            {
                Singleton.activatedUIs.Add(result);

                if (Singleton.IsNeedVisibleCursor)
                {
                    //InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.UI;
                }

                result.Show(showCallback);
                return result;
            }

            UnityEngine.Object.Destroy(loadedUI);
            return null;
        }

        // POPUP
        if (uiName is > UIList.POPUP_START and < UIList.POPUP_MAX)
        {
            if (!AssetManager.Singleton.InstantiateUIPrefabSync(uiName, out GameObject loadedUI) || loadedUI == null)
                return null;

            loadedUI.transform.SetParent(Singleton.popupRoot, false);

            if (loadedUI.TryGetComponent(out T result))
            {
                Singleton.activatedUIs.Add(result);

                if (Singleton.IsNeedVisibleCursor)
                {
                    //InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.UI;
                }

                result.Show(showCallback);
                return result;
            }

            UnityEngine.Object.Destroy(loadedUI);
            return null;
        }

        return null;
    }

    public static void Release<T>(T instance, UIList uiName) where T : UIBase =>
        Singleton.ReleaseUI(instance, uiName);

    public static T Show<T>(UIList ui, UnityAction showCallback = null, bool isReload = false) where T : UIBase
    {
        var newUI = Singleton.GetUI<T>(ui, isReload);
        if (!newUI)
            return null;

        newUI.Show(showCallback);

        if (!Singleton.activatedUIs.Contains(newUI))
        {
            Singleton.activatedUIs.Add(newUI);
        }

        if (Singleton.IsNeedVisibleCursor)
        {
            //InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.UI;
        }

        if (newUI.IsNeedOnlyOneRenderUI)
        {
            HideActivatedUIAndRemember(new GameObject[] { newUI.gameObject });
        }

        return newUI;
    }

    public static T Hide<T>(UIList ui, UnityAction hideCallback = null) where T : UIBase
    {
        var targetUI = Singleton.GetUI<T>(ui);
        if (!targetUI)
            return null;

        Hide(targetUI, hideCallback);
        return targetUI;
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
                capturedTarget.OnHideAnimationCompleted -= Handler;
                FinalizeHide(capturedTarget);
            }

            targetUI.OnHideAnimationCompleted += Handler;
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