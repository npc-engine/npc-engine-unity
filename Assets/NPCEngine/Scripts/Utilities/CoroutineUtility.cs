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
    [ExecuteInEditMode]
    public class CoroutineUtility : Singleton<CoroutineUtility>
    {
#if UNITY_EDITOR
        private Dictionary<
            string,
            Dictionary<string, EditorCoroutine>
        > EditorCoroutines;
#endif
        private Dictionary<
            string,
            Dictionary<string, Coroutine>
        > Coroutines;

        private Dictionary<
            string,
            Dictionary<string, IEnumerator>
        > CoroutinesRunning;



        private void CheckInitVariables()
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

        private IEnumerator WrapCoroutine(MonoBehaviour behaviour, IEnumerator coroutine, string name)
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
            Instance.CheckInitVariables();
            if (!Instance.CoroutinesRunning.ContainsKey(GetGameObjectPath(owner)))
            {
                return false;
            }
            if (!Instance.CoroutinesRunning[GetGameObjectPath(owner)].ContainsKey(name))
            {
                return false;
            }
            return true;
        }

        void OnDisable()
        {
            StopAllEditorCoroutines();
        }

        public static void StartCoroutine(IEnumerator routine, MonoBehaviour owner, string id)
        {
            Instance.CheckInitVariables();
            if (!Instance.CoroutinesRunning.ContainsKey(GetGameObjectPath(owner)))
            {
                Instance.CoroutinesRunning.Add(GetGameObjectPath(owner), new Dictionary<string, IEnumerator>());
            }
            Instance.CoroutinesRunning[GetGameObjectPath(owner)][id] = routine;

            if (Application.isPlaying)
            {
                if (Instance.Coroutines.ContainsKey(GetGameObjectPath(owner)))
                {
                    if (Instance.Coroutines[GetGameObjectPath(owner)].ContainsKey(id))
                    {
                        try
                        {
                            owner.StopCoroutine(Instance.Coroutines[GetGameObjectPath(owner)][id]);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
                else
                {
                    Instance.Coroutines[GetGameObjectPath(owner)] = new Dictionary<string, Coroutine>();
                }
                Instance.Coroutines[GetGameObjectPath(owner)][id] = owner.StartCoroutine(Instance.WrapCoroutine(owner, routine, id));

            }
            else
            {
                
#if UNITY_EDITOR

                if (Instance.EditorCoroutines.ContainsKey(GetGameObjectPath(owner)))
                {
                    if (Instance.EditorCoroutines[GetGameObjectPath(owner)].ContainsKey(id))
                    {
                        try
                        {
                            EditorCoroutineUtility.StopCoroutine(Instance.EditorCoroutines[GetGameObjectPath(owner)][id]);
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
                    foreach(var kvp in Instance.EditorCoroutines)
                    {
                        log += ", " + kvp.Key + " " + id;
                    }
                    Instance.EditorCoroutines[GetGameObjectPath(owner)] = new Dictionary<string, EditorCoroutine>();
                }
                Instance.EditorCoroutines[GetGameObjectPath(owner)][id] = EditorCoroutineUtility.StartCoroutineOwnerless(Instance.WrapCoroutine(owner, routine, id));
#endif
            }

        }

        public static void StopCoroutine(string id, MonoBehaviour owner)
        {
            Instance.CheckInitVariables();
            if (Instance.CoroutinesRunning.ContainsKey(GetGameObjectPath(owner)))
            {
                Instance.CoroutinesRunning[GetGameObjectPath(owner)].Remove(id);
            }
            if (Application.isPlaying)
            {
                if (Instance.Coroutines.ContainsKey(GetGameObjectPath(owner)))
                {
                    if (Instance.Coroutines[GetGameObjectPath(owner)].ContainsKey(id))
                    {
                        try
                        {
                            owner.StopCoroutine(Instance.Coroutines[GetGameObjectPath(owner)][id]);
                        }
                        catch (Exception)
                        {
                        }
                        Instance.Coroutines[GetGameObjectPath(owner)].Remove(id);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                if (Instance.EditorCoroutines.ContainsKey(GetGameObjectPath(owner)))
                {
                    if (Instance.EditorCoroutines[GetGameObjectPath(owner)].ContainsKey(id))
                    {
                        try
                        {
                            EditorCoroutineUtility.StopCoroutine(Instance.EditorCoroutines[GetGameObjectPath(owner)][id]);
                        }
                        catch (Exception)
                        {
                        }
                        Instance.EditorCoroutines[GetGameObjectPath(owner)].Remove(id);
                    }
                }
#endif
            }

        }

        public static void StopAllPlayingCoroutines(MonoBehaviour owner)
        {
            Instance.CheckInitVariables();
            if (Instance.Coroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                foreach (var coroutine in Instance.Coroutines[GetGameObjectPath(owner)])
                {
                    try
                    {
                        owner.StopCoroutine(coroutine.Value);
                    }
                    catch (Exception)
                    { }
                }
                Instance.Coroutines[GetGameObjectPath(owner)].Clear();
            }

        }

        public static void StopAllEditorCoroutines()
        {
            Instance.CheckInitVariables();
#if UNITY_EDITOR
            foreach (var owner in Instance.EditorCoroutines)
            {
                foreach (var coroutine in owner.Value)
                {
                    EditorCoroutineUtility.StopCoroutine(coroutine.Value);
                }
            }
            Instance.EditorCoroutines.Clear();
#endif
        }

        public static void StopAllEditorCoroutines(MonoBehaviour owner)
        {
            Instance.CheckInitVariables();
#if UNITY_EDITOR
            if (Instance.EditorCoroutines.ContainsKey(GetGameObjectPath(owner)))
            {
                foreach (var coroutine in Instance.EditorCoroutines[GetGameObjectPath(owner)])
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
                yield return new EditorWaitForSeconds(seconds);
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
    }
}