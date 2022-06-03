using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using NPCEngine.Components;

namespace NPCEngine
{
    
    [CustomEditor(typeof(NonPlayerCharacter))]
    public class NonPlayerCharacterEditor : Editor {

        SerializedProperty temperatureProp;
        SerializedProperty topKProp;
        SerializedProperty nChunksTextGenerationProp;
        SerializedProperty defaultThresholdProp;
        SerializedProperty characterNameProp;
        SerializedProperty personaProp;
        SerializedProperty voiceIdProp;
        SerializedProperty historyProp;
        SerializedProperty OnDialogueStartProp;
        SerializedProperty OnDialogueLineProp;
        SerializedProperty OnProcessingStartProp;
        SerializedProperty OnProcessingEndProp;
        SerializedProperty OnTopicHintsUpdateProp;
        SerializedProperty OnDialogueEndProp;
        SerializedProperty audioSourceQueueProp;
        SerializedProperty dialogueSystemProp;

        private bool showTextGeneration = true;
        private bool showVoice = true;
        private bool showDialogueTree = true;
        private bool showCallbacks = true;

        private bool showTextGenerationTest = false;
        private bool showVoiceTest = false;
        private bool showDialogueTreeTest = false;

        void OnEnable()
        {
            temperatureProp = serializedObject.FindProperty("temperature");
            topKProp = serializedObject.FindProperty("topK");
            nChunksTextGenerationProp = serializedObject.FindProperty("nChunksTextGeneration");
            defaultThresholdProp = serializedObject.FindProperty("defaultThreshold");
            characterNameProp = serializedObject.FindProperty("characterName");
            personaProp = serializedObject.FindProperty("persona");
            voiceIdProp = serializedObject.FindProperty("voiceId");
            historyProp = serializedObject.FindProperty("history");
            OnDialogueStartProp = serializedObject.FindProperty("OnDialogueStart");
            OnDialogueLineProp = serializedObject.FindProperty("OnDialogueLine");
            OnProcessingStartProp = serializedObject.FindProperty("OnProcessingStart");
            OnProcessingEndProp = serializedObject.FindProperty("OnProcessingEnd");
            OnTopicHintsUpdateProp = serializedObject.FindProperty("OnTopicHintsUpdate");
            OnDialogueEndProp = serializedObject.FindProperty("OnDialogueEnd");
            audioSourceQueueProp = serializedObject.FindProperty("audioSourceQueue");
            dialogueSystemProp = serializedObject.FindProperty("dialogueSystem");
        }

        public override void OnInspectorGUI() {
            showTextGeneration = EditorGUILayout.Foldout(showTextGeneration, "TextGeneration");
            if (showTextGeneration) {
                EditorGUILayout.PropertyField(temperatureProp);
                EditorGUILayout.PropertyField(topKProp);
                EditorGUILayout.PropertyField(characterNameProp);
                EditorGUILayout.PropertyField(personaProp);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();

                showTextGenerationTest = EditorGUILayout.Foldout(showTextGenerationTest, "Test");
                if (showTextGenerationTest)
                {
                    if(GUILayout.Button("Chat", GUILayout.MaxWidth(120)))
                    {

                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            showVoice = EditorGUILayout.Foldout(showVoice, "Voice");
            if(showVoice) {
                EditorGUILayout.PropertyField(voiceIdProp);
                EditorGUILayout.PropertyField(nChunksTextGenerationProp);
                EditorGUILayout.PropertyField(audioSourceQueueProp);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                showVoiceTest = EditorGUILayout.Foldout(showVoiceTest, "Test");
                if(showVoiceTest)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Test Utterance:", GUILayout.MaxWidth(100));
                    GUILayout.TextField("");
                    GUILayout.EndHorizontal();
                    if(GUILayout.Button("Generate Speech", GUILayout.MaxWidth(120)))
                    {
                        
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            showDialogueTree = EditorGUILayout.Foldout(showDialogueTree, "Dialogue tree");
            if(showDialogueTree) {
                EditorGUILayout.PropertyField(dialogueSystemProp);
                EditorGUILayout.PropertyField(defaultThresholdProp);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                showDialogueTreeTest = EditorGUILayout.Foldout(showDialogueTreeTest, "Test");
                if(showDialogueTreeTest)
                {
                    
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            showCallbacks = EditorGUILayout.Foldout(showCallbacks, "Callbacks");
            if(showCallbacks) {
                EditorGUILayout.PropertyField(OnDialogueStartProp);
                EditorGUILayout.PropertyField(OnDialogueLineProp);
                EditorGUILayout.PropertyField(OnProcessingStartProp);
                EditorGUILayout.PropertyField(OnProcessingEndProp);
                EditorGUILayout.PropertyField(OnTopicHintsUpdateProp);
                EditorGUILayout.PropertyField(OnDialogueEndProp);
            }
            EditorGUILayout.PropertyField(historyProp);
            serializedObject.ApplyModifiedProperties();
        }
    }
}