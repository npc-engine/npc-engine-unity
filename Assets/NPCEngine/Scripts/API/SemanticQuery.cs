using System;
using System.Collections;
using System.Collections.Generic;
using NPCEngine.RPC;


namespace NPCEngine.API
{


    /// <summary>
    /// Static class <c>SemanticQuery</c> provides remote procedure calls 
    /// to inference engine's Semantic similarity model.
    /// Prefer predefining queries via <c>PredefineQuery</c> and then using
    /// <c>QueryPredefined</c> instead of directly using <c>Query</c>
    ///</summary>
    public class SemanticQuery : RPCBase
    {

        void Awake()
        {
            if (this.serviceId == "")
            {
                this.serviceId = "SimilarityAPI";
            }
        }

        [Serializable()]
        private class QueryMessage
        {
            public string query;
            public List<string> context;
        }

        public IEnumerable Cache(List<string> queryIds, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, bool>("compare", queryIds, (result) => { }, errorCallback);
        }

        //Compare in a coroutine   
        public IEnumerator Compare(string query, List<string> context, Action<List<float>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            var message = new QueryMessage { query = query, context = context };
            yield return this.Run<QueryMessage, List<float>>("compare", message, outputCallback, errorCallback);
        }
    }
}