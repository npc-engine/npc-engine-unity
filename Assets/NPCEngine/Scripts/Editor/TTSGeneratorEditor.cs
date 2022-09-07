using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using NPCEngine.Components;
using NPCEngine.Utility;

namespace NPCEngine
{
    
    [CustomEditor(typeof(TTSGenerator))]
    public class TTSGeneratorEditor : Editor {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var ttsGenerator = (TTSGenerator)target;
            // print state of generating
            if (ttsGenerator.Generating)
            {
                EditorGUILayout.LabelField("Generating...");
            }
            else
            {
                if (GUILayout.Button("Generate"))
                {
                    CoroutineUtility.Instance.StartCoroutine(ttsGenerator.GenerateAudioCoroutine());
                }
            }
            if (GUILayout.Button("Play Audio"))
            {
                ttsGenerator.audioSource.Play();
            }
            if (ttsGenerator.Saving)
            {
                EditorGUILayout.LabelField("Saving...");
            }
            else
            {
                if (GUILayout.Button("Save Audio"))
                {
                    CoroutineUtility.Instance.StartCoroutine(ttsGenerator.SaveAudio());
                }
            }
        }
        
    }
}