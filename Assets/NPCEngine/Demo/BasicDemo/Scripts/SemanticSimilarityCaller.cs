using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine;
using NPCEngine.API;
using NPCEngine.Components;

public class SemanticSimilarityCaller : MonoBehaviour
{
    public InputField prompt1;
    public InputField prompt2;
    public Text outputLabel;

    public void CallSemanticSimilarity()
    {
        StartCoroutine(NPCEngineManager.Instance.GetAPI<SemanticQuery>().Compare(prompt1.text, new List<string> { prompt2.text }, (result) =>
        {
            outputLabel.text = result[0].ToString();
        }));
    }


}
