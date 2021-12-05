using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Components
{

    public abstract class AbstractDialogueSystem : MonoBehaviour
    {

        public abstract void StartDialogue();

        public abstract void EndDialog();

        /// <summary>
        /// Returns true if it's players turn in the dialogue.
        /// </summary>
        public abstract bool CurrentNodeIsPlayer();

        /// <summary>
        /// Get the lines associated with the current node in the dialogue.
        /// </summary>
        public abstract List<string> GetCurrentNodeOptions();

        /// <summary>
        /// Get the short abstract descriptions of the lines associated with the current node in the dialogue.
        /// </summary>
        public abstract List<string> GetCurrentNodeTopics();

        /// <summary>
        /// Select a line from the current node in the dialogue.
        /// </summary>
        public abstract void SelectOption(int optionId);

        /// <summary>
        /// Move to the next node in the dialogue.
        /// </summary>
        public abstract void Next();

        /// <summary>
        /// Get the semantic similarity threshold for the current node in the dialogue.
        /// Return -1 if no threshold is set.
        /// </summary>
        public abstract float CurrentNodeThreshold();

        /// <summary>
        /// If the current node is NPC, return the NPC's line.
        /// else return empty string.
        /// </summary>
        public abstract string CurrentNodeNPCLine();

        /// <summary>
        /// Get the current node's audio for the NPC line (Never called for player).
        /// Return null if no audio is set or if the node is player.
        /// </summary>
        public abstract AudioClip CurrentNodeNPCAudio();

    }

}