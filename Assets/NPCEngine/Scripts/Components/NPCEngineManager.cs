using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using ProcessUtils;
using NPCEngine.API;
using NPCEngine.Utility;
using NPCEngine.RPC;

namespace NPCEngine.Components
{

    public class NPCEngineManager : Singleton<NPCEngineManager>
    {
        private int inferenceEngineProcessId = -1;

        private bool initialized = false;

        public bool Initialized
        {
            get { return initialized; }
        }

        public bool InferenceEngineRunning
        {
            get { return inferenceEngineProcessId != -1; }
        }

        public void StartInferenceEngine()
        {
            if (inferenceEngineProcessId == -1)
            {

                Process myProcess = new Process();

                if (NPCEngineConfig.Instance.debug)
                {
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    myProcess.StartInfo.CreateNoWindow = false;
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = "CMD.EXE";
                    myProcess.StartInfo.Arguments = String.Format(
                        " /k \"\"{0}\" --verbose run --port {1} --models-path \"{2}\" --dont-start"
                        + (NPCEngineConfig.Instance.serverType == ServerType.ZMQ ? "\"" : " --http\""),
                        Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.npcEnginePath),
                        NPCEngineConfig.Instance.Port,
                        Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath)
                    );
                    myProcess.EnableRaisingEvents = true;
                    myProcess.Start();
                }
                else
                {
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.npcEnginePath);
                    myProcess.StartInfo.Arguments = String.Format(
                        "\"run --port {0} --models-path \"{1}\" --dont-start"
                        + (NPCEngineConfig.Instance.serverType == ServerType.ZMQ ? "\"" : " --http\""),
                        NPCEngineConfig.Instance.Port,
                        Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath)
                    );
                    myProcess.EnableRaisingEvents = true;
                    myProcess.Start();
                    ChildProcessTracker.AddProcess(myProcess);
                }
                inferenceEngineProcessId = myProcess.Id;
                if (Application.IsPlaying(this))
                {
                    StartCoroutine("InferenceEngineHealthCheck");
                }

                var rpcComponents = FindObjectsOfType<RPCBase>();
                foreach (var rpcComponent in rpcComponents)
                {
                    rpcComponent.Connect();
                }

                foreach (var service in NPCEngineConfig.Instance.services)
                {
                    if (service.start)
                    {
                        UnityEngine.Debug.Log("Starting service " + service.name);
                        StartCoroutine(GetAPI<Control>().StartService(service.name));
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Trying to start InferenceEngine that is already running");
            }
        }

        public void StopInferenceEngine()
        {
            try
            {
                var rpcComponents = FindObjectsOfType<RPCBase>();
                foreach (var rpcComponent in rpcComponents)
                {
                    rpcComponent.Disconnect();
                }
                if (inferenceEngineProcessId != -1)
                {
                    Process myProcess = Process.GetProcessById(inferenceEngineProcessId);
                    myProcess.Kill();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Trying to stop InferenceEngine that is not running");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }
            finally
            {
                inferenceEngineProcessId = -1;
            }

        }

        IEnumerator InferenceEngineHealthCheck()
        {
            while (true && !NPCEngineConfig.Instance.connectToExistingServer)
            {
                try
                {
                    var proc = Process.GetProcessById(inferenceEngineProcessId);
                }
                catch (ArgumentException)
                {
                    UnityEngine.Debug.LogError("Fatal error: Dialog InferenceEngine unresponsive");
                    inferenceEngineProcessId = -1;
                    Application.Quit(1);
                }
                yield return new WaitForSeconds(5f);
            }
        }

        // Start is called before the first frame update
        private void Awake()
        {
            if (!NPCEngineConfig.Instance.connectToExistingServer)
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
                api.Connect();
            }
            return api;
        }
    }
}