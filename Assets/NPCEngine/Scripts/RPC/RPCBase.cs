using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NPCEngine.Utility;



/// <summary>
/// Namespace containing generic RPC functionality.
///</summary>
namespace NPCEngine.RPC
{

    [ExecuteAlways]
    /// <summary>
    /// Class <c>NPCEngineManager</c> manages inference engine sidecart process lifetime and communication.
    ///</summary>
    public abstract class RPCBase : MonoBehaviour
    {

        public string serviceId = "";

        Queue<Request> taskQueue = new Queue<Request>();
        private RequestDispatcherImpl impl;


        public ResultFuture<R> Run<P, R>(String methodName, P parameters)
        where P : new()
        {
            var result = new ResultFuture<R>();
            var request = new RPCRequestMessage<P>();
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
            RPCRequestMessage<P> message,
            Action<R> replyCallback = null,
            Action<NPCEngineException> errorCallback = null
        ) where P : new()
        {
            string messageString = JsonConvert.SerializeObject(message);
            taskQueue.Enqueue(new Request(
                    messageString,
                    (string reply) =>
                    {
                        if (NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.LogFormat("Received message: {0}", reply);
                        try
                        {
                            var response = JsonConvert.DeserializeObject<RPCResponseMessage<R>>(reply);
                            if (response != null)
                            {
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
                            else
                            {
                                errorCallback(
                                    new NPCEngineException(
                                        "Empty reply from inference engine"
                                    )
                                );
                            }
                        }
                        catch (Exception)
                        {
                            errorCallback(
                                    new NPCEngineException(
                                        "Error: " + reply
                                    )
                                );
                            return;
                        }
                    }
                )
            );
        }

        void OnEnable()
        {
            switch (NPCEngineConfig.Instance.serverType)
            {
                case ServerType.HTTP:
                    impl = new APICommunicatorHTTPImpl(NPCEngineConfig.Instance.serverAddress, serviceId);
                    break;
                case ServerType.ZMQ:
                    impl = new APICommunicatorZMQImpl(NPCEngineConfig.Instance.serverAddress, serviceId);
                    break;
                default:
                    throw new NPCEngineException("Unknown server type");
            }
            CoroutineUtility.StartCoroutine(this.impl.DispatchRequestsCoroutine(taskQueue), this, "implCoroutine");
        }

        void OnDisable()
        {
            impl = null;
            if(!Application.isPlaying)
            {
                var prevActive = CoroutineUtility.Instance.gameObject.activeSelf;
                if(!prevActive)
                    CoroutineUtility.Instance.gameObject.SetActive(true);
                CoroutineUtility.StopCoroutine("implCoroutine", this);
                if(!prevActive)
                    CoroutineUtility.Instance.gameObject.SetActive(false);
            }
            taskQueue.Clear();
        }
    }
}