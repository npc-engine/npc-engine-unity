using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine.Server;
public class StatusChecker : MonoBehaviour
{

    public GameObject InitializedIndicator;
    public GameObject NotInitializedIndicator;
    public GameObject NotStartedIndicator;

    public List<Button> Interactables;

    public Button ConnectToServerButton;
    public Button StartServerButton;
    private void Start()
    {
        InitializedIndicator.SetActive(false);
        NotInitializedIndicator.SetActive(false);
        NotStartedIndicator.SetActive(true);
        ConnectToServerButton.interactable = true;
        StartServerButton.interactable = true;
        foreach (var button in Interactables)
        {
            button.interactable = false;
        }
    }

    public void StartServer()
    {
        NPCEngineServer.Instance.StartInferenceEngine();
        NPCEngineServer.Instance.ConnectToServer();

        InitializedIndicator.SetActive(false);
        NotInitializedIndicator.SetActive(true);
        NotStartedIndicator.SetActive(false);
        ConnectToServerButton.interactable = false;
        StartServerButton.interactable = false;
    }

    public void ConnectToExistingServer()
    {
        NPCEngineServer.Instance.connectToExistingServer = true;
        NPCEngineServer.Instance.ConnectToServer();

        InitializedIndicator.SetActive(false);
        NotInitializedIndicator.SetActive(true);
        NotStartedIndicator.SetActive(false);
        ConnectToServerButton.interactable = false;
        StartServerButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!InitializedIndicator.activeSelf)
        {
            if (NPCEngineServer.Instance.Initialized)
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
