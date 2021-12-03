using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using ProcessUtils;
using NPCEngine.Utility;

namespace NPCEngine
{

    [Serializable()]
    public class NPCEngineException : System.Exception
    {

        public NPCEngineException() : base() { }
        public NPCEngineException(string message) : base(message) { }
        public NPCEngineException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected NPCEngineException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    public class ResultFuture<ReturnType>
    {
        public bool ResultReady
        {
            get
            {
                return resultReady;
            }
        }

        public ReturnType Result
        {
            get
            {
                if (resultReady)
                {
                    if (error != null)
                    {
                        throw error;
                    }
                    return result;
                }
                else
                {
                    throw new NPCEngineException("Computation not finished");
                }
            }
        }
        public NPCEngineException Error
        {
            get
            {
                return error;
            }
        }

        private ReturnType result;
        private NPCEngineException error;
        private bool resultReady = false;

        public void ResultFinishedCallback(ReturnType result)
        {
            this.result = result;
            this.resultReady = true;
        }

        public void ErrorCallback(NPCEngineException error)
        {
            this.resultReady = true;
            this.error = error;
        }
    }
}

namespace NPCEngine.Server
{

    [Serializable()]
    public class RPCRequstMessage<ParametersType>
    where ParametersType : new()
    {
        public int id = 0;
        public String jsonrpc = "2.0";
        public String method;

        [JsonProperty(PropertyName = "params")]
        public ParametersType parameters;

        public RPCRequstMessage()
        {
            if (!typeof(ParametersType).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(ParametersType))))
                throw new InvalidOperationException("A serializable argument type is required");
            parameters = new ParametersType();
            method = "";
        }
    }

    [Serializable()]
    public class RPCResponseMessage<ReturnType>
    {
        public int id = 0;
        public String jsonrpc = "2.0";

        public ReturnType result;

        public RPCResponseError error;

        public RPCResponseMessage()
        {
            if (!typeof(ReturnType).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(ReturnType))))
                throw new InvalidOperationException("A serializable argument type is required");
            error = new RPCResponseError();
        }
    }

    [Serializable()]
    public class RPCResponseError
    {
        public int code = 0;
        public String message = "";
    }

    /// <summary>
    /// Class <c>InferenceEngine</c> manages inference engine sidecart process lifetime and communication.
    ///</summary>
    public class NPCEngineServer : Singleton<NPCEngineServer>
    {

        [Tooltip("Relative to StreamingAssets folder")]
        public string modelsPath = "/.models/";

        [Tooltip("Relative to StreamingAssets folder")]
        public string npcEnginePath = "/.npc-engine/cli.exe";

        public bool runOnStart = true;
        public bool debug = false;
        public bool connectToExistingServer = false;
        private RequestSocket zmqClient;

        private int inferenceEngineProcessId = -1;

        private bool initialized = false;

        public bool Initialized
        {
            get { return initialized; }
        }

        Queue<Tuple<string, Action<string>>> taskQueue;


        IEnumerator InferenceEngineCommunicationCoroutine()
        {

            while (true)
            {
                while (taskQueue.Count == 0)
                {
                    yield return null;
                }
                Tuple<string, Action<string>> task = taskQueue.Dequeue();
                UnityEngine.Debug.Log(string.Format("SendMessage, {0}", task.Item1));

                while (!zmqClient.TrySendFrame(task.Item1))
                {
                    // UnityEngine.Debug.Log("SendMessage TrySend");
                    yield return null;
                }
                string reply;
                while (!zmqClient.TryReceiveFrameString(out reply))
                {
                    // UnityEngine.Debug.Log("SendMessage TryReceive");
                    yield return null;
                }
                task.Item2(reply);
            }
        }

        public ResultFuture<R> Run<P, R>(String methodName, P parameters)
        where P : new()
        {
            var result = new ResultFuture<R>();
            var request = new RPCRequstMessage<P>();
            request.method = methodName;
            request.parameters = parameters;
            this.SendMessage(
                request,
                (R reply) =>
                {
                    result.ResultFinishedCallback(reply);
                },
                (NPCEngineException error) =>
                {
                    result.ErrorCallback(error);
                }
            );
            return result;
        }

        private void SendMessage<P, R>(
            RPCRequstMessage<P> message,
            Action<R> replyCallback = null,
            Action<NPCEngineException> errorCallback = null
        ) where P : new()
        {
            string messageString = JsonConvert.SerializeObject(message);
            taskQueue.Enqueue(new Tuple<string, Action<string>>(
                    messageString,
                    (string reply) =>
                    {
                        UnityEngine.Debug.LogFormat("Received message: {0}", reply);
                        var response = JsonConvert.DeserializeObject<RPCResponseMessage<R>>(reply);
                        if (response.error.code != 0)
                        {
                            errorCallback(
                                new NPCEngineException(
                                    "Error code: "
                                    + response.error.code.ToString()
                                    + " Message: " + response.error.message
                                )
                            );
                            return;
                        }
                        replyCallback(response.result);
                    }
                )
            );
        }

        public void StartInferenceEngine()
        {
            if (!connectToExistingServer)
            {
                if (inferenceEngineProcessId == -1)
                {
                    Process myProcess = new Process();

                    if (debug)
                    {
                        myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        myProcess.StartInfo.CreateNoWindow = false;
                        myProcess.StartInfo.UseShellExecute = true;
                        myProcess.StartInfo.FileName = "CMD.EXE";
                        myProcess.StartInfo.Arguments = (
                            " /k "
                            + Application.streamingAssetsPath + npcEnginePath
                            + " run "
                            + " --models-path " + Application.streamingAssetsPath + modelsPath
                            + " --port 5555"
                        );
                        myProcess.EnableRaisingEvents = true;
                        myProcess.Start();
                    }
                    else
                    {
                        myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        myProcess.StartInfo.CreateNoWindow = true;
                        myProcess.StartInfo.UseShellExecute = false;
                        myProcess.StartInfo.FileName = Application.streamingAssetsPath + npcEnginePath;
                        myProcess.StartInfo.Arguments = (
                            " run "
                            + " --models-path " + Application.streamingAssetsPath + modelsPath
                            + " --port 5555"
                        );
                        myProcess.EnableRaisingEvents = true;
                        myProcess.Start();
                        ChildProcessTracker.AddProcess(myProcess);
                    }
                    inferenceEngineProcessId = myProcess.Id;
                    StartCoroutine("InferenceEngineHealthCheck");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Trying to start InferenceEngine that is already running");
                }
            }
        }

        IEnumerator InferenceEngineHealthCheck()
        {
            while (true && !connectToExistingServer)
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
            AsyncIO.ForceDotNet.Force();

            taskQueue = new Queue<Tuple<string, Action<string>>>();
            if (runOnStart)
            {
                StartInferenceEngine();
                ConnectToServer();
            }
        }

        public void ConnectToServer()
        {

            UnityEngine.Debug.Log("binding zmqClient");
            zmqClient = new RequestSocket();
            zmqClient.Connect("tcp://localhost:5555");
            UnityEngine.Debug.Log("binding successful");

            Action<string> initialize = (string response) =>
            {
                if (response != "OK")
                {
                    throw new NPCEngineException(string.Format("Status check failed: {0}", response));
                }
                initialized = true; // set initialized on successful response from server
            };
            var statusRequest = new RPCRequstMessage<List<String>>();
            statusRequest.method = "status";
            // Initialize 
            SendMessage(statusRequest, initialize);
        }



        private void OnDestroy()
        {
            StopCoroutine("InferenceEngineCommunicationCoroutine");
            StopCoroutine("InferenceEngineHealthCheck");
            zmqClient.Close();
            NetMQConfig.Cleanup(false);
        }

        private void Start()
        {
            StartCoroutine("InferenceEngineCommunicationCoroutine");
        }

    }
}