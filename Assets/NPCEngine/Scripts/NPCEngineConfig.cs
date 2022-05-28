using NPCEngine.Utility;
using NPCEngine.RPC;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace NPCEngine
{

    public class ServiceConfigDescriptor
    {
        public string name;
        public string type;
        public bool start;
    }
    /// <summary>
    /// Data class containing NPC Engine config.
    /// </summary>
    public class NPCEngineConfig : Singleton<NPCEngineConfig>
    {

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

        public bool debug = false;
        public bool connectToExistingServer = false;

        public int inferenceEngineProcessId = -1;

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