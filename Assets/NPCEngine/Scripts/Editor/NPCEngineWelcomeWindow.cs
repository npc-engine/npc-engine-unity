
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Net;
using System;
using System.Diagnostics;


#if UNITY_EDITOR
[InitializeOnLoad]
public class NPCEngineWelcomeWindow : EditorWindow
{

    public static Vector2 windowSize = new Vector2(800, 700);



    [MenuItem("Tools/NPC Engine/Show Welcome Screen")]
    private static void InitWindow()
    {
        var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        GetWindowWithRect<NPCEngineWelcomeWindow>(new Rect(screenCenter, windowSize), true, "Welcome to NPC Engine Unity", true);
        headerStyle.fontSize = 22;
        headerStyle.normal.textColor = Color.white;
        descriptionStyle.fontSize = 14;
        descriptionStyle.normal.textColor = Color.white;

        logo = (Texture2D)Resources.Load("settings", typeof(Texture2D));
        githubLogo = (Texture2D)Resources.Load("github", typeof(Texture2D));
        docsLogo = (Texture2D)Resources.Load("file-text", typeof(Texture2D));
    }

    private static GUIStyle headerStyle = new GUIStyle();
    private static GUIStyle descriptionStyle = new GUIStyle();

    private const string npcEngineURL = "https://github.com/npc-engine/npc-engine/releases/latest/download/npc-engine-.zip";
    private const string githubURL = "https://github.com/npc-engine/npc-engine";
    private const string docsURL = "https://npc-engine.github.io/npc-engine/";


    private static Texture2D logo = null;
    private static Texture2D docsLogo = null;
    private static Texture2D githubLogo = null;

    private static bool downloading = false;
    private static bool downloadFinished = false;

    private static int progressId = -1;

    public static bool DisplayWelcomeScreen
    {
        get
        {
            return File.Exists(Path.Combine(Application.streamingAssetsPath, ".npc-engine/cli.exe"));
        }
    }


    /// <summary>
    /// Executes on Unity loading.
    /// </summary>
    static NPCEngineWelcomeWindow()
    {
        EditorApplication.delayCall += () =>
        {
            if (
                !DisplayWelcomeScreen
                &&
                !Application.isPlaying
            )
            {
                InitWindow();
            }
        };
    }


    void OnGUI()
    {
        downloadFinished = File.Exists(Path.Combine(Application.streamingAssetsPath, ".npc-engine/cli.exe"));
        DrawWelcomeScreenGUI();
    }

