using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using NPCEngine.Utility;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif


/// <summary>
/// Namespace containing generic RPC functionality.
///</summary>
namespace NPCEngine.RPC
{


    /// <summary>
    /// Class <c>NPCEngineManager</c> manages inference engine sidecart process lifetime and communication.
    ///</summary>
    public abstract class RPCBase : MonoBehaviour
    {

        public virtual string ServiceId { get { return ""; } }
        Queue<Request> taskQueue = new Queue<Request>();
        private RequestDispatcherImpl impl;
        private IEnumerator implCoroutine;

#if UNITY_EDITOR
        private EditorCoroutine editorCoroutine;
#endif


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
                        if (NPCEngineConfig.Instance.debug) UnityEngine.Debug.LogFormat("Received message: {0}", reply);
                        if (reply == null)
                        {
                            errorCallback(
                                new NPCEngineException(
                                    "Empty reply from inference engine"
                                )
                            );
                        }
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
                )
            );
        }

        public void Connect()
        {
            switch (NPCEngineConfig.Instance.serverType)
            {
                case ServerType.HTTP:
                    impl = new APICommunicatorHTTPImpl(NPCEngineConfig.Instance.serverAddress, ServiceId);
                    break;
                case ServerType.ZMQ:
                    impl = new APICommunicatorZMQImpl(NPCEngineConfig.Instance.serverAddress, ServiceId);
                    break;
                default:
                    throw new NPCEngineException("Unknown server type");
            }
            if (Application.isPlaying)
            {
                if (implCoroutine != null)
                    StopCoroutine(implCoroutine);
                implCoroutine = this.impl.DispatchRequestsCoroutine(taskQueue);
                StartCoroutine(implCoroutine);
            }
            else
            {
                UnityEngine.Debug.Log("RPCBase.Start()");
#if UNITY_EDITOR
                if (editorCoroutine != null)
                    EditorCoroutineUtility.StopCoroutine(editorCoroutine);
                editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(this.impl.DispatchRequestsCoroutine(taskQueue));
#endif
            }
        }

        public void Disconnect()
        {
            impl = null;
#if UNITY_EDITOR
            if (editorCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(editorCoroutine);
#else
            if (implCoroutine != null)
                StopCoroutine(implCoroutine);
#endif
            taskQueue.Clear();
        }

    }
}