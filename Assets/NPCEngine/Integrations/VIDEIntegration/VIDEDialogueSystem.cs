using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPCEngine.Components;
using VIDE_Data;


/// <summary>
/// This is an example implementation of an abstract dialogue system to be used with NPCEngine components.
/// </summary>
[RequireComponent(typeof(VIDE_Assign))]
public class VIDEDialogueSystem : AbstractDialogueSystem
{
    private void Start()
    {
        gameObject.AddComponent<VD>();
    }

    public override void StartDialogue()
    {
        VD.BeginDialogue(GetComponent<VIDE_Assign>());
    }

    public override void EndDialogue()
    {
        VD.EndDialogue();
    }

    public override bool CurrentNodeIsPlayer()
    {
        if (VD.nodeData != null)
        {
            return VD.nodeData.isPlayer;
        }
        else
        {
            return false;
        }
    }


    // Get nodaData comments, remove everything until closing square bracket and return
    public override List<string> GetCurrentNodeOptions()
    {
        if (VD.nodeData != null)
        {
            List<string> options = new List<string>();
            string[] lines = VD.nodeData.comments;
            foreach (string line in lines)
            {
                if (line.Contains("]"))
                {
                    options.Add(line.Substring(line.IndexOf(']') + 1));
                }
                else
                {
                    options.Add(line);
                }
            }
            return options;
        }
        else
        {
            return new List<string>();
        }
    }

    // Get nodaData comments, remove everything until closing square bracket and return
    public override string CurrentNodeNPCLine()
    {
        if (VD.nodeData != null)
        {
            if (!CurrentNodeIsPlayer())
            {
                string line = VD.nodeData.comments[VD.nodeData.commentIndex];
                if (line.Contains("]"))
                {
                    return line.Substring(line.IndexOf(']') + 1);
                }
                else
                {
                    return line;
                }
            }
        }
        return "";
    }

    public override AudioClip CurrentNodeNPCAudio()
    {
        if (VD.nodeData != null)
        {
            if (!CurrentNodeIsPlayer())
            {
                AudioClip clip = VD.nodeData.audios[VD.nodeData.commentIndex];
                return clip;
            }
        }
        return null;
    }

    // Get nodaData comments, find square brackets and return their content
    public override List<string> GetCurrentNodeTopics()
    {
        if (VD.nodeData != null)
        {
            if (!CurrentNodeIsPlayer())
            {
                return new List<string>();
            }
            List<string> topics = new List<string>();
            string[] comments = VD.nodeData.comments;
            for (int i = 0; i < comments.Length; i++)
            {
                if (comments[i].StartsWith("["))
                {
                    int index = comments[i].IndexOf("]");
                    if (index > 0)
                    {
                        topics.Add(comments[i].Substring(1, index - 1));
                    }
                }
                else
                {
                    topics.Add(comments[i]);
                }
            }
            return topics;
        }
        else
        {
            return new List<string>();
        }
    }

    public override void SelectOption(int optionId)
    {
        if (VD.nodeData != null)
        {
            Debug.Log("Selecting option " + optionId);
            VD.nodeData.commentIndex = optionId;
        }
    }

    public override void Next()
    {
        if (VD.nodeData != null)
        {
            VD.Next();
        }
    }

    public override float CurrentNodeThreshold()
    {
        if (VD.nodeData != null)
        {
            return (
                VD.nodeData.extraVars.ContainsKey("threshold") ?
                (float)VD.nodeData.extraVars["threshold"] :
                -1f
            );
        }
        else
        {
            return -1f;
        }
    }

}
