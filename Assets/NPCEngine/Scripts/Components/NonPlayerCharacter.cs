using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NPCEngine.API;
using NPCEngine.Utility;


namespace NPCEngine.Components
{

    public class NonPlayerCharacter : MonoBehaviour
    {

        public int maxLines = 10;
        /// <summary>
        /// Default semantic similarity threshold for dialogue tree triggers.
        /// </summary>
        public float defaultThreshold = 0.6f;

        /// <summary>
        /// Characters assigned to this NPC.
        /// </summary>
        public Character character;

        /// <summary>
        /// Dialogue history.
        /// </summary>
        public List<ChatLine> history;

        /// <summary>
        /// Dialogue start event (triggered when the NPC starts talking)
        /// </summary>
        public UnityEvent OnDialogueStart;

        /// <summary>
        /// Event called when new line is added to history 
        /// where parameters are (ChatLine newLine, bool scriptedLine)
        /// </summary>
        public UnityEvent<ChatLine, bool> OnDialogueLine;

        /// <summary>
        /// Event called in the beginning of an input processing
        /// </summary>
        public UnityEvent OnProcessingStart;

        /// <summary>
        /// Event called in the end of an input processing
        /// </summary> 
        public UnityEvent OnProcessingEnd;

        /// <summary>
        /// Event called when new dialogue topic hints appear
        /// </summary>
        public UnityEvent<List<string>> OnTopicHintsUpdate;

        /// <summary>
        /// Event called when dialogue ends
        /// </summary>
        public UnityEvent OnDialogueEnd;

        /// <summary>
        /// Audio source queue that plays generated speech on the fly one-by-one.
        /// </summary>
        public AudioSourceQueue audioSourceQueue;
        
        /// <summary>
        /// Voice ID of the character for TTS.
        /// </summary>
        public string voiceId;

        /// <summary>
        /// Dialogue System integration.
        /// </summary>
        public AbstractDialogueSystem dialogueSystem;


        private bool listen = true;
        private bool active = false;
        private bool inDialog = false;

        private string lastLine = ""; // last scripted line said, used to avoid repeating same line

        private Coroutine runningCoroutine = null;

        // Test properties

        [HideInInspector]
        public Location testLocation;

        [HideInInspector]
        public Character testOtherCharacter;
        
        void Start()
        {
            if (history == null)
            {
                history = new List<ChatLine>();
            }
            if (dialogueSystem == null)
            {
                dialogueSystem = GetComponent<AbstractDialogueSystem>();
            }

            StartCoroutine(DialogueCheckCoroutine());
        }

        /// <summary>
        /// Initialize dialogue.
        /// </summary>
        public void StartDialogue()
        {
            history.Clear();
            inDialog = true;
            OnDialogueStart?.Invoke();
            if(dialogueSystem != null)
            {
                dialogueSystem.StartDialogue();
                OnTopicHintsUpdate.Invoke(dialogueSystem.GetCurrentNodeTopics().Distinct().ToList());
            }
        }

        /// <summary>
        /// End dialogue.
        /// </summary>
        public void EndDialogue()
        {
            inDialog = false;
            history.Clear();
            OnDialogueEnd?.Invoke();
            if (dialogueSystem != null)
            {
                dialogueSystem.EndDialogue();
            }
        }

        /// <summary>
        /// Step dialogue by line from player.
        /// </summary>
        /// <param name="otherName"></param>
        /// <param name="otherPersona"></param>
        /// <param name="line"></param>
        public void HandleLine(string otherName, string otherPersona, string line)
        {
            runningCoroutine = StartCoroutine(HandleLineCoroutine(otherName, otherPersona, line));
        }


        /// <summary>
        /// Line handling coroutine
        /// </summary>
        public IEnumerator HandleLineCoroutine(string otherName, string otherPersona, string line)
        {
            if (listen)
            {
                listen = false;
                OnProcessingStart.Invoke();
                if (dialogueSystem != null && !dialogueSystem.CurrentNodeIsPlayer())
                {
                    yield return SayNPCLines();
                }
                else
                {
                    history.Add(new ChatLine { speaker = otherName, line = line });

                    if(history.Count > maxLines)
                    {
                        history.RemoveAt(0);
                    }
                    OnDialogueLine?.Invoke(new ChatLine { speaker = otherName, line = line }, false);
                    yield return HandlePlayerLineCoroutine(otherName, otherPersona, line);
                }
                OnProcessingEnd.Invoke();
                if (dialogueSystem != null)
                    OnTopicHintsUpdate.Invoke(dialogueSystem.GetCurrentNodeTopics().Distinct().ToList());
                listen = true;
            }
            runningCoroutine = null;
        }

