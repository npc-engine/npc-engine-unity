using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;

namespace NPCEngine.RPC
{

    public abstract class RequestDispatcherImpl
    {
        protected string id;
        protected string address;
        protected bool debug = false;
        public RequestDispatcherImpl(string address, string id, bool debug = false)
        {
            this.address = address;
            this.id = id;
            this.debug = debug;
        }
        public abstract IEnumerator DispatchRequestsCoroutine(Queue<Request> taskQueue);
    }

    public class APICommunicatorZMQImpl : RequestDispatcherImpl
    {
        private RequestSocket zmqClient;

        public APICommunicatorZMQImpl(string address, string id, bool debug = false) : base(address, id, debug)
        {
            if (debug) UnityEngine.Debug.Log(String.Format("ZMQ Service client {0} connecting to address {1}", id, address));
            zmqClient = new RequestSocket();
            zmqClient.Options.Identity =
                        Encoding.Unicode.GetBytes(id);
            zmqClient.Connect("tcp://" + address);
            if (debug) UnityEngine.Debug.Log(String.Format("ZMQ Service client {0} connected to address {1} successfully", id, address));
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
                if (debug) UnityEngine.Debug.Log(string.Format("SendMessage, {0}", task.Item1));

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

    public class APICommunicatorHTTPImpl : RequestDispatcherImpl
    {

        public APICommunicatorHTTPImpl(string address, string id, bool debug = false) : base(address, id, debug)
        {

        }

        public override IEnumerator DispatchRequestsCoroutine(Queue<Request> taskQueue)
        {
            throw new NotImplementedException();
        }
    }
}