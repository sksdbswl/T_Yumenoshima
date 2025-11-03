#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REIW
{
    public class BootStrapper
    {
        private const string BootStrapperMenuPath = "PROJECT REIW/BootStrapper/Activate BootStrapper";
        
        public static bool IsActivateBootStrapper
        {
            get => UnityEditor.EditorPrefs.GetBool(BootStrapperMenuPath, false);
            set
            {
                UnityEditor.EditorPrefs.SetBool(BootStrapperMenuPath, value);
                UnityEditor.Menu.SetChecked(BootStrapperMenuPath, value);
            }
        }

        [UnityEditor.MenuItem(BootStrapperMenuPath, priority = 1)]
        private static void ActivateBootStrapper()
        {
            // IsActivateBootStrapper = !IsActivateBootStrapper;
            IsActivateBootStrapper = false;
            UnityEditor.Menu.SetChecked(BootStrapperMenuPath, IsActivateBootStrapper);
        }

        [UnityEditor.MenuItem(BootStrapperMenuPath, isValidateFunction:true, priority = 1)]
        private static bool ActivateBootStrapperValidate() { return false; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void SystemBoot()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (IsActivateBootStrapper && false == activeScene.name.Equals("REIW.Main"))
            {
                LogUtil.Log(
                    "REIW-BootStrapper 가 활성화 되어있습니다. \n" +
                    "BootStrapper 를 사용하지 않으시려면, Top Menu > PROJECT_REIW > BootStrapper > Activate BootStrapper 를 체크 해제 해주세요.");
                
                Main.Singleton.Initialize(OnSystemLoadCompletedCallback);
            }
        }

        private static void OnSystemLoadCompletedCallback()
        {
            // To do : Custom System Load
            
        }
        
        [MenuItem("PROJECT REIW/Move Scene/1_REIW Main Scene")]
        public static void GotoREIWMainScene()
        {
            OpenScene("Assets/01_REIW/Scenes/Built-In/REIW.Main.unity");
        }
        
        [MenuItem("PROJECT REIW/Move Scene/2_Main_Island_Test")]
        public static void GotoREIWMain_Island_Test()
        {
            OpenScene("Assets/01_REIW/Scenes/Bundle/Level/Main_Island_Test.unity");
        }
        
        [MenuItem("PROJECT REIW/Move Scene/3_REIW Core Scene")]
        public static void GotoREIWCoreScene()
        {
            OpenScene("Assets/01_REIW/Scenes/Bundle/REIW.Ingame.unity");
        }
        
        [MenuItem("PROJECT REIW/Move Scene/4_ART Test Scene")]
        public static void GotoREIWArtTestScene()
        {
            OpenScene("Assets/Scenes/Main_Island.unity");
        }
     
        [MenuItem("PROJECT REIW/Move Scene/5_REIW.Customizing Scene")]
        public static void GotoREIWCustomizingScene()
        {
            OpenScene("Assets/01_REIW/Scenes/Bundle/REIW.Customizing.unity");
        }

        [MenuItem("PROJECT REIW/Move Scene/6_REIW.Level.Main_Island Scene")]
        public static void GotoREIWLevelMainIslandScene()
        {
            OpenScene("Assets/Scenes/REIW.Level.Main_Island.unity");
        }
        
        [MenuItem("PROJECT REIW/Move Scene/7.AnimationTestScene")]
        public static void GotoAnimationTestScene()
        {
            OpenScene("Assets/Scenes/AnimationTestScene.unity");
        }
        [MenuItem("PROJECT REIW/Move Scene/8.ShopTestScene")]
        public static void GotoShopUITestScene()
        {
            OpenScene("Assets/01_REIW/Scenes/Perosnal/Voyager_SonJunhyuck/test_Shop.unity");
        }

        [MenuItem("PROJECT REIW/Move Scene/9.REIW.CharacterPartsTest")]
        public static void GotoCharacterPartsTest()
        {
            OpenScene("Assets/Scenes/CharacterPartsTestScene.unity");
        }
        
        [MenuItem("PROJECT REIW/Move Scene/99.REIW.MountCustomizingTest")]
        public static void GotoMountCustomizingTest()
        {
            OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_JangSijin/REIW.MountCustomizingTest.unity");
        }
        
        
        public static void GotoArtUIScene_Seokho() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_UI/UI_Team_SeokHo.unity");
        public static void GotoArtUIScene_Ogil() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_UI/UI_Team_Ogil.unity");
        public static void GotoArtUIScene_Daeun() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_UI/UI_Team_Daeun.unity");
        public static void GotoArtUIScene_Seoyoung() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_UI/UI_Team_Seoyoung.unity");
        public static void GotoArtUIScene_Jeongmin() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_UI/UI_Team_Jeongmin.unity");
        public static void GotoArtUIScene_Chanho() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_UI/UI_Team_Chanho.unity");
        public static void GotoMountCustomizingTest_YoungSun() => OpenScene("Assets/01_REIW/Scenes/Personal/Voyager_JangSijin/REIW.MountCustomizingTest.unity");
        
        private static void OpenScene(string sceneName, bool isPlaying = false)
        {
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }
            EditorSceneManager.OpenScene(sceneName);
            EditorApplication.isPlaying = isPlaying;
        }
    }
}
#endif