using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using NPCEngine;
using NPCEngine.Components;
using NPCEngine.API;

namespace NPCEngine
{

    [CustomEditor(typeof(NPCEngineConfig))]
    public class NPCEngineConfigEditor : Editor
    {
        NPCEngineConfig config;

        public override void OnInspectorGUI()
        {
            if (config == null)
            {
                config = (NPCEngineConfig)target;
            }
            base.DrawDefaultInspector();
            if (config.services == null)
            {
                config.services = GetServicesManually();
            }
            DrawServicesInspector();
            // refresh services list
            if (GUILayout.Button("Refresh", EditorStyles.miniButtonLeft))
            {
                config.services = GetServicesManually();
            }
        }

        private void DrawServicesInspector()
        {


            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Box("Service name", EditorStyles.boldLabel, GUILayout.MinWidth(10));
            foreach (var service in config.services)
            {
                GUILayout.Box(service.name.Length > 50 ? service.name.Substring(0, 50) + "..." : service.name, EditorStyles.label, GUILayout.MinWidth(10));
            }
            GUILayout.EndVertical();
            VerticalLine(Color.black);
            GUILayout.BeginVertical();
            GUILayout.Box("Service type", EditorStyles.boldLabel);
            foreach (var service in config.services)
            {
                GUILayout.Box(service.type, EditorStyles.label, GUILayout.MinWidth(10));
            }
            GUILayout.EndVertical();
            VerticalLine(Color.black);
            GUILayout.BeginVertical();
            GUILayout.Box("Load on start", EditorStyles.boldLabel);
            foreach (var service in config.services)
            {
                if (GUILayout.Toggle(service.start, ""))
                {
                    service.start = true;
                }
                else
                {
                    service.start = false;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

        }
        private List<ServiceConfigDescriptor> GetServicesManually()
        {
            // Get all directories in models folder
            string[] directories = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath));
            List<ServiceConfigDescriptor> services = new List<ServiceConfigDescriptor>();
            foreach (string directory in directories)
            {
                ServiceConfigDescriptor service = new ServiceConfigDescriptor();
                service.name = Path.GetFileName(directory);
                string[] lines = System.IO.File.ReadAllLines(Path.Combine(directory, "config.yml"));
                foreach (var line in lines)
                {
                    if (line.StartsWith("type:"))
                    {
                        service.type = line.Replace("type: ", "");
                    }
                    else if (line.StartsWith("model_type:"))
                    {
                        service.type = line.Replace("model_type: ", "");
                    }
                }
                services.Add(service);
            }
            // Copy start setting from existing config
            if (NPCEngineConfig.Instance.services != null)
            {
                foreach (var service in services)
                {
                    foreach (var existingService in NPCEngineConfig.Instance.services)
                    {
                        if (service.name == existingService.name)
                        {
                            service.start = existingService.start;
                        }
                    }
                }
            }
            return services;
        }

        static void VerticalLine(Color color)
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
    }
}