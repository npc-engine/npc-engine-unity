using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.RPC;

namespace NPCEngine.API
{

    /// <summary>
    /// <c>Chatbot</c> provides remote procedure calls 
    /// to inference engine's TextGeneration services.
    ///</summary>
    public class PersonaDialogue : RPCBase
    {

        public override string ServiceId { get { return "PersonaDialogueAPI"; } }

        [Serializable()]
        class StartDialogueMessage
        {
            public string name1;
            public string persona1;
            public string name2;
            public string persona2;
            public string location_name;
            public string location_description;
            public string dialogue_id;
        }


        [Serializable()]
        class StepDialogueMessage
        {
            public string dialogue_id;
            public string speaker_id;
            public string utterance;
            public List<string> scripted_utterances;
            public float scripted_threshold;
            public bool update_history;
        }

        public IEnumerator StartDialogue(
            string name1,
            string persona1,
            string name2,
            string persona2,
            string location_name,
            string location_description,
            Action<string> outputCallback,
            string dialogue_id = null)
        {
            var msg = new StartDialogueMessage
            {
                name1 = name1,
                persona1 = persona1,
                name2 = name2,
                persona2 = persona2,
                location_name = location_name,
                location_description = location_description,
                dialogue_id = dialogue_id
            };
            var result = this.Run<StartDialogueMessage, string>("start_dialogue", msg);
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        /// <summary>
        /// Step dialogue.
        /// 
        /// If utterance is null, it will be generated.
        /// If scripted utterances are not null they will be compared to the utterance 
        /// and replace it if similarity score is above scripted_threshold (score is in range [0,1]).
        /// If update_history is true, the dialogue history will be updated with the utterance.
        /// </summary>
        /// <param name="dialogue_id"></param>
        /// <param name="speaker_id"></param>
        /// <param name="utterance"></param>
        /// <param name="scripted_utterances"></param>
        /// <param name="scripted_threshold"></param>
        /// <param name="update_history"></param>
        /// <param name="outputCallback">Callback that accepts results: Tuple with utterance 
        /// and bool flag that is True if scripted utterance was used</param>
        /// <returns></returns>
        public IEnumerator StepDialogue(
            string dialogue_id,
            string speaker_id,
            bool update_history,
            Action<Tuple<string, bool>> outputCallback,
            float scripted_threshold = 0.5f,
            List<string> scripted_utterances = null,
            string utterance = null
        )
        {
            var msg = new StepDialogueMessage
            {
                dialogue_id = dialogue_id,
                speaker_id = speaker_id,
                utterance = utterance,
                scripted_utterances = scripted_utterances,
                scripted_threshold = scripted_threshold,
                update_history = update_history
            };
            var result = this.Run<StepDialogueMessage, Tuple<string, bool>>("step_dialogue", msg);
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }
    }
}