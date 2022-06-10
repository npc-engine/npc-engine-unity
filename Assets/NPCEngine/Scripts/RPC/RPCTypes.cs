using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NPCEngine
{

    /// <summary>
    /// General NPC Engine exception.
    /// </summary>
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


    /// <summary>
    /// Future result of RPC call.
    /// </summary>
    /// <typeparam name="ReturnType"></typeparam>
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

namespace NPCEngine.RPC
{
    public class Request : Tuple<string, Action<string>>
    {
        public Request(string key, Action<string> action) : base(key, action)
        {
        }
    }

    /// <summary>
    /// JSON RPC request message.
    /// </summary>
    /// <typeparam name="ParametersType"></typeparam>
    [Serializable()]
    public class RPCRequestMessage<ParametersType>
    where ParametersType : new()
    {
        public int id = 0;
        public String jsonrpc = "2.0";
        public String method;

        [JsonProperty(PropertyName = "params")]
        public ParametersType parameters;

        public RPCRequestMessage()
        {
            if (!typeof(ParametersType).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(ParametersType))))
                throw new InvalidOperationException("A serializable argument type is required");
            parameters = new ParametersType();
            method = "";
        }
    }

    /// <summary>
    /// JSON RPC response message.
    /// </summary>
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


    /// <summary>
    /// RPC response error.
    /// </summary>
    [Serializable()]
    public class RPCResponseError
    {
        public int code = 0;
        public String message = "";
    }

    /// <summary>
    /// Transport layer for RPC enum.
    /// </summary>
    public enum ServerType
    {
        HTTP,
        ZMQ
    }
}