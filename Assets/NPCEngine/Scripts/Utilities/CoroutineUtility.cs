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
            MonoBehaviour,
            Dictionary<string, EditorCoroutine>
        > EditorCoroutines = new Dictionary<MonoBehaviour, Dictionary<string, EditorCoroutine>>();
#endif
        private Dictionary<
            MonoBehaviour,
            Dictionary<string, Coroutine>
        > Coroutines = new Dictionary<MonoBehaviour, Dictionary<string, Coroutine>>();

        private Dictionary<
            MonoBehaviour,
            Dictionary<string, IEnumerator>
        > CoroutinesRunning = new Dictionary<MonoBehaviour, Dictionary<string, IEnumerator>>();

        private IEnumerator WrapCoroutine(MonoBehaviour behaviour, IEnumerator coroutine, string name)
        {
            if (!CoroutinesRunning.ContainsKey(behaviour))
            {
                CoroutinesRunning.Add(behaviour, new Dictionary<string, IEnumerator>());
            }
            CoroutinesRunning[behaviour][name] = coroutine;
            yield return coroutine;
            CoroutinesRunning[behaviour].Remove(name);
        }

        public static bool IsRunning(MonoBehaviour owner, string name)
        {
            if (!Instance.CoroutinesRunning.ContainsKey(owner))
            {
                return false;
            }
            if (!Instance.CoroutinesRunning[owner].ContainsKey(name))
            {
                return false;
            }
            return true;
        }

        public static void StartCoroutine(IEnumerator routine, MonoBehaviour owner, string id)
        {
            if (!Instance.CoroutinesRunning.ContainsKey(owner))
            {
                Instance.CoroutinesRunning.Add(owner, new Dictionary<string, IEnumerator>());
            }
            Instance.CoroutinesRunning[owner][id] = routine;

            if (Application.isPlaying)
            {
                if (Instance.Coroutines.ContainsKey(owner))
                {
                    if (Instance.Coroutines[owner].ContainsKey(id))
                    {
                        try
                        {
                            owner.StopCoroutine(Instance.Coroutines[owner][id]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    Instance.Coroutines[owner] = new Dictionary<string, Coroutine>();
                }
                Instance.Coroutines[owner][id] = owner.StartCoroutine(Instance.WrapCoroutine(owner, routine, id));

            }
            else
            {
#if UNITY_EDITOR
                if (Instance.EditorCoroutines.ContainsKey(owner))
                {
                    if (Instance.EditorCoroutines[owner].ContainsKey(id))
                    {
                        try
                        {
                            EditorCoroutineUtility.StopCoroutine(Instance.EditorCoroutines[owner][id]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    Instance.EditorCoroutines[owner] = new Dictionary<string, EditorCoroutine>();
                }
                Instance.EditorCoroutines[owner][id] = EditorCoroutineUtility.StartCoroutineOwnerless(Instance.WrapCoroutine(owner, routine, id));
#endif
            }

        }

        public static void StopCoroutine(string id, MonoBehaviour owner)
        {
            if (Instance.CoroutinesRunning.ContainsKey(owner))
            {
                Instance.CoroutinesRunning[owner].Remove(id);
            }
            if (Application.isPlaying)
            {
                if (Instance.Coroutines.ContainsKey(owner))
                {
                    if (Instance.Coroutines[owner].ContainsKey(id))
                    {
                        try
                        {
                            owner.StopCoroutine(Instance.Coroutines[owner][id]);
                        }
                        catch (Exception)
                        {
                        }
                        Instance.Coroutines[owner].Remove(id);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                if (Instance.EditorCoroutines.ContainsKey(owner))
                {
                    if (Instance.EditorCoroutines[owner].ContainsKey(id))
                    {
                        try
                        {
                            EditorCoroutineUtility.StopCoroutine(Instance.EditorCoroutines[owner][id]);
                        }
                        catch (Exception)
                        {
                        }
                        Instance.EditorCoroutines[owner].Remove(id);
                    }
                }
#endif
            }

        }

        public static void StopAllPlayingCoroutines(MonoBehaviour owner)
        {
            if (Instance.Coroutines.ContainsKey(owner))
            {
                foreach (var coroutine in Instance.Coroutines[owner])
                {
                    try
                    {
                        owner.StopCoroutine(coroutine.Value);
                    }
                    catch (Exception)
                    { }
                }
                Instance.Coroutines[owner].Clear();
            }

        }

        public static void StopAllEditorCoroutines(MonoBehaviour owner)
        {
#if UNITY_EDITOR
            if (Instance.EditorCoroutines.ContainsKey(owner))
            {
                foreach (var coroutine in Instance.EditorCoroutines[owner])
                {
                    try
                    {
                        EditorCoroutineUtility.StopCoroutine(coroutine.Value);
                    }
                    catch (Exception)
                    { }
                }
                Instance.EditorCoroutines[owner].Clear();
            }
#endif
        }

        public static new void StopAllCoroutines()
        {
            foreach (var owner in Instance.Coroutines.Keys)
            {
                StopAllPlayingCoroutines(owner);
            }
#if UNITY_EDITOR
            foreach (var owner in Instance.EditorCoroutines.Keys)
            {
                StopAllEditorCoroutines(owner);
            }
#endif
        }

    }


}