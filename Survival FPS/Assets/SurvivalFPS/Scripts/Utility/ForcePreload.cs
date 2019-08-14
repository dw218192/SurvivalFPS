using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalFPS.Utility
{
    // this should run absolutely first; use script-execution-order to do so.
    public class ForcePreload : MonoBehaviour
    {
        void Awake()
        {
            GameObject check = GameObject.Find("__app");
            if (check == null)
            {
                Debug.Log("loading preload scene...");

                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.LoadScene("_preload"); 
            }
        }
    }
}
