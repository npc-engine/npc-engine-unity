using System;
using System.Collections;
using System.Collections.Generic;
using NPCEngine.RPC;


namespace NPCEngine.API
{


    /// <summary>
    /// Classification services RPC Interface.
    ///</summary>
    public class Classification : RPCBase
    {
        void Awake()
        {
            if(serviceId == "")
            {
                serviceId = "ClassificationAPI";
            }
        }

        [Serializable()]
        private class ClassifyMessage
        {
            public List<string> texts;
        }

        public IEnumerator Classify(string query, List<string> context, Action<List<List<float>>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            var message = new ClassifyMessage { texts = context };
            yield return this.Run<ClassifyMessage, List<List<float>>>("classify", message, outputCallback, errorCallback);

        }


    }
}