using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class UIManager
{
    public bool IsPopupOpened =>
        popups.Values.Any(popup => popup != null && popup.gameObject.activeSelf);

    public bool IsNeedVisibleCursor => Singleton &&
                                       Singleton.activatedUIs.Exists(x =>
                                           x != null && x.gameObject.activeSelf && x.IsNeedVisibleCursor);

    [field: SerializeField] public Camera UICamera { get; private set; } = null;

    /// <summary> 2D Panel UI Container </summary>
    private readonly Dictionary<UIList, UIBase> panels = new();

    /// <summary> 2D Popup UI Container </summary>
    private readonly Dictionary<UIList, UIBase> popups = new();

    [SerializeField] internal Transform panelRoot;
    [SerializeField] internal Transform popupRoot;

    internal readonly List<UIBase> activatedUIs = new(30);
    internal readonly List<GameObject> cachedUIsForOnlyOneRender = new();
    public bool IsInitialized { get; private set; } = false;

    public static event Action OnEscapeWithNoMorePopups;

    /// <summary> 씬 전환 또는 특수한 상황에서 자동으로 닫히면 안되는 케이스의 UI들을 등재할 것 </summary>
    private readonly List<UIList> autoHideExceptList = new()
    {
        // UIList.LoadingUI,
        // UIList.ConsoleUI,
    };

    /// <summary> Except UI List, for last Popup Hide Event Calculation </summary>
    private readonly List<UIList> popupCalculateExceptList = new()
    {
    };

    private void OnEscapeCallback()
    {
        HideLastPopup();
    }

    // ------------------------------------------------
    // ESC 로 맨 위 팝업 닫기
    // ------------------------------------------------
    private void HideLastPopup()
    {
        if (!IsInitialized) return;

        // popupRoot 아래의 "활성" Canvas 들 중, 제외 리스트가 아닌 팝업만 후보로 모음
        var candidates = popupRoot.GetComponentsInChildren<Canvas>(false)
            .Select(c => new { canvas = c, ui = c.GetComponentInParent<UIBase>(true) })
            .Where(x => x.ui != null && !IsExcludedPopup(x.ui))
            // 동일 팝업(UIBase)이 여러 Canvas를 갖는 경우를 대비해서 UI 단위로 그룹핑 후
            // 각 UI에서 가장 높은 sortingOrder를 대표로 사용
            .GroupBy(x => x.ui)
            .Select(g => g.OrderByDescending(x => x.canvas.sortingOrder).First())
            .ToList();

        if (candidates.Count == 0)
        {
            // 제외 대상들을 걷어내고 나서 '보이는 팝업'이 더 이상 없을 때만 이벤트 발생
            OnEscapeWithNoMorePopups?.Invoke();
            return;
        }

        // 제외되지 않은 팝업들 중 sortingOrder가 가장 높은(가장 위에 그려진) 팝업을 닫음
        var last = candidates.OrderByDescending(x => x.canvas.sortingOrder).FirstOrDefault();
        if (last?.ui != null)
        {
            Hide(last.ui);
        }
    }

    // popupCalculateExceptList에 등록된 UIList와 매핑된 실제 인스턴스인지 확인
    private bool IsExcludedPopup(UIBase ui)
    {
        foreach (var except in popupCalculateExceptList)
        {
            if (popups.TryGetValue(except, out var inst) && inst != null && ReferenceEquals(inst, ui))
                return true;
        }

        return false;
    }

    // ------------------------------------------------
    // 초기화 / Root & Camera 생성
    // ------------------------------------------------
    public void Initialize()
    {
        if (IsInitialized)
            return;

        CreateUICamera();
        CreatePanelRoot();
        CreatePopupRoot();

        popups.Clear();
        for (UIList index = UIList.POPUP_START + 1; index < UIList.POPUP_MAX; ++index)
        {
            popups.Add(index, null);
        }

        panels.Clear();
        for (int index = (int)UIList.PANEL_START + 1; index < (int)UIList.PANEL_MAX; index++)
        {
            panels.Add((UIList)index, null);
        }

        IsInitialized = true;

        // 인게임 진입을 여기서 바로 시작하고 싶다면:
        _ = GameManager.Singleton.EnterIngameAsync();
    }

    private void CreatePopupRoot()
    {
        if (!popupRoot)
        {
            GameObject goPopupRoot = new GameObject("Popup Root");
            popupRoot = goPopupRoot.transform;
            popupRoot.parent = transform;
            popupRoot.localPosition = Vector3.zero;
            popupRoot.localRotation = Quaternion.identity;
            popupRoot.localScale = Vector3.one;
        }
    }

    private void CreatePanelRoot()
    {
        if (!panelRoot)
        {
            GameObject goPanelRoot = new GameObject("Panel Root");
            panelRoot = goPanelRoot.transform;
            panelRoot.parent = transform;
            panelRoot.localPosition = Vector3.zero;
            panelRoot.localRotation = Quaternion.identity;
            panelRoot.localScale = Vector3.one;
        }
    }

    private void CreateUICamera()
    {
        if (!UICamera)
        {
            GameObject newUICameraGo = new GameObject("UI Camera");
            newUICameraGo.transform.SetParent(transform);
            UICamera = newUICameraGo.AddComponent<Camera>();
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.cullingMask = LayerMask.GetMask("UI");
            UICamera.fieldOfView = 60;
            UICamera.nearClipPlane = 0.3f;
            UICamera.farClipPlane = 1000;
            UICamera.orthographic = false;
            UICamera.useOcclusionCulling = false;
            UICamera.depthTextureMode = DepthTextureMode.None;

            var uiCameraUacd = newUICameraGo.AddComponent<UniversalAdditionalCameraData>();
            uiCameraUacd.renderType = CameraRenderType.Overlay;
            uiCameraUacd.renderShadows = false;
        }
    }

    // ------------------------------------------------
    // UI 로드 / 캐싱 (Addressables → AssetManager 사용)
    // ------------------------------------------------
    public async UniTask<T> GetUI<T>(UIList uiName, bool reload = false) where T : UIBase
    {
        // Panel / Popup 컨테이너 & Root 결정
        Dictionary<UIList, UIBase> container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;
        Transform root =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popupRoot : panelRoot;

        // reload 처리: 기존 인스턴스를 파괴하고 다시 로드
        if (reload && container.TryGetValue(uiName, out var oldUi) && oldUi)
        {
            UnityEngine.Object.Destroy(oldUi.gameObject);
            container[uiName] = null;
        }

        // 아직 로드 안됐으면 Addressables 통해 로드
        if (!container.TryGetValue(uiName, out var uiBase) || uiBase == null)
        {
            var loadedUI = await AssetManager.Singleton.InstantiateUIPrefabAsync(uiName, root);
            if (loadedUI == null)
                return null;

            var component = loadedUI.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[{nameof(UIManager)}] {uiName} 프리팹에 {typeof(T).Name}가 없습니다.");
                UnityEngine.Object.Destroy(loadedUI);
                return null;
            }

            container[uiName] = component;
            uiBase = component;

            uiBase.gameObject.SetActive(false);

            // Depth UI 설정
            if (uiBase.IsDepthUI)
            {
                var canvas = uiBase.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = this.UICamera;
                }
            }
        }

        return (T)uiBase;
    }

    public void ReleaseUI<T>(T instance, UIList uiName) where T : UIBase
    {
        Dictionary<UIList, UIBase> container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;

        container[uiName] = null;
        UnityEngine.Object.Destroy(instance.gameObject);
        // 필요하다면 Addressables.ReleaseInstance(instance.gameObject) 도 여기서 호출 가능
    }

    public void HideAllUI()
    {
        HideAllPopup();
        HideAllPanel();
    }

    /// <summary> Hide All 2D Popup UI </summary>
    public void HideAllPopup()
    {
        foreach (var popup in popups)
        {
            if (autoHideExceptList.Contains(popup.Key))
                continue;

            if (popup.Value)
            {
                popup.Value.Hide();
            }
        }
    }

    /// <summary> Hide All 2D Panel </summary>
    public void HideAllPanel()
    {
        foreach (var panel in panels)
        {
            if (autoHideExceptList.Contains(panel.Key))
                continue;

            if (panel.Value)
            {
                panel.Value.Hide();
            }
        }
    }

    // ------------------------------------------------
    // 유틸: FindChild
    // ------------------------------------------------
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);

        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false)
        where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (!recursive)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);

                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();

                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public void RegisterToManagerSceneInstancedUI(UIList uiName, UIBase instance)
    {
        Dictionary<UIList, UIBase> container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;

        if (container[uiName] != null)
        {
            UnityEngine.Object.Destroy(instance.gameObject);
            Debug.LogWarning(
                $"Already Registered UI Instance. Received Instance will be Auto Destroy Object. UIList:{uiName}, Type:{instance.GetType()}");
            return;
        }

        container[uiName] = instance;

        Transform root =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popupRoot : panelRoot;
        instance.transform.SetParent(root);

        if (instance.gameObject.activeSelf && instance.IsNeedVisibleCursor)
        {
            if (!cachedUIsForOnlyOneRender.Contains(instance.gameObject))
            {
                cachedUIsForOnlyOneRender.Add(instance.gameObject);
            }

            if (!activatedUIs.Contains(instance))
            {
                activatedUIs.Add(instance);
            }

            // InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.UI;
        }
    }

    public bool IsRegisteredUI(UIList uiName)
    {
        var container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;
        return container.ContainsKey(uiName) && container[uiName] != null;
    }
}
