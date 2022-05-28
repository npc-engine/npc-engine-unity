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

        public override string ServiceId { get { return "ClassificationAPI"; } }

        [Serializable()]
        private class ClassifyMessage
        {
            public List<string> texts;
        }

        public IEnumerator Classify(string query, List<string> context, Action<List<List<float>>> outputCallback)
        {
            var message = new ClassifyMessage { texts = context };
            var result = this.Run<ClassifyMessage, List<List<float>>>("classify", message);

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }


    }
}