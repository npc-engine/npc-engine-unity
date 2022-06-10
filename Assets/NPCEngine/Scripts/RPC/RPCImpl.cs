using System.Diagnostics;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine.Networking;

namespace NPCEngine.RPC
{
    /// <summary>
    /// Abstract class for RPC loop.
    /// </summary>
    public abstract class RequestDispatcherImpl
    {
        protected string id;
        protected string address;
        public RequestDispatcherImpl(string address, string id)
        {
            this.address = address;
            this.id = id;
        }
        public abstract IEnumerator DispatchRequestsCoroutine(Queue<Request> taskQueue);
    }

    /// <summary>
    /// RPC loop implementation for ZMQ protocol.
    /// </summary>
    public class APICommunicatorZMQImpl : RequestDispatcherImpl
    {
        private RequestSocket zmqClient;

        public APICommunicatorZMQImpl(string address, string id) : base(address, id)
        {
            if (NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.Log(String.Format("ZMQ Service client {0} connecting to address {1}", id, address));
            zmqClient = new RequestSocket();
            zmqClient.Options.Identity =
                        Encoding.Unicode.GetBytes(id);
            zmqClient.Connect("tcp://" + address);
            if (NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.Log(String.Format("ZMQ Service client {0} connected to address {1} successfully", id, address));
        }

        ~APICommunicatorZMQImpl()
        {
            zmqClient.Close();
            NetMQConfig.Cleanup(false);
        }

        public override IEnumerator DispatchRequestsCoroutine(Queue<Request> taskQueue)
        {
            while (true)
            {
                while (taskQueue.Count == 0)
                {
                    yield return null;
                }
                Tuple<string, Action<string>> task = taskQueue.Dequeue();
                if (NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.Log(string.Format("SendMessage, {0}", task.Item1));

                while (!zmqClient.TrySendFrame(task.Item1))
                {
                    yield return null;
                }
                string reply;
                while (!zmqClient.TryReceiveFrameString(out reply))
                {
                    yield return null;
                }
                task.Item2(reply);
            }
        }
    }

    /// <summary>
    /// RPC loop implementation for HTTP protocol.
    /// </summary>
    public class APICommunicatorHTTPImpl : RequestDispatcherImpl
    {

        public APICommunicatorHTTPImpl(string address, string id) : base(address, id)
        {

        }

        UnityWebRequest CreateUnityWebRequest(string url, string param)
        {
            UnityWebRequest requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            byte[] bytes = Encoding.UTF8.GetBytes(param);
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            requestU.SetRequestHeader("Content-Type", "application/json");
            requestU.uploadHandler = uH;
            DownloadHandler dH = new DownloadHandlerBuffer();
            requestU.downloadHandler = dH;
            return requestU;
        }

        public override IEnumerator DispatchRequestsCoroutine(Queue<Request> taskQueue)
        {
            while (true)
            {
                while (taskQueue.Count == 0)
                {
                    yield return null;
                }
                Tuple<string, Action<string>> task = taskQueue.Dequeue();
                UnityWebRequest request = null;
                UnityWebRequestAsyncOperation requestOp = null;
                try
                {

                    if (NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.Log(string.Format("SendMessage, {0}", task.Item1));
                    request = CreateUnityWebRequest(this.id != "" ? ("http://" + address + "/" + this.id) : "http://" + address, task.Item1);
                    requestOp = request.SendWebRequest();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                while (requestOp != null && !requestOp.isDone)
                {
                    yield return null;
                }
                string reply;

                try
                {
                    if (request.error != null)
                    {
                        reply = request.error;
                    }
                    else
                    {
                        reply = request.downloadHandler.text;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                    reply = "";
                }
                if(request != null)
                {
                    request.Dispose();
                }
                task.Item2(reply);
            }
        }
    }
}