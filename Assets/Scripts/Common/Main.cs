using System;
using UnityEngine;

namespace REIW
{
    public partial class Main : SingletonBase<Main>
    {
        public enum eConnectBundlePort { Dev, QA }

        public ClientNetworkConfigSo NetworkConfig => networkConfig;
        [Header("Network Config (SO)")]
        [SerializeField] private ClientNetworkConfigSo networkConfig;

        
        public ArtConfigSO ArtConfig => _artConfigSO;
        [Header("Art Config (SO)")]
        [SerializeField] private ArtConfigSO _artConfigSO;
        
        
        [Header("Current Server Env (string key)")]
        [SerializeField] private string serverEnv = "Dev"; // 예: "Dev","Choi","DockerBuild"...

        public eConnectBundlePort ConnectBundlePort
        {
            get => connectBundlePort;
            set
            {
                connectBundlePort = value;
                AssetManager.Singleton.SetRemoteLocationChange(connectBundlePort);
            }
        }

#if REIW_QA
        [SerializeField] private eConnectBundlePort connectBundlePort = eConnectBundlePort.QA;
#else
        [SerializeField] private eConnectBundlePort connectBundlePort = eConnectBundlePort.Dev;
#endif

        [HideInInspector] public bool TestTileMap = true;

        public void SetServerEnv(string envKey)
        {
            serverEnv = envKey;
        }

        public string GetAuthUrl()
        {
            return networkConfig != null ? networkConfig.GetAuthUrl() : string.Empty;
        }

        public string GetLobbyServerUrl()
        {
            string url = (networkConfig != null) ? networkConfig.GetLobbyWsUrl(serverEnv) : string.Empty;
            LogUtil.Log(("GetLobbyServerUrl :" + url).Color(Color.yellow));
            return url;
        }

        // 서버에서 host/port를 내려줄 수 있으므로, 있으면 우선 적용. 없으면 SO 값 사용.
        public string GetGameServerUrl(string hostFromServer, int portFromServer)
        {
            // string url;
            // if (!string.IsNullOrWhiteSpace(hostFromServer) && portFromServer > 0)
            //     url = $"ws://{hostFromServer}:{GetGameServerPort()}"; // Port 값은 So 파일을 기준으로 override 함
            // else
            //     url = (networkConfig != null) ? networkConfig.GetGameWsUrl(serverEnv) : string.Empty;
            
            string url = (networkConfig != null) ? networkConfig.GetGameWsUrl(serverEnv) : string.Empty;
            LogUtil.Log(("GetGameServerUrl :" + url).Color(Color.yellow));
            return url;
        }

        public int GetGameServerPort()
        {
            return (networkConfig != null) ? networkConfig.GetGamePort(serverEnv) : 0;
        }

        public eSceneType GetSceneType()
        {
            return currentSceneFlow.SceneType;
        }

        //===================================================
        // Change Scene Flow
        //===================================================
        
        public async void StartTitleFlow()
        {
            var nextScene = new TitleSceneFlow();
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene, false);
            currentSceneFlow = nextScene;
        }

        public async void StartIngameFlow()
        {
            if (!isInitializedCoreSystems)
            {
                InitializeBasicSystems();
            }
            
            var nextScene = new IngameSceneFlow();
            
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene);
            
            currentSceneFlow = nextScene;
        }

        public void DisableAudioListener()
        {
            if(GetComponent<AudioListener>())
                GetComponent<AudioListener>().enabled = false;
        }

        public async void StartCustomizeFlow()
        {
            var nextScene = new CustomizeSceneFlow();
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene);
            currentSceneFlow = nextScene;
        }

        public async void StartHousingFlow()
        {
            var nextScene = new HousingSceneFlow();
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene);
            currentSceneFlow = nextScene;
        }

        public async void StartCharacterSelectFlow()
        {
            var nextScene = new CharacterSelectionSceneFlow();
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene);
            currentSceneFlow = nextScene;
        }

        public async void StartSeamlessSceneFlow()
        {
            var nextScene = new SeamlessSceneFlow();
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene);
            currentSceneFlow = nextScene;
        }
        
        public async void StartFullWorldSceneFlow()
        {
            var nextScene = new FullWorldSceneFlow();
            await SceneFlowExecutor.RunSceneFlow(currentSceneFlow, nextScene);
            currentSceneFlow = nextScene;
        }
    }
}