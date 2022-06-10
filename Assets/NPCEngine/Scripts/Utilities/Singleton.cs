using UnityEngine;

namespace NPCEngine.Utility
{
    /// <summary>
    /// Singleton <c>MonoBehaviour</c> base class. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        var subsysObj = GameObject.Find("Subsystems");
                        if (subsysObj == null)
                        {
                            subsysObj = new GameObject("Subsystems");
                        }
                        GameObject singletonObject = new GameObject(typeof(T).ToString());
                        singletonObject.transform.parent = subsysObj.transform;
                        instance = singletonObject.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        public static T GetInstance()
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    var subsysObj = GameObject.Find("Subsystems");
                    if (subsysObj == null)
                    {
                        subsysObj = new GameObject("Subsystems");
                    }
                    GameObject singletonObject = new GameObject(typeof(T).ToString());
                    singletonObject.transform.parent = subsysObj.transform;
                    instance = singletonObject.AddComponent<T>();
                }
            }

            return instance;
        }

    }
}