    private void DrawWelcomeScreenGUI()
    {
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(logo);
        GUILayout.Label("Welcome to NPC Engine!\n\n", headerStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("NPC engine allows you to design game characters AI using natural language.\n", descriptionStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        GUIContent docs = new GUIContent("See documentation\n\n", docsLogo);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(docs, EditorStyles.whiteLargeLabel))
            Application.OpenURL(docsURL);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUIContent github = new GUIContent("Checkout NPC Engine Github\n\n", githubLogo);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(github, EditorStyles.whiteLargeLabel))
            Application.OpenURL(githubURL);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Before using this package it's required to disable play mode compilation in\n"
                        + "Edit -> Preferences -> General -> Script changes while playing.\n"
                        + "If play mode compilation will happen Unity will freeze"
                        + "\nand only way to restart it would be to kill the process manually!\n",
                        EditorStyles.whiteLargeLabel
        );
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.enabled = !downloading && !downloadFinished;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (downloadFinished)
        {
            GUILayout.Label("(Already downloaded) To start using it you need to download the inference engine server.");
        }
        else if (downloading)
        {
            GUILayout.Label("(Download in progress) To start using it you need to download the inference engine server.");
        }
        else
        {
            GUILayout.Label("To start using it you need to download the inference engine server.");
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("It's a python server that handles all the heavy lifting.\n");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Download NPC Engine", GUILayout.Width(160)))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndExtractNPCEngine(npcEngineURL));
            downloading = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.enabled = downloadFinished;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (downloadFinished)
        {
            GUILayout.Label("You can also download default deep learning models (they are required unless you have your own)\n");
        }
        else
        {
            GUILayout.Label("(Downloaded NPC engine required)\nYou can also download default deep learning models (they are required unless you have your own)\n");

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Download default models", GUILayout.Width(160)))
        {
            DownloadDefaultModels(".models");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }


    public static void DownloadDefaultModels(string path)
    {
        Process myProcess = new Process();
        myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        myProcess.StartInfo.CreateNoWindow = false;
        myProcess.StartInfo.UseShellExecute = true;
        myProcess.StartInfo.FileName = "CMD.EXE";
        myProcess.StartInfo.Arguments = String.Format(
            " /c \"\"{0}\" download-default-models --models-path \"{1}\"\"",
            Path.Combine(Application.streamingAssetsPath, ".npc-engine/cli.exe"),
            Path.Combine(Application.streamingAssetsPath, path)
        );

        UnityEngine.Debug.Log(myProcess.StartInfo.Arguments);
        myProcess.EnableRaisingEvents = true;
        myProcess.Start();
    }


    IEnumerator DownloadAndExtractNPCEngine(string address)
    {

        progressId = Progress.Start("Downloading NPC Engine", "Downloading npc-engine", 0);
        Directory.CreateDirectory(Application.streamingAssetsPath);
        string path = Path.Combine(Application.streamingAssetsPath, ".npc-engine.zip");

        DownLoadFileInBackground(address, path);

        while (!downloadFinished)
        {
            yield return null;
        }

        // The task is finished. Remove the associated progress indicator.
        Progress.Remove(progressId);
        byte[] file = System.IO.File.ReadAllBytes(path);
        UnityEngine.Debug.Log("Downloaded " + file.Length + " bytes");
        progressId = Progress.Start("Extracting NPC Engine", "Downloading npc-engine", 0);
        UnityEngine.Debug.Log("Extracting " + Path.Combine(Application.streamingAssetsPath, ".npc-engine"));
        yield return ExtractZipFile(file, Path.Combine(Application.streamingAssetsPath, ".npc-engine"));
        Progress.Remove(progressId);

        File.Delete(path);
        downloading = false;
    }

    public static void DownLoadFileInBackground(string address, string path)
    {
        WebClient client = new WebClient();
        Uri uri = new Uri(address);

        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
        client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadFileCompleted);

        client.DownloadFileAsync(uri, path);
    }

    private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
    {
        Progress.Report(progressId, e.ProgressPercentage);
    }

    private static void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error == null)
        {
            downloadFinished = true;
        }
    }

    IEnumerator ExtractZipFile(byte[] zipFileData, string targetDirectory, int bufferSize = 256 * 1024)
    {
        Directory.CreateDirectory(targetDirectory);

        using (MemoryStream fileStream = new MemoryStream())
        {
            fileStream.Write(zipFileData, 0, zipFileData.Length);
            fileStream.Flush();
            fileStream.Seek(0, SeekOrigin.Begin);

            ZipFile zipFile = new ZipFile(fileStream);

            foreach (ZipEntry entry in zipFile)
            {
                string targetFile = Path.Combine(targetDirectory, entry.Name);

                if (entry.IsFile)
                {
                    using (FileStream outputFile = File.Create(targetFile))
                    {
                        if (entry.Size > 0)
                        {
                            Stream zippedStream = zipFile.GetInputStream(entry);
                            byte[] dataBuffer = new byte[bufferSize];

                            int readBytes;
                            while ((readBytes = zippedStream.Read(dataBuffer, 0, bufferSize)) > 0)
                            {
                                outputFile.Write(dataBuffer, 0, readBytes);
                                outputFile.Flush();
                                yield return null;
                            }
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(targetFile);
                }

            }
        }
    }
}
#endif // UNITY_EDITOR