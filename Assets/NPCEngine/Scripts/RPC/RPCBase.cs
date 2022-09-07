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

    [ExecuteAlways]
    /// <summary>
    /// Class <c>NPCEngineManager</c> manages inference engine sidecart process lifetime and communication.
    ///</summary>
    public abstract class RPCBase : MonoBehaviour
    {

        public string serviceId = "";

        private RequestDispatcherImpl impl;


        public IEnumerator Run<P, R>(String methodName, P parameters, Action<R> resultCallback, Action<NPCEngineException> errorCallback)
        where P : new()
        {
            var request = new RPCRequestMessage<P>();
            request.method = methodName;
            request.parameters = parameters;
            yield return this.SendMessage(
                request,
                resultCallback,
                errorCallback
            );
        }

        private IEnumerator SendMessage<P, R>(
            RPCRequestMessage<P> message,
            Action<R> replyCallback = null,
            Action<NPCEngineException> errorCallback = null
        ) where P : new()
        {
            string messageString = JsonConvert.SerializeObject(message);
            yield return impl.DispatchRequestCoroutine(new Request(
                    messageString,
                    (string reply) =>
                    {
                        if (NPCEngineConfig.Instance.debugLogs) UnityEngine.Debug.LogFormat("Received message: {0}", reply);
                        try
                        {   RPCResponseMessage<R> response;
                            try{
                                response = JsonConvert.DeserializeObject<RPCResponseMessage<R>>(reply);
                            } catch (Exception){
                                Debug.Log("reply: " + reply);
                                return;
                            }
                            if (response != null)
                            {
                                if (response.error.code != 0)
                                {
                                    if (errorCallback != null)
                                        errorCallback(
                                            new NPCEngineException(
                                                "Error code: "
                                                + response.error.code.ToString()
                                                + " Message: " + response.error.data.type,
                                                response.error.data.message + "\nArgs: " + String.Join(", ", response.error.data.args)
                                            )
                                        );
                                    else
                                    {
                                        Debug.LogErrorFormat(
                                            "Error code: "
                                            + response.error.code.ToString()
                                            + " Message: " + response.error.message
                                            + " Data: " + response.error.data
                                        );
                                    }
                                    return;
                                }
                                if(replyCallback != null)
                                {
                                    replyCallback(response.result);
                                }
                            }
                            else
                            {
                                if(errorCallback != null)
                                {
                                    errorCallback(
                                        new NPCEngineException(
                                            "Empty reply from inference engine"
                                        )
                                    );
                                }
                                else
                                {
                                    Debug.LogError(
                                        "Empty reply from inference engine"
                                    );
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (errorCallback != null)
                                errorCallback(
                                        new NPCEngineException(
                                            "Error: " + e
                                        )
                                    );
                            else
                            {
                                Debug.LogError(
                                    e
                                );
                            }
                            return;
                        }
                    }
                )
            );
        }

        public void UpdateServerType()
        {
            impl = null;
            switch (NPCEngineConfig.Instance.serverType)
            {
                case ServerType.ZMQ:
                    impl = new APICommunicatorZMQImpl(NPCEngineConfig.Instance.serverAddress, serviceId);
                    break;
                case ServerType.HTTP:
                    impl = new APICommunicatorHTTPImpl(NPCEngineConfig.Instance.serverAddress, serviceId);
                    break;
                default:
                    throw new NPCEngineException("Unknown server type");
            }
        }

        void OnEnable()
        {
            UpdateServerType();
        }
    }
}