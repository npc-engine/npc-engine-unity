using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Components
{

    public abstract class AbstractDialogueSystem : MonoBehaviour
    {

        public abstract void StartDialogue();

        public abstract void EndDialog();

        public abstract bool CurrentNodeIsPlayer();

        public abstract List<string> GetCurrentNodeOptions();

        public abstract List<string> GetCurrentNodeTopics();

        public abstract void SelectOption(int optionId);

        public abstract void Next();

        public abstract float CurrentNodeThreshold();

        public abstract string CurrentNodeNPCLine();

        public abstract AudioClip CurrentNodeNPCAudio();

    }

}