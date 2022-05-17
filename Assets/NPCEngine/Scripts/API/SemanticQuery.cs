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
    public class SemanticQuery : RPCBase<SemanticQuery>
    {

        [Serializable()]
        private class QueryMessage
        {
            public string query;
            public List<string> context;
        }


        public ResultFuture<List<float>> Compare(string query, List<string> context)
        {
            var message = new QueryMessage { query = query, context = context };
            return this.Run<QueryMessage, List<float>>("compare", message);
        }


        public void Cache(List<string> queryIds)
        {
            this.Run<List<string>, bool>("compare", queryIds);
        }

        //Compare in a coroutine   
        public IEnumerator CompareCoroutine(string query, List<string> context, Action<List<float>> outputCallback)
        {
            var message = new QueryMessage { query = query, context = context };
            var result = this.Run<QueryMessage, List<float>>("compare", message);

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }


    }
}