        /// <summary>
        /// Player line handling coroutine
        /// </summary>
        public IEnumerator HandlePlayerLineCoroutine(string otherName, string otherPersona, string line)
        {
            List<float> scores = new List<float>();
            var dialogueLines = dialogueSystem != null? dialogueSystem.GetCurrentNodeOptions(): new List<string>();
            if (dialogueLines.Count != 0)
            {
                yield return NPCEngineManager.Instance.GetAPI<SemanticQuery>().Compare(line, dialogueLines, (output) => { scores = output; });
                var maxScore = scores.Max();
                var customThreshold = dialogueSystem.CurrentNodeThreshold();
                var threshold = customThreshold != -1f ? customThreshold : defaultThreshold;
                if (maxScore > threshold)
                {
                    var maxScoreIndex = scores.IndexOf(maxScore);
                    dialogueSystem.SelectOption(maxScoreIndex);
                    dialogueSystem.Next();
                    yield return SayNPCLines();
                }
                else
                {
                    yield return GenerateReply(otherName, otherPersona, line);
                }
            }
            else
            {
                yield return GenerateReply(otherName, otherPersona, line);
            }

        }

        /// <summary>
        /// Run Chatbot API and generate and play speech
        /// </summary>
        /// <param name="otherName">Other conversant name</param>
        /// <param name="otherPersona">Other conversant persona</param>
        /// <param name="line">Utterance to generate reply to.</param>
        /// <returns></returns>
        public IEnumerator GenerateReply(string otherName, string otherPersona, string line)
        {
            var context = new FantasyChatbotContext
            {
                name = character.Name,
                persona = character.Persona,
                other_name = otherName,
                other_persona = otherPersona,
                location_name = PlayerCharacter.Instance.currentLocation.Name, // Use player's location because Player and NPC should be in the same location
                location = PlayerCharacter.Instance.currentLocation.Description,
                history = history,
            };
            string reply = "";
            yield return NPCEngineManager.Instance.GetAPI<FantasyChatbotTextGeneration>()
                .GenerateReply(context, character.temperature, character.topK, character.numSampled, (output) => { reply = output; }, (e)=> { Debug.LogError(e); });

            OnDialogueLine?.Invoke(new ChatLine { speaker = character.Name, line = reply }, false);
            history.Add(new ChatLine { speaker = character.Name, line = reply });
            if(history.Count > maxLines)
            {
                history.RemoveAt(0);
            }
            yield return GenerateAndPlaySpeech(reply);
        }

        /// <summary>
        /// Scan dialogue tree and say all scripted NPC lines
        /// </summary>
        /// <returns></returns>
        public IEnumerator SayNPCLines()
        {
            string line = dialogueSystem.CurrentNodeNPCLine();
            bool firstLine = true;
            while (!dialogueSystem.CurrentNodeIsPlayer() && (line != lastLine || firstLine))
            {
                firstLine = false;
                lastLine = line;
                var audio = dialogueSystem.CurrentNodeNPCAudio();
                history.Add(new ChatLine { speaker = character.Name, line = line });
                if(history.Count > maxLines)
                {
                    history.RemoveAt(0);
                }
                OnDialogueLine?.Invoke(new ChatLine { speaker = character.Name, line = line }, true);
                if (audio != null)
                {
                    audioSourceQueue.PlaySound(audio);
                }
                else
                {
                    yield return GenerateAndPlaySpeech(line);
                }
                if(dialogueSystem.IsEnd())
                {
                    EndDialogue();
                    break;
                }
                dialogueSystem.Next();
                line = dialogueSystem.CurrentNodeNPCLine();
            }
        }

        /// <summary>
        /// Coroutine to generate speech from text.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public IEnumerator GenerateAndPlaySpeech(string line)
        {
            yield return NPCEngineManager.Instance.GetAPI<TextToSpeech>().StartTTS(voiceId, line, NPCEngineConfig.Instance.nChunksSpeechGeneration, () => { });

            List<float> audioData;
            do
            {
                audioData = new List<float>();
                yield return NPCEngineManager.Instance.GetAPI<TextToSpeech>().GetNextResult((output) => { audioData = output; });
                if(audioData == null)
                {
                    break;
                }
                if (audioData.Count > 0)
                {
                    var clip = AudioClip.Create("tmp", audioData.Count, 1, 22050, false);
                    clip.SetData(audioData.ToArray(), 0);
                    audioSourceQueue.PlaySound(clip);
                }
            } while (audioData.Count != 0);

        }




        private void Update()
        {
        }

        IEnumerator DialogueCheckCoroutine()
        {
            while (true)
            {
                if (!inDialog)
                {
                    if (
                        !active
                        && !PlayerCharacter.Instance.IsRegistered(this)
                        && PlayerCharacter.Instance.CheckIsSeen(audioSourceQueue.audioSource.transform.position)
                    )
                    {
                        PlayerCharacter.Instance.RegisterDialogueCandidate(this);
                        active = true;
                    }
                    else if (
                        !inDialog 
                        && active 
                        && !PlayerCharacter.Instance.CheckIsSeen(audioSourceQueue.audioSource.transform.position)
                        && PlayerCharacter.Instance.IsRegistered(this) 
                    )
                    {
                        PlayerCharacter.Instance.DeregisterDialogueCandidate(this);
                        active = false;
                    }
                }
                else if ((PlayerCharacter.Instance.transform.position - audioSourceQueue.audioSource.transform.position).magnitude > PlayerCharacter.Instance.MaxRange)
                {
                    EndDialogue();
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}