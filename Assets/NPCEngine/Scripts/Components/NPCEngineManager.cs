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
    [ExecuteInEditMode]
    public class NPCEngineManager : Singleton<NPCEngineManager>
    {

        private List<ServiceMetadata> services;

        public List<ServiceMetadata> Services
        {
            get
            {
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

        [SerializeField]
        [HideInInspector]
        private int inferenceEngineProcessId;

        protected int InferenceEngineProcessId
        {
            get
            {
                return inferenceEngineProcessId;
            }
            set
            {
                inferenceEngineProcessId = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        private Process inferenceEngineProcess;

        public bool InferenceEngineRunning
        {
            get
            {
                if (inferenceEngineProcessId != -1 && inferenceEngineProcess == null)
                {
                    try
                    {
                        inferenceEngineProcess = Process.GetProcessById(inferenceEngineProcessId);
                    }
                    catch (ArgumentException)
                    {
                        InferenceEngineProcessId = -1;
                    }
                }
                return (inferenceEngineProcess != null && !inferenceEngineProcess.HasExited);
            }
        }

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

                foreach (var service in NPCEngineConfig.Instance.services)
                {
                    if (service.start)
                    {
                        CoroutineUtility.StartCoroutine(GetAPI<Control>().StartService(service.name), this, "ManagerStartServiceCoroutine_" + service.name);
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Trying to start InferenceEngine that is already running");
            }
            CoroutineUtility.StartCoroutine(UpdateServiceStatuses(), this, "UpdateServiceStatuses");
            CoroutineUtility.StartCoroutine(UpdateServices(), this, "UpdateServices");
        }

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

        public void StopInferenceEngine()
        {
            CoroutineUtility.StopCoroutine("UpdateServiceStatuses", this);
            CoroutineUtility.StopCoroutine("UpdateServices", this);
            serviceStatuses = null;
            services = null;
            foreach (var service in NPCEngineConfig.Instance.services)
            {
                GetAPI<Control>().StopServiceNoConfirm(service.name);
            }
            CoroutineUtility.StartCoroutine(StopInferenceEngineCoroutine(), this, "StopInferenceEngineCoroutine");
            CoroutineUtility.StopCoroutine("ManagerHealthCheckCoroutine", this);
        }

        private IEnumerator StopInferenceEngineCoroutine()
        {
            if (Application.isPlaying)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return new EditorWaitForSeconds(1f);
            }
            try
            {
                if (InferenceEngineRunning)
                {
                    inferenceEngineProcess.CloseMainWindow();
                    inferenceEngineProcess.WaitForExit();
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
                    var proc = Process.GetProcessById(inferenceEngineProcessId);
                }
                catch (ArgumentException e)
                {
                    UnityEngine.Debug.LogFormat("InferenceEngine process {0} is not running: {1}", inferenceEngineProcessId, e.Message);

                    UnityEngine.Debug.LogError("Fatal error: Dialog InferenceEngine unresponsive");
                    InferenceEngineProcessId = -1;
                    Application.Quit(1);
                    break;
                }
                if (Application.isPlaying)
                {
                    yield return new WaitForSeconds(5f);
                }
                else
                {
                    yield return new EditorWaitForSeconds(5f);
                }
            }
#endif
            yield return null;
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                StartInferenceEngine();
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnEnable()
        {
            if (!Application.isPlaying && InferenceEngineRunning)
            {
                StartInferenceEngine();
            }
        }


        public T GetAPI<T>() where T : RPCBase
        {
            var api = GetComponent<T>();
            if (api == null)
            {
                api = gameObject.AddComponent<T>();
            }
            return api;
        }

        public T GetAPI<T>(string id) where T : RPCBase
        {
            var api = GetComponents<T>();
            if (api == null)
            {
                api = gameObject.AddComponent<T>();
            }
            return api;
        }


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
                if (Application.isPlaying)
                {
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    yield return new EditorWaitForSeconds(1);
                }
            }
        }

        public IEnumerator UpdateServices()
        {
            while (true)
            {
                StartCoroutine(NPCEngineManager.Instance.GetAPI<Control>().GetServicesMetadata((List<ServiceMetadata> s) =>
                {
                    services = s;
                }));
                if (Application.isPlaying)
                {
                    yield return new WaitForSeconds(4);
                }
                else
                {
                    yield return new EditorWaitForSeconds(4);
                }
            }
        }

    }
}