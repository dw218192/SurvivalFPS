using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core.UI
{
    public class MenuManager : SingletonBehaviour<MenuManager>
    {
        [SerializeField] private MainMenu m_MainMenuPrefab;
        [SerializeField] private SettingMenu m_SettingsMenuPrefab;
        [SerializeField] private PauseMenu m_PauseMenu;
        [SerializeField] private LoadingScreen m_LoadingScreenPrefab;
        //[SerializeField] private WinScreen winScreen;

        private Transform m_MenuParent;
        private Stack<GameMenu> m_MenuStack = new Stack<GameMenu>();

        public void CloseCurrentMenu()
        {
            if (m_MenuStack.Count == 0)
            {
                Debug.LogWarning("MenuManager.CloseCurrentMenu()- there is no menu in the stack");
                return;
            }

            GameMenu topMenu = m_MenuStack.Pop();
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
            if (m_MenuParent == null)
            {
                GameObject menuParentObject = new GameObject("menus");
                m_MenuParent = menuParentObject.transform;
            }

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(m_MenuParent.gameObject);

            InitializeMenus();
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
                    GameMenu menuInstance = Instantiate(prefab, m_MenuParent);
                    menuInstance.Init();

                    if (prefab != m_MainMenuPrefab)
                    {
                        menuInstance.gameObject.SetActive(false);
                    }
                    else
                    {
                        OpenMenu(menuInstance);
                    }
                }
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