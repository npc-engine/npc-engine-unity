using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine.Components;
using NPCEngine;

public class StatusChecker : MonoBehaviour
{

    public GameObject InitializedIndicator;
    public GameObject NotInitializedIndicator;
    public GameObject NotStartedIndicator;

    public List<Button> Interactables;

    public Button StartServerButton;
    private void Start()
    {
        InitializedIndicator.SetActive(false);
        NotInitializedIndicator.SetActive(false);
        NotStartedIndicator.SetActive(true);
        StartServerButton.interactable = true;
        foreach (var button in Interactables)
        {
            button.interactable = false;
        }
    }

    public void StartServer()
    {
        NPCEngineManager.Instance.StartInferenceEngine();

        InitializedIndicator.SetActive(false);
        NotInitializedIndicator.SetActive(true);
        NotStartedIndicator.SetActive(false);
        StartServerButton.interactable = false;
    }

    public void ConnectToExistingServer()
    {
        NPCEngineConfig.Instance.connectToExistingServer = true;

        InitializedIndicator.SetActive(false);
        NotInitializedIndicator.SetActive(true);
        NotStartedIndicator.SetActive(false);
        StartServerButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!InitializedIndicator.activeSelf)
        {
            if (NPCEngineManager.Instance.Initialized)
            {
                InitializedIndicator.SetActive(true);
                NotInitializedIndicator.SetActive(false);
                foreach (var button in Interactables)
                {
                    button.interactable = true;
                }
            }
        }
    }
}
