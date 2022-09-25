using System.Net.Mime;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using ProcessUtils;
using NPCEngine.API;
using NPCEngine.Utility;
using NPCEngine.RPC;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace NPCEngine.Components
{
    /// <summary>
    /// Manager class that handles services and server lifetime and status.
    /// </summary>
    [ExecuteAlways]
    public class NPCEngineManager : Singleton<NPCEngineManager>
    {

        private List<ServiceMetadata> services;

        public List<ServiceMetadata> Services
        {
            get
            {
                if (services == null)
                {
                    services = new List<ServiceMetadata>();
                }
                return services;
            }
        }

        public Dictionary<string, ServiceMetadata> ServicesById
        {
            get
            {
                Dictionary<string, ServiceMetadata> servicesDictionary = new Dictionary<string, ServiceMetadata>();
                foreach (ServiceMetadata service in services)
                {
                    servicesDictionary.Add(service.id, service);
                }
                return servicesDictionary;
            }
        }

        private List<ServiceStatus> serviceStatuses;

        public List<ServiceStatus> ServiceStatuses
        {
            get
            {
                if (serviceStatuses == null || serviceStatuses.Count != Services.Count)
                {
                    serviceStatuses = new List<ServiceStatus>();
                }
                return serviceStatuses;
            }
        }

        public Dictionary<string, ServiceStatus> ServiceStatusesById
        {
            get
            {
                if (serviceStatuses == null || serviceStatuses.Count != Services.Count)
                {
                    serviceStatuses = new List<ServiceStatus>();
                }
                Dictionary<string, ServiceStatus> serviceStatusesDictionary = new Dictionary<string, ServiceStatus>();
                int i = 0;
                foreach (ServiceStatus serviceStatus in serviceStatuses)
                {
                    serviceStatusesDictionary.Add(services[i].id, serviceStatus);
                    i++;
                }
                return serviceStatusesDictionary;
            }
        }

        private int inferenceEngineProcessId = -1;

        protected int InferenceEngineProcessId
        {
            get
            {
                #if UNITY_EDITOR
                if (inferenceEngineProcessId == -1)
                {
                    inferenceEngineProcessId = EditorPrefs.GetInt("NPCEngine.InferenceEngineProcessId");
                }
                #endif
                return inferenceEngineProcessId;
            }
            set
            {
                inferenceEngineProcessId = value;
                #if UNITY_EDITOR
                EditorPrefs.SetInt("NPCEngine.InferenceEngineProcessId", inferenceEngineProcessId);
                #endif
            }
        }

        private Process inferenceEngineProcess;

        public bool InferenceEngineRunning
        {
            get
            {
                if (InferenceEngineProcessId != -1 && inferenceEngineProcess == null)
                {
                    try
                    {
                        inferenceEngineProcess = Process.GetProcessById(InferenceEngineProcessId);
                    }
                    catch (ArgumentException)
                    {
                        UnityEngine.Debug.LogWarning("Inference engine process not found.");
                        InferenceEngineProcessId = -1;
                        inferenceEngineProcess = null;
                    }
                }
                try {
                    return (inferenceEngineProcess != null && !inferenceEngineProcess.HasExited);
                } catch (InvalidOperationException) {
                    UnityEngine.Debug.LogWarning("Inference engine process object is no longer relevant.");
                    inferenceEngineProcess = null;
                    return InferenceEngineRunning;
                }
            }
        }


        /// <summary>
        /// Starts the inference engine server and managing coroutines.
        /// </summary>
        public void StartInferenceEngine()
        {
            if (!InferenceEngineRunning && !NPCEngineConfig.Instance.connectToExistingServer)
            {
                inferenceEngineProcess = new Process();

                serviceStatuses = null;
                services = null;
                if (NPCEngineConfig.Instance.serverConsole)
                {
                    inferenceEngineProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    inferenceEngineProcess.StartInfo.CreateNoWindow = false;
                    inferenceEngineProcess.StartInfo.UseShellExecute = true;
                    inferenceEngineProcess.StartInfo.FileName = "CMD.EXE";
                    inferenceEngineProcess.StartInfo.Arguments = String.Format(
                        " /k \"\"{0}\" --verbose run --port {1} --models-path \"{2}\" --dont-start"
                        + (NPCEngineConfig.Instance.serverType == ServerType.ZMQ ? "\"" : " --http\""),
                        Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.npcEnginePath),
                        NPCEngineConfig.Instance.Port,
                        Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath)
                    );
                    inferenceEngineProcess.EnableRaisingEvents = true;
                    inferenceEngineProcess.Start();
                    InferenceEngineProcessId = inferenceEngineProcess.Id;
                }
                else
                {
                    inferenceEngineProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    inferenceEngineProcess.StartInfo.CreateNoWindow = true;
                    inferenceEngineProcess.StartInfo.UseShellExecute = false;
                    inferenceEngineProcess.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.npcEnginePath);
                    inferenceEngineProcess.StartInfo.Arguments = String.Format(
                        "run --port {0} --models-path \"{1}\" --dont-start"
                        + (NPCEngineConfig.Instance.serverType == ServerType.ZMQ ? "\"" : " --http"),
                        NPCEngineConfig.Instance.Port,
                        Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath)
                    );
                    inferenceEngineProcess.EnableRaisingEvents = true;
                    inferenceEngineProcess.Start();
                    ChildProcessTracker.AddProcess(inferenceEngineProcess);
                    InferenceEngineProcessId = inferenceEngineProcess.Id;
                }

                CoroutineUtility.StartCoroutine(InferenceEngineHealthCheck(), this, "ManagerHealthCheckCoroutine");

                CoroutineUtility.StartCoroutine(StartServices(), this, "StartServicesCoroutine");
            }
            else
            {
                UnityEngine.Debug.LogWarning("Trying to start InferenceEngine that is already running");
            }

            if(!CoroutineUtility.IsRunning(this, "UpdateServiceStatuses"))
                CoroutineUtility.StartCoroutine(UpdateServiceStatuses(), this, "UpdateServiceStatuses");
            if(!CoroutineUtility.IsRunning(this, "UpdateServices"))
                CoroutineUtility.StartCoroutine(UpdateServices(), this, "UpdateServices");
            if(!CoroutineUtility.IsRunning(this, "StartAndMonitorServerLife"))
                CoroutineUtility.StartCoroutine(StartAndMonitorServerLife(), this, "StartAndMonitorServerLife");
        }

        private IEnumerator StartServices()
        {
            bool all_started = false;
            while (!all_started)
            {
                all_started = true;
                foreach (var service in NPCEngineConfig.Instance.services)
                {
                    if (service.start)
                    {
                        ServiceStatus status = ServiceStatus.UNKNOWN;
                        yield return GetAPI<Control>().GetServiceStatus(service.name, (x) => { status = x; });
                        if (status != ServiceStatus.RUNNING || status != ServiceStatus.STARTING)
                        {
                            yield return GetAPI<Control>().StartService(service.name);
                            all_started = false;
                        }
                    }
                }
                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// Download model by ID
        /// </summary>
        /// <param name="id"></param>
        public void DownloadModel(string id)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            myProcess.StartInfo.CreateNoWindow = false;
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.npcEnginePath);
            myProcess.StartInfo.Arguments = String.Format(
                " --verbose download-model --models-path \"{0}\" \"{1}\"",
                Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath),
                id
            );
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
        }

        /// <summary>
        /// Stop the inference engine server, services and dispose resources.
        /// </summary>
        public void StopInferenceEngine()
        {
            CoroutineUtility.StartCoroutine(StopInferenceEngineCoroutine(), this, "StopInferenceEngineCoroutine");

            CoroutineUtility.StopCoroutine("StartAndMonitorServerLife", this);
            CoroutineUtility.StopCoroutine("UpdateServiceStatuses", this);
            CoroutineUtility.StopCoroutine("UpdateServices", this);
            CoroutineUtility.StopCoroutine("ManagerHealthCheckCoroutine", this);
            
        }

        private IEnumerator StopInferenceEngineCoroutine()
        {
            
            foreach (var service in NPCEngineConfig.Instance.services)
            {
                yield return GetAPI<Control>().StopService(service.name);
            }

            try
            {
                if (InferenceEngineRunning)
                {
                    inferenceEngineProcess.CloseMainWindow();
                    inferenceEngineProcess.WaitForExit();
                    inferenceEngineProcess.Kill();
                    inferenceEngineProcess.Dispose();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Trying to stop InferenceEngine that is not running");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("Error while stopping InferenceEngine: " + e.Message);
            }
            finally
            {
                inferenceEngineProcess = null;
                InferenceEngineProcessId = -1;
            }

            yield return null;
        }

        IEnumerator InferenceEngineHealthCheck()
        {
#if !UNITY_EDITOR
            while (!NPCEngineConfig.Instance.connectToExistingServer)
            {
                try
                {
                    var proc = Process.GetProcessById(InferenceEngineProcessId);
                }
                catch (ArgumentException e)
                {
                    UnityEngine.Debug.LogFormat("InferenceEngine process {0} is not running: {1}", InferenceEngineProcessId, e.Message);

                    UnityEngine.Debug.LogError("Fatal error: Dialog InferenceEngine unresponsive");
                    InferenceEngineProcessId = -1;
                    Application.Quit(1);
                    break;
                }
                yield return CoroutineUtility.WaitForSeconds(5f);
            }
#endif
            yield return null;
        }


        private void Awake() {
            if(Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
        }

        void OnEnable()
        {
            StartInferenceEngine();
        }

        /// <summary>
        /// Get or construct API for the given type.
        /// </summary>
        /// <typeparam name="T">API type</typeparam>
        /// <returns></returns>
        public T GetAPI<T>() where T : RPCBase
        {
            var api = GetComponent<T>();
            if (api == null)
            {
                api = gameObject.AddComponent<T>();
            }
            return api;
        }

        /// <summary>
        /// Get or construct API for the given type and service ID.
        /// </summary>
        /// <param name="id">Service ID or otherwise resolvable name</param>
        /// <typeparam name="T">API Type</typeparam>
        /// <returns></returns>
        public T GetAPI<T>(string id) where T : RPCBase
        {
            var apis = GetComponents<T>();
            foreach (var api in apis)
            {
                if (api.serviceId == id)
                {
                    return api;
                }
            }
            var api2 = gameObject.AddComponent<T>();
            api2.serviceId = id;
            return api2;   
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerator StartAndMonitorServerLife()
        {
            yield return CoroutineUtility.WaitForSeconds(1f);
            while (true)
            {
                if (!InferenceEngineRunning)
                {
                    var npcEnginePath = Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.npcEnginePath);
                    if(File.Exists(npcEnginePath))
                    {
                        if(NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.Log("InferenceEngine is not running. Starting...");
                        StartInferenceEngine();
                    }
                }
                yield return CoroutineUtility.WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Coroutine that updates service statuses.
        /// </summary>
        public IEnumerator UpdateServiceStatuses()
        {
            while (true)
            {
                if (services != null)
                {
                    if (serviceStatuses == null || serviceStatuses.Count != services.Count)
                    {
                        serviceStatuses = new List<ServiceStatus>();
                        for (int i = 0; i < services.Count; i++)
                        {
                            serviceStatuses.Add(ServiceStatus.UNKNOWN);
                        }
                    }
                    for (int i = 0; i < services.Count; i++)
                    {
                        yield return NPCEngineManager.Instance.GetAPI<Control>().GetServiceStatus(services[i].id, (ServiceStatus s) =>
                        {
                            if (i >= serviceStatuses.Count)
                            {
                                UnityEngine.Debug.LogErrorFormat("Service status index out of range {0} i= {1}", serviceStatuses.Count, i);
                                return;
                            }
                            serviceStatuses[i] = s;
                        });
                    }
                }
                
                yield return CoroutineUtility.WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Coroutine that updates service list.
        /// </summary>
        public IEnumerator UpdateServices()
        {
            while (true)
            {
                yield return NPCEngineManager.GetInstance().GetAPI<Control>().GetServicesMetadata((List<ServiceMetadata> s) =>
                {
                    try{
                        services = s;
                    } catch (Exception)
                    {}
                });
                yield return CoroutineUtility.WaitForSeconds(4f);
            }
        }
    }
}