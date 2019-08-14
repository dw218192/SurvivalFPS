using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.Inventory;
using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    public static class GameAssetManager
    {
        private static readonly Dictionary<string, UnityEngine.Object> m_LoadedAssets = new Dictionary<string, UnityEngine.Object>();

        public static bool GetAsset<T>(string address, out T assetInstance) where T : UnityEngine.Object
        {
            assetInstance = null;
            if(m_LoadedAssets.ContainsKey(address))
            {
                UnityEngine.Object obj = m_LoadedAssets[address];

                if(obj is T)
                {
                    assetInstance = (T)obj;
                    return true;
                }

                Debug.LogWarningFormat("GameAssetManager-GetAsset: address is associated with type {0} but the given type is {1}.", obj.GetType().ToString(), typeof(T));
                return false;
            }

            return false;
        }
    }
}
