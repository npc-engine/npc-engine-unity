using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace NPCEngine.Utility
{
    /// <summary>
    /// Utility that allows to run coroutines in a coherent way both in editor and in player modes.
    /// </summary>
    [ExecuteAlways]
    public class CoroutineUtility: Singleton<CoroutineUtility>
    {
#if UNITY_EDITOR
        private static Dictionary<
            string,
            Dictionary<string, EditorCoroutine>
        > EditorCoroutines;
#endif
        private static Dictionary<
            string,
            Dictionary<string, Coroutine>
        > Coroutines;

        private static Dictionary<
            string,
            Dictionary<string, IEnumerator>
        > CoroutinesRunning;

        private static List<MonoBehaviour> owners;

        private static void CheckInitVariables()
        {
            if (Coroutines == null)
            {
                Coroutines = new Dictionary<string, Dictionary<string, Coroutine>>();
            }
            if (CoroutinesRunning == null)
            {
                CoroutinesRunning = new Dictionary<string, Dictionary<string, IEnumerator>>();
            }
#if UNITY_EDITOR
            if (EditorCoroutines == null)
            {
                EditorCoroutines = new Dictionary<string, Dictionary<string, EditorCoroutine>>();
            }
#endif
        }

        private static IEnumerator WrapCoroutine(MonoBehaviour behaviour, IEnumerator coroutine, string name)
        {

            if (!CoroutinesRunning.ContainsKey(GetGameObjectPath(behaviour)))
            {
                CoroutinesRunning.Add(GetGameObjectPath(behaviour), new Dictionary<string, IEnumerator>());
            }
            CoroutinesRunning[GetGameObjectPath(behaviour)][name] = coroutine;
            yield return coroutine;
            CoroutinesRunning[GetGameObjectPath(behaviour)].Remove(name);
        }

        public static bool IsRunning(MonoBehaviour owner, string name)
        {
            CheckInitVariables();
            if (!CoroutinesRunning.ContainsKey(GetGameObjectPath(owner)))
            {
                return false;
            }
            if (!CoroutinesRunning[GetGameObjectPath(owner)].ContainsKey(name))
            {
                return false;
            }
            return true;
        }

        void OnDisable()
        {
            CheckInitVariables();
            if(owners == null)
            {
                owners = new List<MonoBehaviour>();
            }
            StopAllEditorCoroutines();
            if(Application.isPlaying)
            {
                foreach(var owner in owners)
                {
                    if(owner != null)
                    {
                        StopAllPlayingCoroutines(owner);
                    }
                }
                owners.Clear();
                Coroutines.Clear();
                CoroutinesRunning.Clear();
            }
        }

        void OnEnable()
        {
            CheckInitVariables();
            if(owners == null)
            {
                owners = new List<MonoBehaviour>();
            }
            StopAllEditorCoroutines();
            if(Application.isPlaying)
            {
                foreach(var owner in owners)
                {
                    if(owner != null)
                    {
                        StopAllPlayingCoroutines(owner);
                    }
                }
                owners.Clear();
                Coroutines.Clear();
                CoroutinesRunning.Clear();
            }
            else
            {
                StopAllEditorCoroutines();
            }

        }

        public static void StartCoroutine(IEnumerator routine, MonoBehaviour owner, string id)
        {
            CheckInitVariables();
            if (owner == null)
            {
                return;
            }
            owners.Add(owner);
            if (!CoroutinesRunning.ContainsKey(GetGameObjectPath(owner)))
            {
                CoroutinesRunning.Add(GetGameObjectPath(owner), new Dictionary<string, IEnumerator>());
            }
            CoroutinesRunning[GetGameObjectPath(owner)][id] = routine;

            if (Application.isPlaying)
            {
                StartCouroutinePlayMode(routine, owner, id);
            }
            else
            {
                StartCoroutineEditMode(routine, owner, id);
            }
        }

        private static void StartCouroutinePlayMode(IEnumerator routine, MonoBehaviour owner, string id)
        {
            if (Coroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                if (Coroutines[GetGameObjectPath(owner)].ContainsKey(id))
                {
                    try
                    {
                        owner.StopCoroutine(Coroutines[GetGameObjectPath(owner)][id]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
            {
                Coroutines[GetGameObjectPath(owner)] = new Dictionary<string, Coroutine>();
            }
            Coroutines[GetGameObjectPath(owner)][id] = owner.StartCoroutine(WrapCoroutine(owner, routine, id));
        }

        private static void StartCoroutineEditMode(IEnumerator routine, MonoBehaviour owner, string id)
        {
#if UNITY_EDITOR
            if (owner == null)
            {
                return;
            }
            if (EditorCoroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                if (EditorCoroutines[GetGameObjectPath(owner)].ContainsKey(id))
                {
                    try
                    {
                        EditorCoroutineUtility.StopCoroutine(EditorCoroutines[GetGameObjectPath(owner)][id]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
            {
                string log = "";
                foreach(var kvp in EditorCoroutines)
                {
                    log += ", " + kvp.Key + " " + id;
                }
                EditorCoroutines[GetGameObjectPath(owner)] = new Dictionary<string, EditorCoroutine>();
            }
            EditorCoroutines[GetGameObjectPath(owner)][id] = EditorCoroutineUtility.StartCoroutine(WrapCoroutine(owner, routine, id), owner);
#endif
        }

        public static void StopCoroutine(string id, MonoBehaviour owner)
        {
            CheckInitVariables();
            if (CoroutinesRunning.ContainsKey(GetGameObjectPath(owner)))
            {
                CoroutinesRunning[GetGameObjectPath(owner)].Remove(id);
            }
            if (Application.isPlaying)
            {
                StopCoroutinePlayMode(id, owner);
            }
            else
            {
                StopCoroutineEditMode(id, owner);
            }
        }

        private static void StopCoroutinePlayMode(string id, MonoBehaviour owner)
        {
            if (Coroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                if (Coroutines[GetGameObjectPath(owner)].ContainsKey(id))
                {
                    try
                    {
                        owner.StopCoroutine(Coroutines[GetGameObjectPath(owner)][id]);
                    }
                    catch (Exception)
                    {
                    }
                    Coroutines[GetGameObjectPath(owner)].Remove(id);
                }
            }
        }

        private static void StopCoroutineEditMode(string id, MonoBehaviour owner)
        {
#if UNITY_EDITOR
            if (EditorCoroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                if (EditorCoroutines[GetGameObjectPath(owner)].ContainsKey(id))
                {
                    try
                    {
                        EditorCoroutineUtility.StopCoroutine(EditorCoroutines[GetGameObjectPath(owner)][id]);
                    }
                    catch (Exception)
                    {
                    }
                    EditorCoroutines[GetGameObjectPath(owner)].Remove(id);
                }
            }
#endif
        }

        public static void StopAllPlayingCoroutines(MonoBehaviour owner)
        {
            CheckInitVariables();
            if (Coroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                foreach (var coroutine in Coroutines[GetGameObjectPath(owner)])
                {
                    owner.StopCoroutine(coroutine.Value);
                }
                Coroutines[GetGameObjectPath(owner)].Clear();
            }

        }

        public static void StopAllEditorCoroutines()
        {
            CheckInitVariables();
#if UNITY_EDITOR
            foreach (var owner in EditorCoroutines)
            {
                foreach (var coroutine in owner.Value)
                {
                    EditorCoroutineUtility.StopCoroutine(coroutine.Value);
                }
            }
            EditorCoroutines.Clear();
#endif
        }


        

        public static void StopAllEditorCoroutines(MonoBehaviour owner)
        {
            CheckInitVariables();
#if UNITY_EDITOR
            if (EditorCoroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                foreach (var coroutine in EditorCoroutines[GetGameObjectPath(owner)])
                {
                    EditorCoroutineUtility.StopCoroutine(coroutine.Value);
                }
            }
#endif
        }

        public static IEnumerator WaitForSeconds(float seconds)
        {
            if(Application.isPlaying)
            {
                yield return new WaitForSeconds(seconds);
            }
            else
            {
                #if UNITY_EDITOR
                yield return new EditorWaitForSeconds(seconds);
                #endif
            }
        }
       
        public static string GetGameObjectPath(MonoBehaviour behaviour)
        {
            GameObject obj = behaviour.gameObject;
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            path = path + "/" + behaviour.GetType().Name;
            return path;
        }

        [ContextMenu("Check Coroutines")]
        public void PrintCoroutines()
        {
            CheckInitVariables();
            foreach (var kvp in Coroutines)
            {
                string coroutines_string = "";
                foreach (var kvp2 in kvp.Value)
                {
                    coroutines_string += ", " + kvp2.Key;
                }
                Debug.LogFormat("Object: {0} Coroutines: {1}", kvp.Key, coroutines_string);
            }
        }

        [ContextMenu("Check Editor Coroutines")]
        public void PrintEditorCoroutines()
        {
            CheckInitVariables();
            #if UNITY_EDITOR
            foreach (var kvp in EditorCoroutines)
            {
                string coroutines_string = "";
                foreach (var kvp2 in kvp.Value)
                {
                    coroutines_string += ", " + kvp2.Key;
                }
                Debug.LogFormat("Object: {0} Coroutines: {1}", kvp.Key, coroutines_string);
            }
            #endif
        }

        [ContextMenu("Check Running Coroutines")]
        public void PrintRunningCoroutines()
        {
            CheckInitVariables();
            foreach (var kvp in CoroutinesRunning)
            {
                string coroutines_string = "";
                foreach (var kvp2 in kvp.Value)
                {
                    coroutines_string += ", " + kvp2.Key;
                }
                Debug.LogFormat("Object: {0} Coroutines: {1}", kvp.Key, coroutines_string);
            }
        }
    }
}