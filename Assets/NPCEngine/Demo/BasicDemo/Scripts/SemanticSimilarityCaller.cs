using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine;
using NPCEngine.API;
using NPCEngine.Utility;

public class SemanticSimilarityCaller : MonoBehaviour
{
    public InputField prompt1;
    public InputField prompt2;
    public Text outputLabel;
    ResultFuture<List<float>> result;

    private void Update()
    {
        if (result != null && result.ResultReady)
        {
            outputLabel.text = result.Result[0].ToString();
            result = null;
        }
    }

    public void CallSemanticSimilarity()
    {
        result = SemanticQuery.Compare(prompt1.text, new List<string> { prompt2.text });
    }


}
