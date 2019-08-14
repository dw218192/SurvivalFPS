using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core.UI
{
    public class MenuManager : SingletonBehaviour<MenuManager>
    {
        [SerializeField] private MainMenu m_MainMenuPrefab;
        [SerializeField] private SettingMenu m_SettingsMenuPrefab;
        [SerializeField] private PauseMenu m_PauseMenuPrefab;
        [SerializeField] private LoadingScreen m_LoadingScreenPrefab;
        [SerializeField] private InventoryUI m_InventoryMenuPrefab;
        //[SerializeField] private WinScreen winScreen;

        List<GameMenu> m_MenuInstances = new List<GameMenu>();

        private Stack<GameMenu> m_MenuStack = new Stack<GameMenu>();

        public void CloseCurrentMenu()
        {
            if (m_MenuStack.Count == 0)
            {
                Debug.LogWarning("MenuManager.CloseCurrentMenu()- there is no menu in the stack");
                return;
            }

            GameMenu topMenu = m_MenuStack.Pop();
            topMenu.OnLeaveMenu();
            topMenu.gameObject.SetActive(false);

            if (m_MenuStack.Count > 0)
            {
                GameMenu nextMenu = m_MenuStack.Peek();
                nextMenu.gameObject.SetActive(true);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
            InitializeMenus();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void InitializeMenus()
        {
            BindingFlags myFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = this.GetType().GetFields(myFlags);

            foreach (FieldInfo field in fields)
            {
                GameMenu prefab = field.GetValue(this) as GameMenu;

                if (prefab != null)
                {
                    GameMenu menuInstance = Instantiate(prefab, transform);

                    m_MenuInstances.Add(menuInstance);
                    menuInstance.Init();
                }
            }

            foreach (GameMenu menuInstance in m_MenuInstances)
            {
                if (menuInstance != MainMenu.Instance)
                {
                    menuInstance.gameObject.SetActive(false);
                }
                else
                {
                    OpenMenu(menuInstance);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!GameManager.Instance.IsGameLevel(scene.buildIndex)) return;
            
            foreach (GameMenu menuInstance in m_MenuInstances)
            {
                menuInstance.SceneInit(scene);
            }
        }

        public void OpenMenu(GameMenu menuInstance)
        {
            if (menuInstance == null)
            {
                Debug.LogWarning("MenuManager.OpenMenu()- menu instance is null");
                return;
            }

            if (m_MenuStack.Count > 0)
            {
                foreach (GameMenu menu in m_MenuStack)
                {
                    menu.gameObject.SetActive(false);
                }
            }

            menuInstance.gameObject.SetActive(true);
            menuInstance.OnEnterMenu();
            m_MenuStack.Push(menuInstance);
        }

        public GameMenu GetActiveMenu()
        {
            if (m_MenuStack.Count > 0 && m_MenuStack.Peek().gameObject.activeSelf)
            {
                return m_MenuStack.Peek();
            }
            else
            {
                return null;
            }
        }
    }
}