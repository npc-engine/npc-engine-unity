using NPCEngine.Utility;
using UnityEngine;

namespace NPCEngine.RPC
{
    /// <summary>
    /// Data class containing NPC Engine config.
    /// </summary>
    public class RPCConfig : Singleton<RPCConfig>
    {
        public ServerType serverType = ServerType.HTTP;

        public string serverAddress = "localhost:8080";

        [Tooltip("Relative to StreamingAssets folder")]
        public string modelsPath = ".models/";

        [Tooltip("Relative to StreamingAssets folder")]
        public string npcEnginePath = ".npc-engine/cli.exe";

        public bool debug = false;
        public bool connectToExistingServer = false;

        public int inferenceEngineProcessId = -1;

        public bool initialized = false;
    }
}