using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;
using NPCEngine.Utility;

namespace NPCEngine.Components
{
    public class PlayerCharacter : Singleton<PlayerCharacter>
    {

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        
        public Location currentLocation;

        public Character character;

        [Tooltip("This camera will check if Player sees NPC to initiate dialog")]
        public Camera CheckCamera;

        [Tooltip("This will limit Player distance to NPC for initiating dialog")]
        public float MaxRange = 3f;

        [Tooltip("This will define how far from vertical edges NPC face should be ( in [0, 0.5] range, 0.5 would mean it should be straight in the center)")]
        [Range(0, 0.5f)]
        public float VerticalMargin = 0.2f;

        [Tooltip("This will define how far from horizontal edges NPC face should be ( in [0, 0.5] range, 0.5 would mean it should be straight in the center)")]
        [Range(0, 0.5f)]
        public float HorizontalMargin = 0.2f;

        public AbstractSpeechToText SpeechToText;

        private NonPlayerCharacter currentDialog;
        private List<NonPlayerCharacter> activeDialogAgents;



        void Start()
        {
            activeDialogAgents = new List<NonPlayerCharacter>();
            if (SpeechToText == null)
            {
                SpeechToText = GetComponent<AbstractSpeechToText>();
            }
            SpeechToText.OnSpeechRecognized += HandleUtterance;
        }



        public void RegisterDialogueCandidate(NonPlayerCharacter agent)
        {
            activeDialogAgents.Add(agent);
        }

        public void DeregisterDialogueCandidate(NonPlayerCharacter agent)
        {
            activeDialogAgents.Remove(agent);
        }

        public bool IsRegistered(NonPlayerCharacter agent)
        {
            if (activeDialogAgents == null)
            {
                return false;
            }

            return activeDialogAgents.Contains(agent);
        }

        public bool CheckIsSeen(Vector3 dialogAgentPosition)
        {
            var cameraPoint = PlayerCharacter.Instance.CheckCamera.WorldToViewportPoint(dialogAgentPosition);

            return (
                cameraPoint.x > HorizontalMargin && cameraPoint.x < (1f - HorizontalMargin)
                && cameraPoint.y > VerticalMargin && cameraPoint.y < (1f - VerticalMargin)
                && cameraPoint.z < MaxRange
            );
        }

        public void LeaveDialog()
        {
            currentDialog.EndDialog();
        }

        private void OnDialogueEnd()
        {
            currentDialog.OnDialogueLine.RemoveListener(SetContextFromChatLine);
            currentDialog.OnProcessingEnd.RemoveListener(SpeechToText.StartListening);
            currentDialog.OnProcessingStart.RemoveListener(SpeechToText.StopListening);
            currentDialog.OnDialogueEnd.RemoveListener(OnDialogueEnd);
            currentDialog = null;
            SpeechToText.StartListening();
        }

        private void HandleUtterance(string utterance)
        {
            if (currentDialog == null)
            {
                var last_magnitude = float.PositiveInfinity;
                foreach (var da in activeDialogAgents)
                {
                    var new_magn = (da.gameObject.transform.position - transform.position).magnitude;
                    if (new_magn < last_magnitude)
                    {
                        last_magnitude = new_magn;
                        currentDialog = da;
                    }
                }
                if (currentDialog != null)
                {
                    currentDialog.StartDialogue();
                    currentDialog.OnDialogueLine.AddListener(SetContextFromChatLine);
                    currentDialog.OnProcessingEnd.AddListener(SpeechToText.StartListening);
                    currentDialog.OnProcessingStart.AddListener(SpeechToText.StopListening);
                    currentDialog.OnDialogueEnd.AddListener(OnDialogueEnd);
                    currentDialog.HandleLine(character.Name, character.Persona, utterance);
                }
            }
            else
            {
                currentDialog.HandleLine(character.Name, character.Persona, utterance);
            }
        }

        private void SetContextFromChatLine(ChatLine chatLine, bool _)
        {
            if (chatLine.speaker != character.Name)
            {
                SpeechToText.Context = chatLine.line;
            }
        }

        private void OnDestroy()
        {
            SpeechToText.OnSpeechRecognized -= HandleUtterance;
        }
    }

}
