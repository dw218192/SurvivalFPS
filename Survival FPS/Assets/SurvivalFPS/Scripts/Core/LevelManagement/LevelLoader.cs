using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SurvivalFPS.Core.UI;

namespace SurvivalFPS.Core.LevelManagement
{
    public static class LevelLoader
    {
        private static int MainMenuIndex = 1;

        public static void ReloadCurScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static void LoadNextScene()
        {
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            LoadScene(curSceneIndex + 1);
        }

        public static void LoadNextSceneAsync()
        {
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            LoadSceneAsync(curSceneIndex + 1);
        }

        public static void LoadMainMenuScene()
        {
            SceneManager.LoadScene(MainMenuIndex);
            MenuManager.Instance.OpenMenu(MainMenu.Instance);
        }

        public static void LoadScene(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(levelIndex);
            }
            else
            {
                Debug.LogWarning("LevelLoader.LoadScene(): invalid scene specified!");
            }
        }

        // start the coroutine to load asynchronously and generate a progress bar prefab
        public static void LoadSceneAsync(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < SceneManager.sceneCountInBuildSettings)
            {
                GameManager.Instance.StartCoroutine(LoadLevelAsyncRoutine(levelIndex));
            }
            else
            {
                Debug.LogWarning("LevelLoader.LoadSceneAsync(): invalid scene specified!");
            }
        }

        // load a level asynchronously and update the load progress bar
        private static IEnumerator LoadLevelAsyncRoutine(int levelIndex)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelIndex);
            asyncLoad.allowSceneActivation = false;

            LoadingScreen loadingScreen = LoadingScreen.Instance;

            if (loadingScreen != null)
            {
                MenuManager.Instance.OpenMenu(loadingScreen);

                while (loadingScreen.currentProgress < 0.9f)
                {
                    loadingScreen.UpdateProgress(asyncLoad.progress);
                    yield return null;
                }

                loadingScreen.UpdateProgress(1.0f);

                MenuManager.Instance.CloseCurrentMenu();
            }
            else
            {
                Debug.LogWarning("LevelLoader.LoadLevelAsyncRoutine(): loadingScreen is null!");
            }

            asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }
}
