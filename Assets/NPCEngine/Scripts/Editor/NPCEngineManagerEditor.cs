
#if UNITY_EDITOR
using System.Net.Mime;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NPCEngine.Utility;
using NPCEngine.Components;
using NPCEngine.API;
using Unity.EditorCoroutines.Editor;


[CustomEditor(typeof(NPCEngineManager))]
public class NPCEngineManagerEditor : Editor
{
    private double time;

    private bool[] serviceShow;

    NPCEngineManager manager;


    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        manager = (NPCEngineManager)target;

        if (!manager.InferenceEngineRunning)
        {

            GUILayout.Space(10);
            GUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Start Inference Engine", GUILayout.Width(200), GUILayout.Height(35)))
            {
                manager.StartInferenceEngine();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
        else
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            DrawServicesInspector();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Stop Inference Engine", GUILayout.Width(200), GUILayout.Height(35)))
            {
                manager.StopInferenceEngine();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
    }

    private void DrawServicesInspector()
    {

        if (manager.Services == null)
        {
            GUILayout.Box("Loading services metadata...", GUILayout.Width(300), GUILayout.Height(35));
        }
        else
        {
            DrawServicesTable();
        }

    }

    private void DrawServicesTable()
    {
        GUILayout.BeginVertical();


        GUILayout.Box("Service name", EditorStyles.boldLabel, GUILayout.MinWidth(10));
        int i = 0;
        if (serviceShow == null || serviceShow.Length != manager.Services.Count)
        {
            serviceShow = new bool[manager.Services.Count];
        }

        foreach (var service in manager.Services)
        {
            if (!serviceShow[i])
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                serviceShow[i] = EditorGUILayout.Foldout(
                    serviceShow[i],
                    service.id.Length > 50 ?
                        service.id.Substring(0, 50) + "..." :
                        service.id
                );
                ServiceStatusBox(manager.ServiceStatuses[i]);
                StatusButton(i);
                GUILayout.EndHorizontal();
            }
            else
            {
                serviceShow[i] = EditorGUILayout.Foldout(
                    serviceShow[i],
                    service.id.Length > 50 ?
                        service.id.Substring(0, 50) + "..." :
                        service.id
                );
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUIStyle wrappedLabel = new GUIStyle(GUI.skin.GetStyle("label"))
                {
                    wordWrap = true
                };
                GUILayout.Box("Service type: ", EditorStyles.boldLabel, GUILayout.MinWidth(10));
                GUILayout.Box(service.service, wrappedLabel, GUILayout.MinWidth(10));
                GUILayout.Box("Service API: ", EditorStyles.boldLabel, GUILayout.MinWidth(10));
                GUILayout.Box(service.api_name, wrappedLabel, GUILayout.MinWidth(10));
                GUILayout.Box("Service description: ", EditorStyles.boldLabel, GUILayout.MinWidth(10));
                GUILayout.Box(service.service_short_description, wrappedLabel, GUILayout.MinWidth(10));
                GUILayout.Box("Model description: ", EditorStyles.boldLabel, GUILayout.MinWidth(10));
                GUILayout.Box(service.readme, wrappedLabel, GUILayout.MinWidth(10));

                GUILayout.Box("Status:", EditorStyles.boldLabel, GUILayout.MaxWidth(60));
                ServiceStatusBox(manager.ServiceStatuses[i]);
                GUILayout.Space(10f);
                GUILayout.Button("Stop", GUILayout.MaxWidth(80));
                GUILayout.EndVertical();
            }
            i += 1;

        }

        GUILayout.EndVertical();
    }

    void VerticalLine(Color color)
    {
        GUIStyle horizontalLine;
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        // horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedWidth = 1;
        horizontalLine.stretchHeight = true;
        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;
    }


    void ServiceStatusBox(ServiceStatus status)
    {
        switch (status)
        {
            case ServiceStatus.RUNNING:
                var prev_color = GUI.color;
                GUI.color = Color.green;
                GUILayout.Box("RUNNING", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
                GUI.color = prev_color;
                break;
            case ServiceStatus.STOPPED:
                var prev_color2 = GUI.color;
                GUI.color = Color.red;
                GUILayout.Box("STOPPED", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
                GUI.color = prev_color2;
                break;
            case ServiceStatus.STARTING:
                var prev_color3 = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Box("STARTING", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
                GUI.color = prev_color3;
                break;
            case ServiceStatus.ERROR:
                var prev_color4 = GUI.color;
                GUI.color = Color.red;
                GUILayout.Box("ERROR", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
                GUI.color = prev_color4;
                break;
            default:
                var prev_color5 = GUI.color;
                GUI.color = Color.grey;
                GUILayout.Box("UNKNOWN", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
                GUI.color = prev_color5;
                break;
        }
    }

    void StatusButton(int serviceIndex)
    {
        switch (manager.ServiceStatuses[serviceIndex])
        {
            case ServiceStatus.RUNNING:
                if (GUILayout.Button("Stop", GUILayout.MaxWidth(80)))
                {
                    CoroutineUtility.StartCoroutine(
                        NPCEngineManager.Instance.GetAPI<Control>().StopService(manager.Services[serviceIndex].id), manager, "EditorStopService"
                    );
                }
                break;
            case ServiceStatus.STOPPED:
                if (GUILayout.Button("Start", GUILayout.MaxWidth(80)))
                {
                    CoroutineUtility.StartCoroutine(
                        NPCEngineManager.Instance.GetAPI<Control>().StartService(manager.Services[serviceIndex].id), manager, "EditorStartService"
                    );
                }
                break;
            default:
                GUI.enabled = false;
                GUILayout.Button("Start", GUILayout.MaxWidth(80));
                GUI.enabled = true;
                break;
        }
    }

}

#endif
