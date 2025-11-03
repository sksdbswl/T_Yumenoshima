// using System;
// using REIW.Network;
// using UnityEngine;
//
// namespace REIW
// {
//     public partial class Main
//     {
//         public bool IsInitialized => isInitialized;
//         private bool isInitialized = false;
//
//         private ISceneFlow currentSceneFlow;
//         private static bool isInitializedCoreSystems = false;
//
//         public void Initialize(System.Action onSystemLoadCompleted = null)
//         {
//             if (isInitialized)
//                 return;
//             
// #if REIW_QA
//             SRDebug.Init();
// #endif
//             
//             Application.runInBackground = true;
//
//             // TODO : Custom System Initialize
//             UIManager.Singleton.Initialize();
//             AssetManager.OnBasicSystemAssetLoaded += InitializeBasicSystems;
//
//             // Load EventSystem Prefabs
//             GameObject cloneEventSystem = Instantiate((Resources.Load<GameObject>("Systems/REIW.EventSystem")));
//             DontDestroyOnLoad(cloneEventSystem);
//
//             #region [Obsolete] BootStrapper 전용 함수 // @Voyager_Kim Jaejun
// #if UNITY_EDITOR // 아래 define 코드는 [Editor 환경 + BootStrapper 활성화] 상황에만 들어갑니다.
//             // if (BootStrapper.IsActivateBootStrapper && false == UnityEditor.SceneManagement.EditorSceneManager
//             //         .GetActiveScene().name.Equals("REIW.Main"))
//             // {
//             //     bool isSuccess = AssetManager.Singleton.InitializeForEditor();
//             //     if (!isSuccess)
//             //     {
//             //         LogUtil.LogError("AssetManager Initialize Failed. Maybe Addressable not Fastest Mode ??");
//             //         return;
//             //     }
//             //
//             //     Debug.Log("Main InitializeBasicSystems");
//             //
//             //     InitializeBasicSystems();
//             //     UIManager.Hide<ConsoleUI>(UIList.ConsoleUI);
//             //     var versionProfileUI = UIManager.Show<VersionProfileUI>(UIList.VersionProfileUI);
//             //     versionProfileUI.BuildProfile = AssetManager.Singleton.GetPlayerBuildVersionProfile();
//             //     versionProfileUI.BuildProfileSoAddressableProfile = AssetManager.Singleton.GetAddressableVersionProfile();
//             // }
// #endif
//             #endregion
//
//             // option load
//             OptionManager.Singleton.LoadOption();
//
//             // Instance Network Test
//             ReNetworkClient.Singleton.Initialize();
//
//             // Localization System Initialize
//             LocalizationManager.Singleton.Initialize();
//             
//             // Notify System Initialize
//             NotifyEntryManager.Singleton.Initialize();
//             
//             // Initialize Complete
//             isInitialized = true;
//             onSystemLoadCompleted?.Invoke();
//         }
//
//         public static void InitializeBasicSystems()
//         {
//             if (isInitializedCoreSystems)
//                 return;
//
//             InputController.Singleton.Initialize();
//             SpriteAtlasLazyLoader.Singleton.Initialize();
// #if REIW_DEBUG
//             UIManager.Hide<ConsoleUI>(UIList.ConsoleUI);
// #endif
//             GameDataModel.Singleton.Initialize();
//             UserDataModel.Singleton.Initialize();
//
//             isInitializedCoreSystems = true;
//         }
//
// #if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS)
//         private async void Start()
//         {
//             Initialize();
//             StartTitleFlow();
//         }
// #elif UNITY_EDITOR
//         private async void Start()
//         {
//             if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name.Equals("REIW.Main"))
//             {
//                 Initialize();
//                 StartTitleFlow();
//             }
//         }
// #endif
//
//         private void OnGUI()
//         {
//             ReGUIUtility.DrawGUI();
//             if (Event.current.type == EventType.Repaint)
//                 ReGUIUtility.Clear();
//         }
//
//         public event System.Action OnSystemDecommission;
//
//         public void SystemQuit()
//         {
//             OnSystemDecommission?.Invoke();
// #if !UNITY_EDITOR
//             Application.Quit();
// #elif UNITY_EDITOR
//             UnityEditor.EditorApplication.isPlaying = false;
// #endif
//         }
//     }
// }