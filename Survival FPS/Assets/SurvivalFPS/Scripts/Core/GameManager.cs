using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using SurvivalFPS.Core.Audio;
using SurvivalFPS.Core.UI;
using SurvivalFPS.Core.Weapon;
using SurvivalFPS.Utility;
using SurvivalFPS.Core.LevelManagement;

namespace SurvivalFPS
{
    public class SceneSpecificManagerAttribute : Attribute { }

    public class GameManager : SingletonBehaviour<GameManager>
    {
        [Serializable]
        private class SceneSetup
        {
            [Header("Persistent Managers")]
            [SerializeField] private AudioManager m_AudioManager;
            [SerializeField] private MenuManager m_MenuManager;
            [Header("Scene Specific")]
            [SerializeField] private GameSceneManager m_GameSceneManager;
            [SerializeField] private BulletHoleManager m_BulletHoleManager;
            [SerializeField] private EventSystem m_EventSystem;

            public void Setup()
            {
                //singleton managers
                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = GetType().GetFields(bindingFlags);

                foreach (FieldInfo field in fields)
                {
                    UnityEngine.Object managerPrefab = field.GetValue(this) as UnityEngine.Object;

                    if (managerPrefab != null && !FindObjectOfType(managerPrefab.GetType()))
                    {
                        //if it's the preload scene or the main menu, don't instantiate scene specific managers
                        if(SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().buildIndex == 1
                           && managerPrefab.GetType().IsDefined(typeof(SceneSpecificManagerAttribute), true))
                        {
                            continue;
                        }

                        Instantiate(managerPrefab);
                    }
                }

                //initialization complete, jump out of the preload scene
                if(SceneManager.GetActiveScene().buildIndex == 0)
                {
                    LevelLoader.LoadNextScene();
                    return;
                }

                if(!FindObjectOfType(m_EventSystem.GetType()))
                {
                    Instantiate(m_EventSystem);
                }
            }
        }

        [SerializeField] private SceneSetup m_SceneSetup;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            m_SceneSetup.Setup();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            m_SceneSetup.Setup();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            //user input is not allowed in preload
            if(SceneManager.GetActiveScene().buildIndex == 0)
            {
                return;
            }

            MenuManager menuManager = MenuManager.Instance;
            GameMenu activemenu = menuManager.GetActiveMenu();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (activemenu != null)
                {
                    if(activemenu == PauseMenu.Instance)
                    {
                        PauseMenu.Instance.OnResumePressed();
                    }
                    else
                    {
                        menuManager.CloseCurrentMenu();
                    }
                }
                else
                {
                    menuManager.OpenMenu(PauseMenu.Instance);
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (activemenu != null && activemenu == InventoryUI.Instance)
                {
                    menuManager.CloseCurrentMenu();
                }
                else
                {
                    menuManager.OpenMenu(InventoryUI.Instance);
                }
            }
        }

        //check if the scene is a game level
        public bool IsGameLevel(int sceneIndex)
        {
            return (sceneIndex != 0 && sceneIndex != 1);
        }
    }
}
