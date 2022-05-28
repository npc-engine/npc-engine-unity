using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NPCEngine.RPC;
using NPCEngine.API;
using NPCEngine.Utility;


namespace NPCEngine.Components
{

    [RequireComponent(typeof(AbstractDialogueSystem))]
    public class NonPlayerCharacter : MonoBehaviour
    {

        [Range(0, 2)]
        public float temperature = 1;
        public int topK = 0;

        public int nChunksTextGeneration = 7;

        public float defaultThreshold = 0.6f;

        private bool initialized = false;
        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public string characterName;
        [TextArea(3, 10)]
        public string persona;
        public string voiceId;

        public List<ChatLine> history;

        //Dialogue start event (triggered when the NPC starts talking)
        public UnityEvent OnDialogueStart;

        //Event called when new line is added to history 
        //where parameters are (ChatLine newLine, bool scriptedLine)
        public UnityEvent<ChatLine, bool> OnDialogueLine;

        //Event called in the beginning of an input processing
        public UnityEvent OnProcessingStart;

        //Event called in the end of an input processing 
        public UnityEvent OnProcessingEnd;

        //Event called when new dialogue topic hints appear
        public UnityEvent<List<string>> OnTopicHintsUpdate;

        //Event called when dialogue ends
        public UnityEvent OnDialogueEnd;

        public AudioSourceQueue audioSourceQueue;

        public AbstractDialogueSystem dialogueSystem;


        private bool listen = true;
        private bool active = false;
        private bool inDialog = false;

        private string lastLine = ""; // last scripted line said, used to avoid repeating same line

        private Coroutine runningCoroutine = null;

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

        public void StartDialogue()
        {
            history.Clear();
            inDialog = true;
            OnDialogueStart?.Invoke();
            dialogueSystem.StartDialogue();
            OnTopicHintsUpdate.Invoke(dialogueSystem.GetCurrentNodeTopics().Distinct().ToList());
        }

        public void EndDialog()
        {
            inDialog = false;
            history.Clear();
            OnDialogueEnd?.Invoke();
            dialogueSystem.EndDialog();
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }
        }

        public void HandleLine(string otherName, string otherPersona, string line)
        {
            runningCoroutine = StartCoroutine(HandleLineCoroutine(otherName, otherPersona, line));
        }


        //Line handling coroutine
        public IEnumerator HandleLineCoroutine(string otherName, string otherPersona, string line)
        {
            if (listen)
            {
                listen = false;
                OnProcessingStart.Invoke();
                if (!dialogueSystem.CurrentNodeIsPlayer())
                {
                    yield return SayNPCLines();
                }
                else
                {
                    history.Add(new ChatLine { speaker = otherName, line = line });

                    OnDialogueLine?.Invoke(new ChatLine { speaker = otherName, line = line }, false);
                    yield return HandlePlayerLineCoroutine(otherName, otherPersona, line);
                }
                OnProcessingEnd.Invoke();
                OnTopicHintsUpdate.Invoke(dialogueSystem.GetCurrentNodeTopics().Distinct().ToList());
                listen = true;
            }
            runningCoroutine = null;
        }

        //Player line handling coroutine
        public IEnumerator HandlePlayerLineCoroutine(string otherName, string otherPersona, string line)
        {
            List<float> scores = new List<float>();
            var dialogueLines = dialogueSystem.GetCurrentNodeOptions();
            if (dialogueLines.Count != 0)
            {
                yield return NPCEngineManager.Instance.GetAPI<SemanticQuery>().CompareCoroutine(line, dialogueLines, (output) => { scores = output; });
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

        // Run Chatbot API and generate and play speech
        public IEnumerator GenerateReply(string otherName, string otherPersona, string line)
        {
            var context = new FantasyChatbotContext
            {
                name = characterName,
                persona = persona,
                other_name = otherName,
                other_persona = otherPersona,
                location_name = PlayerCharacter.Instance.settingName, // Use player's location because Player and NPC should be in the same location
                location = PlayerCharacter.Instance.settingDescription,
                history = history,
            };
            string reply = "";
            yield return NPCEngineManager.Instance.GetAPI<TextGeneration<FantasyChatbotContext>>()
                .GenerateReplyCoroutine(context, (output) => { reply = output; }, temperature, topK);

            OnDialogueLine?.Invoke(new ChatLine { speaker = characterName, line = reply }, false);
            history.Add(new ChatLine { speaker = characterName, line = reply });
            yield return GenerateAndPlaySpeech(reply);
        }

        public IEnumerator SayNPCLines()
        {
            string line = dialogueSystem.CurrentNodeNPCLine();
            bool firstLine = true;
            while (!dialogueSystem.CurrentNodeIsPlayer() && (line != lastLine || firstLine))
            {
                firstLine = false;
                lastLine = line;
                var audio = dialogueSystem.CurrentNodeNPCAudio();
                history.Add(new ChatLine { speaker = characterName, line = line });
                OnDialogueLine?.Invoke(new ChatLine { speaker = characterName, line = line }, true);
                if (audio != null)
                {
                    audioSourceQueue.PlaySound(audio);
                }
                else
                {
                    yield return GenerateAndPlaySpeech(line);
                }
                dialogueSystem.Next();
                line = dialogueSystem.CurrentNodeNPCLine();
            }
        }

        public IEnumerator GenerateAndPlaySpeech(string line)
        {
            NPCEngineManager.Instance.GetAPI<TextToSpeech>().StartTTS(voiceId, line, nChunksTextGeneration);

            List<float> audioData;
            do
            {
                audioData = new List<float>();
                yield return NPCEngineManager.Instance.GetAPI<TextToSpeech>().GetNextResultCoroutine((output) => { audioData = output; });

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
            if (NPCEngineManager.Instance.Initialized && !initialized)
                initialized = true;

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
                    else if (!inDialog && active && !PlayerCharacter.Instance.CheckIsSeen(audioSourceQueue.audioSource.transform.position))
                    {
                        PlayerCharacter.Instance.DeregisterDialogueCandidate(this);
                        active = false;
                    }
                }
                else if ((PlayerCharacter.Instance.transform.position - audioSourceQueue.audioSource.transform.position).magnitude > PlayerCharacter.Instance.MaxRange)
                {
                    EndDialog();
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}