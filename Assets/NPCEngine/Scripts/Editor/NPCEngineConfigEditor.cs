using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using NPCEngine.Components;

namespace NPCEngine
{

    [CustomEditor(typeof(NPCEngineConfig))]
    public class NPCEngineConfigEditor : Editor
    {
        NPCEngineConfig config;
        bool showDownload = false;
        private string downloadModelID;

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
            showDownload = EditorGUILayout.Foldout(showDownload, "Add New Services");
            if (showDownload)
            {
                DrawAddNewService();
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
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            VerticalLine(Color.black);
            GUILayout.BeginVertical();
            GUILayout.Box("Service type", EditorStyles.boldLabel);
            foreach (var service in config.services)
            {
                GUILayout.Box(service.type, EditorStyles.label, GUILayout.MinWidth(10));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            if(NPCEngineManager.Instance.InferenceEngineRunning) GUI.enabled = false;

            VerticalLine(Color.black);

            LoadOnStartToggles();

            VerticalLine(Color.black);

            OpenConfigButtons();

            VerticalLine(Color.black);

            RemoveButtons();

            GUI.enabled = true;

            GUILayout.EndHorizontal();

        }

        private void LoadOnStartToggles()
        {
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
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }


        private void RemoveButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.Box("", EditorStyles.boldLabel);
            foreach (var service in config.services)
            {
                if (GUILayout.Button("Remove", EditorStyles.miniButtonLeft))
                {
                    bool decision = EditorUtility.DisplayDialog(
                        "Remove Service", // title
                        String.Format("Are you sure want to remove all {0} files?", service.name), // description
                        "Yes", // OK button
                        "No" // Cancel button
                    );
                    if (decision)
                        clearFolder(service.path);
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }


        private void OpenConfigButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.Box("", EditorStyles.boldLabel);
            foreach (var service in config.services)
            {
                if (GUILayout.Button("Edit Config", EditorStyles.miniButtonLeft))
                {
                    string path = "";
                    string default_run = "";
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                       default_run = "start";
                       path = "\"\" \"" + Path.Combine(service.path, "config.yml").Replace("/", "\\") + "\"";
                    }
                    else if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        default_run = "open";
                        path = "\"" + Path.Combine(service.path, "config.yml").Replace("\\", "/") + "\"";
                    }else if (Application.platform == RuntimePlatform.LinuxEditor)
                    {
                        default_run = "xdg-open";
                        path = "\"" + Path.Combine(service.path, "config.yml").Replace("\\", "/") + "\"";
                    }
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "CMD.EXE",
                        Arguments = "/k \"" + default_run + " " +  path + "\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                    };
                    System.Diagnostics.Process.Start(processStartInfo);
                    
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
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
                service.path = directory;
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

        private void DrawAddNewService()
        {
            GUIStyle wrappedLabel = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                wordWrap = true
            };
            GUILayout.Label("Download Service", EditorStyles.boldLabel);
            if (GUILayout.Button("To find services, go to https://huggingface.co/models?other=npc-engine"
            + " and enter model ID you want to download below", wrappedLabel))
            {
                Application.OpenURL("https://huggingface.co/models?other=npc-engine");
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Model ID:", GUILayout.Width(80));
            downloadModelID = GUILayout.TextField(downloadModelID);

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Download", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(80)))
            {
                if (downloadModelID.Length > 0)
                {
                    NPCEngineManager.Instance.DownloadModel(downloadModelID);
                }
            }

            GUILayout.Label("Import Service", EditorStyles.boldLabel);
            GUILayout.Label("Or you can import services from your computer:", wrappedLabel);
            if (GUILayout.Button("Import", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(80)))
            {
                string path = EditorUtility.OpenFolderPanel("Select model", ".", "");
                if (path.Length > 0)
                {
                    var config_file = Path.Combine(path, "config.yml");
                    if (!File.Exists(config_file))
                    {
                        EditorUtility.DisplayDialog("Error", "Service config file not found. Not a valid service.", "OK");
                        return;
                    }
                    copyFolder(path, Path.Combine(Application.streamingAssetsPath, NPCEngineConfig.Instance.modelsPath));
                }
            }

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

        private void clearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
            dir.Delete();
        }

        private void copyFolder(string folder, string to_path, bool recursive = true)
        {
            DirectoryInfo dir = new DirectoryInfo(folder);

            to_path = Path.Combine(to_path, dir.Name);
            if (!dir.Exists)
            {
                UnityEngine.Debug.LogError("Folder " + folder + " does not exist");
                return;
            }
            if (!Directory.Exists(to_path))
            {
                Directory.CreateDirectory(to_path);
            }
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.CopyTo(Path.Combine(to_path, fi.Name), true);
            }
            if (recursive)
            {
                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    copyFolder(di.FullName, Path.Combine(to_path, di.Name), recursive);
                }
            }
        }

    }
}