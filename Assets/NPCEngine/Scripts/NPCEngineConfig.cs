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

    [ExecuteAlways]
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
        public string npcEnginePath = ".npc-engine/npc-engine.exe";

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

        void OnEnable()
        {
            RefreshServices();
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

        public void RefreshServices()
        {
            services = GetServicesManually();
        }

        private List<ServiceConfigDescriptor> GetServicesManually()
        {
            // Get all directories in models folder
            string[] directories = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath));
            List<ServiceConfigDescriptor> services = new List<ServiceConfigDescriptor>();
            foreach (string directory in directories)
            {
                ServiceConfigDescriptor service = new ServiceConfigDescriptor();
                service.name = Path.GetFileName(directory);
                string[] lines = System.IO.File.ReadAllLines(Path.Combine(directory, "config.yml"));
                service.path = directory;
                foreach (var line in lines)
                {
                    if (line.StartsWith("type:"))
                    {
                        service.type = line.Replace("type: ", "");
                    }
                    else if (line.StartsWith("model_type:"))
                    {
                        service.type = line.Replace("model_type: ", "");
                    }
                }
                services.Add(service);
            }
            // Copy start setting from existing config
            if (NPCEngineConfig.Instance.services != null)
            {
                foreach (var service in services)
                {
                    foreach (var existingService in NPCEngineConfig.Instance.services)
                    {
                        if (service.name == existingService.name)
                        {
                            service.start = existingService.start;
                        }
                    }
                }
            }
            return services;
        }

    }
}