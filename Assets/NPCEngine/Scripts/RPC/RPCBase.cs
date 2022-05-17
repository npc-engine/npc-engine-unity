using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NPCEngine.Utility;


/// <summary>
/// Namespace containing generic RPC functionality.
///</summary>
namespace NPCEngine.RPC
{

    /// <summary>
    /// Class <c>NPCEngineManager</c> manages inference engine sidecart process lifetime and communication.
    ///</summary>
    public abstract class RPCBase<T> : Singleton<T> where T : MonoBehaviour
    {
        protected virtual string ServiceId { get { return ""; } }
        private RequestDispatcherImpl impl;
        private IEnumerator implCoroutine;

        Queue<Request> taskQueue;

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
                        if (RPCConfig.Instance.debug) UnityEngine.Debug.LogFormat("Received message: {0}", reply);
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


        private void Awake()
        {
            switch (RPCConfig.Instance.serverType)
            {
                case ServerType.HTTP:
                    impl = new APICommunicatorHTTPImpl(RPCConfig.Instance.serverAddress, ServiceId, RPCConfig.Instance.debug);
                    break;
                case ServerType.ZMQ:
                    impl = new APICommunicatorZMQImpl(RPCConfig.Instance.serverAddress, ServiceId, RPCConfig.Instance.debug);
                    break;
                default:
                    throw new NPCEngineException("Unknown server type");
            }
            taskQueue = new Queue<Request>();
            implCoroutine = this.impl.DispatchRequestsCoroutine(taskQueue);
            StartCoroutine(implCoroutine);
        }

        private void OnDestroy()
        {
            StopCoroutine(implCoroutine);
        }
    }
}