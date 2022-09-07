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
    /// <summary>
    /// MonoBehaviour that handles dialogue initiation and passing recognized speech to NPCs.
    /// </summary>
    public class PlayerCharacter : Singleton<PlayerCharacter>
    {

        void Awake()
        {
            activeDialogAgents = new List<NonPlayerCharacter>();
        }

        /// <summary>
        /// Location description of the player.
        /// </summary>
        public Location currentLocation;

        /// <summary>
        /// Character description of the player.
        /// </summary>
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
            
            if (SpeechToText == null)
            {
                SpeechToText = GetComponent<AbstractSpeechToText>();
            }
            SpeechToText.OnSpeechRecognized += HandleUtterance;
        }


        /// <summary>
        /// Adds NPC to the list of potential conversants.
        /// </summary>
        /// <param name="agent"></param>
        public void RegisterDialogueCandidate(NonPlayerCharacter agent)
        {
            activeDialogAgents.Add(agent);
        }

        /// <summary>
        /// Remove NPC from the list of potential conversants.
        /// </summary>
        /// <param name="agent"></param>
        public void DeregisterDialogueCandidate(NonPlayerCharacter agent)
        {
            activeDialogAgents.Remove(agent);
        }

        /// <summary>
        /// Check if NPC is registered as a dialogue candidate.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool IsRegistered(NonPlayerCharacter agent)
        {
            if (activeDialogAgents == null)
            {
                return false;
            }

            return activeDialogAgents.Contains(agent);
        }

        /// <summary>
        /// Check if NPC is seen.
        /// </summary>
        public bool CheckIsSeen(Vector3 dialogAgentPosition)
        {
            var cameraPoint = PlayerCharacter.Instance.CheckCamera.WorldToViewportPoint(dialogAgentPosition);

            return (
                cameraPoint.x > HorizontalMargin && cameraPoint.x < (1f - HorizontalMargin)
                && cameraPoint.y > VerticalMargin && cameraPoint.y < (1f - VerticalMargin)
                && cameraPoint.z < MaxRange
            );
        }

        /// <summary>
        /// Leave current dialogue.
        /// </summary>
        public void LeaveDialog()
        {
            currentDialog.EndDialogue();
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

                    if (new_magn < last_magnitude && new_magn < MaxRange)
                    {
                        Debug.Log("Found closest NPC");
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
                    Debug.Log("Starting dialogue with " + currentDialog.name);
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
