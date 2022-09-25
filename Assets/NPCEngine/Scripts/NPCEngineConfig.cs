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
        [SerializeField]
        public string name;
        [SerializeField]
        public string type;
        [SerializeField]
        public string path;
        [SerializeField]
        public bool start;
    }

    [ExecuteAlways]
    /// <summary>
    /// Data class containing NPC Engine config.
    /// </summary>
    public class NPCEngineConfig : Singleton<NPCEngineConfig>
    {
        [HideInInspector]
        [SerializeField]
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

        public bool NPCEngineInstalled
        {
            get
            {
                try{
                    return File.Exists(Path.Combine(Application.streamingAssetsPath, npcEnginePath));
                }
                catch(DirectoryNotFoundException)
                {
                    return false;
                }
            }
        }

        public bool ModelsPathExists
        {
            get
            {
                try{
                    return Directory.Exists(Path.Combine(Application.streamingAssetsPath, modelsPath));
                }
                catch(DirectoryNotFoundException)
                {
                    return false;
                }
            }
        }

        [Tooltip("Relative to StreamingAssets folder")]
        public string modelsPath = ".models/";

        [Tooltip("Relative to StreamingAssets folder")]
        public string npcEnginePath = ".npc-engine/npc-engine.exe";

        public bool debugLogs = false;
        public bool serverConsole = false;
        public bool connectToExistingServer = false;
        
        [Header("Speech Generation")]
        public int chunkCharacters = 20;

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
            if(!NPCEngineInstalled || !ModelsPathExists)
            {
                return new List<ServiceConfigDescriptor>();
            }
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