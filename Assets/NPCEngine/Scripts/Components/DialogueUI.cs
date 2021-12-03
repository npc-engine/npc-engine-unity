using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NPCEngine.Components
{
    public class DialogueUI : MonoBehaviour
    {

        public GameObject uiObject;
        public int lineLimit = 10;


        [SerializeField]
        protected Text dialogueHistory;

        [SerializeField]
        protected Text dialogueTopics;

        public Color usernameColor = Color.cyan;
        public Color highlightColor = Color.yellow;


        private Camera cam;

        public void AddLine(ChatLine chatLine, bool highlight = false)
        {

            var line = !highlight ? chatLine.line : string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), chatLine.line);

            dialogueHistory.text += string.Format("\n<color=#{0}>{1}:</color>{2}", ColorUtility.ToHtmlStringRGBA(usernameColor), chatLine.speaker, line);

            var lines = dialogueHistory.text.Split('\n');

            if (lines.Length - 1 > lineLimit)
            {
                var linesList = new List<string>(lines);
                linesList.RemoveAt(0);
                dialogueHistory.text = string.Join("\n", linesList);
            }
        }

        public void UpdateTopics(List<string> topics)
        {
            dialogueTopics.text = string.Join("\n", topics);
        }

        public void Enable()
        {
            uiObject.SetActive(true);
            dialogueHistory.text = "";
            dialogueTopics.text = "";
        }

        public void Disable()
        {
            dialogueHistory.text = "";
            uiObject.SetActive(false);
        }

        private void Start()
        {
            uiObject.SetActive(false);
            cam = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(cam.gameObject.transform.position, Vector3.up);
            transform.localRotation *= Quaternion.Euler(0, 180, 0);
        }

    }
}