using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class UIManager : SingletonBase<UIManager>
{
    public bool IsPopupOpened =>
        popups.Values.Any(popup => popup != null && popup.gameObject.activeSelf);

    public bool IsNeedVisibleCursor => Singleton &&
                                       Singleton.activatedUIs.Exists(x =>
                                           x != null && x.gameObject.activeSelf && x.IsNeedVisibleCursor);

    [field: SerializeField] public Camera UICamera { get; private set; } = null;

    /// <summary> 2D Panel UI Container </summary>
    private readonly Dictionary<UIList, UIBase> panels = new Dictionary<UIList, UIBase>();

    /// <summary> 2D Popup UI Container </summary>
    private readonly Dictionary<UIList, UIBase> popups = new Dictionary<UIList, UIBase>();

    [SerializeField] internal Transform panelRoot;
    [SerializeField] internal Transform popupRoot;

    internal readonly List<UIBase> activatedUIs = new List<UIBase>(30);
    internal readonly List<GameObject> cachedUIsForOnlyOneRender = new List<GameObject>();
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

    // ------------------------------------------------
    // Input 연결
    // ------------------------------------------------
    // private void Start()
    // {
    //     if (InputController.Singleton)
    //     {
    //         InputController.Singleton.OnEscape += OnEscapeCallback;
    //         InputController.Singleton.OnTabForNext += OnTabForNext;
    //         InputController.Singleton.OnTabForPrevious += OnTabForPrevious;
    //     }
    // }
    //
    // private void OnDestroy()
    // {
    //     if (InputController.Singleton)
    //     {
    //         InputController.Singleton.OnEscape -= OnEscapeCallback;
    //         InputController.Singleton.OnTabForNext -= OnTabForNext;
    //         InputController.Singleton.OnTabForPrevious -= OnTabForPrevious;
    //     }
    // }

    private void OnEscapeCallback()
    {
        HideLastPopup();
    }

    // private void OnTabForNext()
    // {
    //     MoveTabFocus(false);
    // }
    //
    // private void OnTabForPrevious()
    // {
    //     MoveTabFocus(true);
    // }

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
            // 이 Hide는 다른 partial에서 정의된 static Hide(UIBase) 사용
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

    /// <summary> Tab 입력이 들어왔을 때 호출 (reverse = Shift+Tab) </summary>
    // public void MoveTabFocus(bool reverse)
    // {
    //     var current = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    //
    //     // 1. 현재 선택된 오브젝트 기준으로 처리
    //     if (current != null)
    //     {
    //         var group = current.GetComponentInParent<UIFocusGroup>(true);
    //         if (group != null)
    //         {
    //             group.MoveFocus(reverse);
    //             return;
    //         }
    //     }
    //
    //     // 2. currentSelected == null인 경우
    //     //    → 활성 UI 중 가장 위 UI에서 FocusGroup을 찾아서 첫 포커스 활성화
    //     for (int i = activatedUIs.Count - 1; i >= 0; i--)
    //     {
    //         var ui = activatedUIs[i];
    //         if (ui == null || !ui.gameObject.activeInHierarchy)
    //             continue;
    //
    //         var fg = ui.GetComponentInChildren<UIFocusGroup>(true);
    //         if (fg != null)
    //         {
    //             fg.FocusFirst();
    //             return;
    //         }
    //     }
    //
    //     // 3. FocusGroup이 하나도 없는 경우 → 아무 것도 하지 않음
    // }

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
    public T GetUI<T>(UIList uiName, bool reload = false) where T : UIBase
    {
        // Panel / Popup 컨테이너 & Root 결정
        Dictionary<UIList, UIBase> container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;
        Transform root =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popupRoot : panelRoot;

        if (!container.ContainsKey(uiName))
        {
            return null;
        }

        // reload 요청 시 기존 인스턴스 제거
        if (reload && container[uiName])
        {
            UnityEngine.Object.Destroy(container[uiName].gameObject);
            container[uiName] = null;
        }

        // 아직 로드 안됐으면 Addressables 통해 로드
        if (!container[uiName])
        {
            if (!AssetManager.Singleton.InstantiateUIPrefabSync(uiName, out GameObject loadedUI) || !loadedUI)
                return null;

            loadedUI.transform.SetParent(root, false);
            var component = loadedUI.GetComponent<T>();
            container[uiName] = component;

            if (container[uiName])
                container[uiName].gameObject.SetActive(false);

            // Depth UI 설정
            if (container[uiName].TryGetComponent(out UIBase ui))
            {
                if (ui.IsDepthUI)
                {
                    Canvas canvas = container[uiName].gameObject.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = this.UICamera;
                }
            }

            // child TextMeshProUGUI Font 세팅
            // var texts = container[uiName].gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            // foreach (var text in texts)
            // {
            //     text.font = AssetManager.Singleton.LoadedMainFont;
            // }
        }

        return (T)container[uiName];
    }

    public void ReleaseUI<T>(T instance, UIList uiName) where T : UIBase
    {
        Dictionary<UIList, UIBase> container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;

        container[uiName] = null;
        UnityEngine.Object.Destroy(instance.gameObject);
        // 필요하다면 여기서 Addressables.ReleaseInstance(instance.gameObject) 도 같이 호출 가능
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

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);

                if (string.IsNullOrEmpty(name) || (transform.name == name))
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
                if (string.IsNullOrEmpty(name) || (component.name == name))
                    return component;
            }
        }

        return null;
    }

    // ------------------------------------------------
    // 씬에서 미리 배치된 UI를 Manager에 등록
    // ------------------------------------------------
    public void RegisterToManagerSceneInstancedUI(UIList uiName, UIBase instance)
    {
        Dictionary<UIList, UIBase> container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;

        if (container[uiName] != null)
        {
            UnityEngine.Object.Destroy(instance.gameObject);
            LogUtil.LogWarning(
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

            //InputController.Singleton.CurrentActionMap = InputController.InputActionMapType.UI;
        }
    }

    // ------------------------------------------------
    // 폰트 변경
    // ------------------------------------------------
    private const string _fontNameFixBold = "Bold";
    private const string _fontNameFixDynamic = "Dynamic";

    // public void SetChangeAllUIFont()
    // {
    //     var mainFont = AssetManager.Singleton.LoadedMainFont;
    //
    //     TextMeshProUGUI[] panelTexts = panelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
    //     TextMeshProUGUI[] popupTexts = popupRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
    //
    //     void ApplyFont(TextMeshProUGUI text)
    //     {
    //         if (text.font == null)
    //         {
    //             text.font = mainFont;
    //             return;
    //         }
    //
    //         // 기존 폰트가 Dynamic 폰트가 아니라면, 대응되는 언어의 폰트로 교체
    //         if (!text.font.name.Contains(_fontNameFixDynamic))
    //         {
    //             text.font = mainFont;
    //         }
    //     }
    //
    //     foreach (var text in panelTexts) ApplyFont(text);
    //     foreach (var text in popupTexts) ApplyFont(text);
    // }

    public bool IsRegisteredUI(UIList uiName)
    {
        var container =
            uiName is > UIList.POPUP_START and < UIList.POPUP_MAX ? popups : panels;
        return container.ContainsKey(uiName) && container[uiName] != null;
    }

    // ------------------------------------------------
    // 씬 Flow 의존 UI 정리
    // ------------------------------------------------
    // public async UniTask ReleaseDependencyUI(ISceneFlow prevScene, ISceneFlow nextScene)
    // {
    //     if (prevScene == null)
    //         return;
    //
    //     Type nextSceneType = nextScene?.GetType();
    //     var uiListToRelease = new List<(UIBase ui, UIList uiList)>();
    //
    //     foreach (UIList uiList in Enum.GetValues(typeof(UIList)))
    //     {
    //         if (!UIManager.Singleton.IsRegisteredUI(uiList))
    //             continue;
    //
    //         var ui = UIManager.Singleton.GetUI<UIBase>(uiList);
    //         if (ui == null)
    //             continue;
    //
    //         // 글로벌 UI → DependencySceneFlowTypes 없으면 스킵
    //         var deps = ui.DependencySceneFlowTypes;
    //         if (deps == null || deps.Count == 0)
    //             continue;
    //
    //         // nextScene에서도 사용하는 경우는 유지
    //         if (!deps.Contains(nextSceneType))
    //         {
    //             uiListToRelease.Add((ui, uiList));
    //         }
    //     }
    //
    //     // 실제 릴리즈 처리
    //     foreach (var (ui, uiList) in uiListToRelease)
    //     {
    //         UIManager.Release(ui, uiList);
    //     }
    //
    //     await UniTask.Yield();
    // }
}
