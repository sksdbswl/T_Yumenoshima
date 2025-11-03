using System;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(menuName = "REIW/Config/Create Client Network Config")]
    public class ClientNetworkConfigSo : ScriptableObject
    {
        [Header("Auth")]
        public string authUrl = "http://13.125.198.74:20000/api/token";

        [Serializable]
        public class LobbyConfig
        {
            public string defaultHost = "ws://192.168.200.39";
            public UDictionary<string, int> portByEnv = new();             // 키: "Dev","Choi"...
            public UDictionary<string, string> hostOverrideByEnv = new();  // 필요 시 호스트 override
        }

        [Serializable]
        public class GameConfig
        {
            public string defaultHost = "ws://192.168.53.31";
            public string dockerHost = "ws://192.168.200.108";
            public UDictionary<string, int> portByEnv = new();
            public UDictionary<string, string> hostOverrideByEnv = new();
        }

        [Header("Lobby Server")]
        public LobbyConfig lobby = new();

        [Header("Game Server")]
        public GameConfig game = new();

#if UNITY_EDITOR
        private static readonly string[] DefaultServerKeys =
        {
            "Dev","Choi","Park","Kim","Koshiba","Jang_QA","Oh","Sung","DockerBuild","Client"
        };

        private void OnValidate()
        {
            // Lobby 기본 포트
            foreach (var key in DefaultServerKeys)
            {
                if (!lobby.portByEnv.ContainsKey(key))
                    lobby.portByEnv[key] = 30000;

                if (key == "DockerBuild" && !lobby.hostOverrideByEnv.ContainsKey(key))
                    lobby.hostOverrideByEnv[key] = lobby.defaultHost;
            }

            // Game 기본 포트
            if (!game.portByEnv.ContainsKey("Dev"))       game.portByEnv["Dev"]       = 40000;
            if (!game.portByEnv.ContainsKey("Choi"))      game.portByEnv["Choi"]      = 40001;
            if (!game.portByEnv.ContainsKey("Park"))      game.portByEnv["Park"]      = 40002;
            if (!game.portByEnv.ContainsKey("Kim"))       game.portByEnv["Kim"]       = 40003;
            if (!game.portByEnv.ContainsKey("Koshiba"))   game.portByEnv["Koshiba"]   = 40004;
            if (!game.portByEnv.ContainsKey("Jang_QA"))   game.portByEnv["Jang_QA"]   = 40005;
            if (!game.portByEnv.ContainsKey("Oh"))        game.portByEnv["Oh"]        = 40006;
            if (!game.portByEnv.ContainsKey("Sung"))      game.portByEnv["Sung"]      = 40007;
            if (!game.portByEnv.ContainsKey("DockerBuild")) game.portByEnv["DockerBuild"] = 42000;
            if (!game.portByEnv.ContainsKey("Client"))    game.portByEnv["Client"]    = 41000;

            if (!game.hostOverrideByEnv.ContainsKey("DockerBuild"))
                game.hostOverrideByEnv["DockerBuild"] = game.dockerHost;
        }
#endif

        
        //================================================
        // Url Getters
        //================================================
        
        public string GetAuthUrl() => authUrl;

        public string GetLobbyWsUrl(string envKey)
        {
            string host = lobby.defaultHost;
            if (lobby.hostOverrideByEnv.TryGetValue(envKey, out var ov) && !string.IsNullOrWhiteSpace(ov))
                host = ov;

            int port = 30000;
            if (lobby.portByEnv.TryGetValue(envKey, out var p)) port = p;

            return $"{host}:{port}";
        }

        public string GetGameWsUrl(string envKey)
        {
            string host = game.defaultHost;

            if (envKey == "DockerBuild" && !string.IsNullOrWhiteSpace(game.dockerHost))
                host = game.dockerHost;

            if (game.hostOverrideByEnv.TryGetValue(envKey, out var ov) && !string.IsNullOrWhiteSpace(ov))
                host = ov;

            int port = 40000;
            if (game.portByEnv.TryGetValue(envKey, out var p)) port = p;

            return $"{host}:{port}";
        }

        public int GetGamePort(string envKey)
        {
            return game.portByEnv.TryGetValue(envKey, out var p) ? p : 40000;
        }
    }
}
