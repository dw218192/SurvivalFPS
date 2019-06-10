using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    /// <summary>
    /// This attribute makes the game asset referenced by the GameAssetManager
    /// and able to be retrived through the manager during runtime
    /// </summary>
    public class ManagedGameDataAssetAttribute : Attribute { }

    public static class GameAssetManager
    {
        //mapping between (asset runtime type - file name) < -- > asset reference
        private static readonly Dictionary<Pair<string, string>, ScriptableObject> m_ScriptableObjects = new Dictionary<Pair<string, string>, ScriptableObject>();

        public static void Init()
        {
            ResourceRequest request = Resources.LoadAsync("GameData", typeof(ScriptableObject));
            GameManager.Instance.StartCoroutine(ResourceLoadRoutine(request));
        }

        private static IEnumerator ResourceLoadRoutine(ResourceRequest request)
        {
            yield return new WaitUntil(() => request.isDone);

            ScriptableObject[] scriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();

            foreach (ScriptableObject so in scriptableObjects)
            {
                if (so.GetType().IsDefined(typeof(ManagedGameDataAssetAttribute), true))
                {
                    AddScriptableObject(so);
                    Debug.LogFormat("GameAssetManager: added {0} of type {1}", so.name, so.GetType());
                }
            }

            Resources.UnloadUnusedAssets();
        }

        private static void AddScriptableObject(ScriptableObject scriptableObject)
        {
            Pair<string, string> key = new Pair<string, string>(scriptableObject.GetType().ToString(), scriptableObject.name);
            ScriptableObject so;
            if(!m_ScriptableObjects.TryGetValue(key, out so))
            {
                m_ScriptableObjects.Add(key, so);
            }
            else
            {
                Debug.LogWarningFormat("GameAssetManager: attempting to add a scriptable object of name {0} and of type {1} that has already been added", scriptableObject.name, scriptableObject.GetType().ToString());
            }
        }

        public static T GetScriptableObject<T>(string name) where T : ScriptableObject
        {
            Pair<string, string> key = new Pair<string, string>(typeof(T).ToString(), name);
            ScriptableObject so;
            if (m_ScriptableObjects.TryGetValue(key, out so))
            {
                if(so.GetType() != typeof(T))
                {
                    Debug.LogWarningFormat("GameAssetManager: attempting to get a scriptable object of name {0} and of type {1} that is already added, " +
                        "which conflicts with the type {2} in the asset table", name, typeof(T).ToString(), so.GetType().ToString());
                    return null;
                }
                else
                {
                    return (T)so;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
