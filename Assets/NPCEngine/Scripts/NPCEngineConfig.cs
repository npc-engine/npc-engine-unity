using System;
using UnityEngine;
using NPCEngine.Utility;
using NPCEngine.RPC;
using System.IO;
using System.Collections.Generic;

namespace NPCEngine
{

    [Serializable]
    public class ServiceConfigDescriptor
    {
        public string name;
        public string type;
        public string path;
        public bool start;
    }
    /// <summary>
    /// Data class containing NPC Engine config.
    /// </summary>
    public class NPCEngineConfig : Singleton<NPCEngineConfig>
    {
        [HideInInspector]
        public List<ServiceConfigDescriptor> services = null;

        public ServerType serverType = ServerType.HTTP;

        public string serverAddress = "localhost:8080";

        public string Port
        {
            get
            {
                return serverAddress.Split(':')[1];
            }
        }

        [Tooltip("Relative to StreamingAssets folder")]
        public string modelsPath = ".models/";

        [Tooltip("Relative to StreamingAssets folder")]
        public string npcEnginePath = ".npc-engine/cli.exe";

        public bool debugLogs = false;
        public bool serverConsole = false;
        public bool connectToExistingServer = false;

        [Header("Text Generation")]
        [Range(0, 2)]
        public float temperature = 1;
        public int topK = 0;

        
        [Header("Speech Generation")]
        public int nChunksSpeechGeneration = 7;

        private void Awake()
        {
        }

        public bool ToBeStarted(string name)
        {
            if (services == null)
            {
                return false;
            }

            foreach (ServiceConfigDescriptor service in services)
            {
                if (service.name == name)
                {
                    return service.start;
                }
            }

            return false;
        }

    }
}