using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using NPCEngine.API;
using NPCEngine.Components;
using NPCEngine.Utility;

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

//      Containers for UI data
//          Foldout flags
        private bool showTextGeneration = true;
        private bool showVoice = true;
        private bool showDialogueTree = true;
        private bool showCallbacks = true;

        private bool showTextGenerationTest = false;
        private bool showVoiceTest = false;
        private bool showDialogueTreeTest = false;

//          Text generation test data
        private string testLocationName = "";
        private string testLocationDescription = "";
        private string testOtherName = "";
        private string testOtherPersona = "";
        private string testChatHistory = "";
        private string testChatLine = "";

        private string testVoiceLine = "";
//          Voice test data
        private string testSimilarityLine1 = "";
        private string testSimilarityLine2 = "";
//          Dialogue tree test data
        private string similarityScore = "";

//      Containers for state
        private bool generatingText = false;
        private bool generatingVoice = false;
        private bool computingSimilarity = false;

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

                showTextGenerationTest = EditorGUILayout.Foldout(showTextGenerationTest, "Test Chat");
                if (showTextGenerationTest)
                {
                    TextGenerationTestUI();
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
                showVoiceTest = EditorGUILayout.Foldout(showVoiceTest, "Test Voice");
                if(showVoiceTest)
                {
                    TestVoiceUI();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            showDialogueTree = EditorGUILayout.Foldout(showDialogueTree, "Dialogue Tree");
            if(showDialogueTree) {
                EditorGUILayout.PropertyField(dialogueSystemProp);
                EditorGUILayout.PropertyField(defaultThresholdProp);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                showDialogueTreeTest = EditorGUILayout.Foldout(showDialogueTreeTest, "Test Similarity");
                if(showDialogueTreeTest)
                {
                    TestSimilarityUI();
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

        private void TextGenerationTestUI()
        {
            GUILayout.Label("Location name");
            testLocationName = EditorGUILayout.TextArea(testLocationName);
            GUILayout.Label("Location description");
            testLocationDescription = EditorGUILayout.TextArea(testLocationDescription, GUILayout.Height(60));
            GUILayout.Label("Your name");
            testOtherName = EditorGUILayout.TextArea(testOtherName);
            GUILayout.Label("Your persona");
            testOtherPersona = EditorGUILayout.TextArea(testOtherPersona, GUILayout.Height(60));
            GUILayout.Label("History");
            GUIStyle myTextAreaStyle = new GUIStyle(EditorStyles.textArea);
            myTextAreaStyle.wordWrap = true;
            myTextAreaStyle.stretchHeight = true;
            testChatHistory = EditorGUILayout.TextArea(testChatHistory, myTextAreaStyle);
            GUILayout.Space(10f);
            HorizontalLine(Color.black);
            EditorGUILayout.BeginHorizontal();
            if(!generatingText)
            {
                testChatLine = EditorGUILayout.TextArea(testChatLine);
                if(GUILayout.Button("Send", GUILayout.MaxWidth(120)))
                {
                    CoroutineUtility.StartCoroutine(
                        GenerateText(String.Copy(testChatLine)), 
                        (NonPlayerCharacter) target, 
                        "GenerateText"
                    );
                    testChatLine = "";
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.TextArea(testChatLine);
                if(GUILayout.Button("Generating...", GUILayout.MaxWidth(120)))
                {}
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void TestVoiceUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Test Utterance:", GUILayout.MaxWidth(100));
            testVoiceLine = GUILayout.TextField(testVoiceLine);
            GUILayout.EndHorizontal();
            if(!generatingVoice)
            {
                if(GUILayout.Button("Generate Speech", GUILayout.MaxWidth(120)))
                {
                    CoroutineUtility.StartCoroutine(
                        GenerateAndPlaySpeech(testVoiceLine), 
                        (NonPlayerCharacter) target, 
                        "GenerateAndPlaySpeech"
                    );
                }
            }
            else
            {
                GUI.enabled = false;
                if(GUILayout.Button("Generating...", GUILayout.MaxWidth(120)))
                {
                }
                GUI.enabled = true;
            }
        }

        private void TestSimilarityUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sentence 1:", GUILayout.MaxWidth(100));
            testSimilarityLine1 = GUILayout.TextField(testSimilarityLine1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sentence 2:", GUILayout.MaxWidth(100));
            testSimilarityLine2 = GUILayout.TextField(testSimilarityLine2);
            GUILayout.EndHorizontal();
            if(similarityScore != "")
            {
                GUILayout.Label("Similarity: " + similarityScore);
            }
            if(!computingSimilarity)
            {
                if(GUILayout.Button("Compute Similarity", GUILayout.MaxWidth(120)))
                {
                    CoroutineUtility.StartCoroutine(
                        ComputeSimilarity(testSimilarityLine1, testSimilarityLine2), 
                        (NonPlayerCharacter) target, 
                        "ComputeSimilarity"
                    );
                }
            }
            else
            {
                GUI.enabled = false;
                if(GUILayout.Button("Computing...", GUILayout.MaxWidth(120)))
                {
                }
                GUI.enabled = true;
            }
        }

        private IEnumerator GenerateText(string line)
        {
            generatingText = true;
            testChatHistory += testOtherName + ": " + line + "\n";
            var npc = (NonPlayerCharacter) target;
            var context = new FantasyChatbotContext
            {
                name = npc.characterName,
                persona = npc.persona,
                other_name = testOtherName,
                other_persona = testOtherPersona,
                location_name = testLocationName,
                location = testLocationDescription,
                history = ParseToChatLines(testChatHistory),
            };
            string reply = "";
            yield return NPCEngineManager.Instance.GetAPI<FantasyChatbotTextGeneration>()
                .GenerateReply(context, (output) => { reply = output; }, npc.temperature, npc.topK);
            
            testChatHistory += npc.characterName + ": " + reply + "\n";
            generatingText = false;
        }

        private List<ChatLine> ParseToChatLines(string history)
        {
            var lines = new List<ChatLine>();
            var linesStr = history.Split('\n');
            foreach(var lineStr in linesStr)
            {
                var line = new ChatLine();
                var lineParts = lineStr.Split(':');
                if(lineParts.Length == 2)
                {
                    line.speaker = lineParts[0];
                    line.line = lineParts[1];
                    lines.Add(line);
                }
            }
            return lines;
        }

        private IEnumerator ComputeSimilarity(string line1, string line2)
        {
            computingSimilarity = true;
            var scores = new List<float>();
            yield return NPCEngineManager.Instance.GetAPI<SemanticQuery>().Compare(line1, new List<string>{line2}, (output) => { scores = output; });
            if(scores.Count > 0)
            {
                similarityScore = scores[0].ToString();
            }
            computingSimilarity = false;
        }

        private IEnumerator GenerateAndPlaySpeech(string utterance)
        {
            generatingVoice = true;
            yield return ((NonPlayerCharacter) target).GenerateAndPlaySpeech(utterance);
            generatingVoice = false;
        }

        void HorizontalLine(Color color)
        {
            GUIStyle horizontalLine;
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            // horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;
            horizontalLine.stretchWidth = true;
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }
    }
}