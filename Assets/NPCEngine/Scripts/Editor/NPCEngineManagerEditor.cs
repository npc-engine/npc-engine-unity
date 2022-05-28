
#if UNITY_EDITOR
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NPCEngine;
using NPCEngine.Components;
using NPCEngine.API;
using Unity.EditorCoroutines.Editor;


[CustomEditor(typeof(NPCEngineManager))]
public class NPCEngineManagerEditor : Editor
{
    private List<ServiceMetadata> services;
    private List<ServiceStatus> serviceStatuses;
    private double time;

    private bool[] serviceShow;

    EditorCoroutine statusCoroutine;
    EditorCoroutine servicesCoroutine;


    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        NPCEngineManager manager = (NPCEngineManager)target;

        if (!manager.InferenceEngineRunning)
        {

            GUILayout.Space(10);
            GUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Start Inference Engine", GUILayout.Width(200), GUILayout.Height(35)))
            {
                if (servicesCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(servicesCoroutine);
                }
                servicesCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(UpdateServices());

                if (statusCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(statusCoroutine);
                }
                statusCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(UpdateServiceStatuses());
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
                if (statusCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(statusCoroutine);
                }
                if (servicesCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(servicesCoroutine);
                }
                serviceStatuses = null;
                services = null;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
    }

    private void DrawServicesInspector()
    {

        if (services == null)
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
        if (serviceShow == null || serviceShow.Length != services.Count)
        {
            serviceShow = new bool[services.Count];
        }
        if (serviceStatuses == null || serviceStatuses.Count != services.Count)
        {
            serviceStatuses = new List<ServiceStatus>();
        }
        foreach (var service in services)
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
                ServiceStatusBox(serviceStatuses[i]);
                GUILayout.Button("Stop", GUILayout.MaxWidth(40));
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
                ServiceStatusBox(serviceStatuses[i]);
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

    public IEnumerator UpdateServiceStatuses()
    {
        UnityEngine.Debug.Log("Updating service statuses...");
        while (true)
        {
            if (services != null)
            {
                if (serviceStatuses == null || serviceStatuses.Count != services.Count)
                {
                    serviceStatuses = new List<ServiceStatus>();
                    for (int i = 0; i < services.Count; i++)
                    {
                        serviceStatuses.Add(new ServiceStatus());
                    }
                }
                for (int i = 0; i < services.Count; i++)
                {
                    ServiceStatus status = serviceStatuses[i];
                    serviceStatuses[i] = status;
                    yield return NPCEngineManager.Instance.GetAPI<Control>().GetServiceStatus(services[i].id, (ServiceStatus s) =>
                    {
                        serviceStatuses[i] = s;
                    });
                }
            }
            yield return new EditorWaitForSeconds(4);
        }
    }

    public IEnumerator UpdateServices()
    {
        UnityEngine.Debug.Log("Updating services...");
        while (true)
        {
            yield return NPCEngineManager.Instance.GetAPI<Control>().GetServicesMetadata((List<ServiceMetadata> s) =>
            {
                services = s;
            });
            yield return new EditorWaitForSeconds(4);
        }
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

}

#endif
