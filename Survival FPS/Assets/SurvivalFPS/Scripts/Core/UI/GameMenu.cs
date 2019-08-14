using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SurvivalFPS.Core.UI
{
    public abstract class GameMenu : MonoBehaviour
    {
        [SerializeField] protected List<Button> m_Buttons = new List<Button>();

        /// <summary>
        /// Init is called when the game starts.
        /// It is only called once.
        /// </summary>
        public virtual void Init()
        {
            
        }

        /// <summary>
        /// Scene-specific initialization.
        /// It is called every time a scene is loaded. (this excludes _preload and main menu)
        /// </summary>
        public virtual void SceneInit(Scene scene)
        {
            
        }

        public virtual void OnEnterMenu()
        {
            
        }

        public virtual void OnLeaveMenu()
        {
            
        }

        public virtual void OnBackPressed()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.CloseCurrentMenu();
            }
        }
    }

    [DisallowMultipleComponent]
    public abstract class GameMenu<MenuType> : GameMenu where MenuType : GameMenu<MenuType>
    {
        private static MenuType m_Instance;
        public static MenuType Instance { get { return m_Instance; } }

        public override void Init()
        {
            if (m_Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_Instance = (MenuType)this;
            }
        }

        /*
        protected virtual void Awake()
        {
            if (m_Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_Instance = (MenuType)this;
            }
        }
        */

        protected virtual void OnDestroy()
        {
            if (m_Instance == this)
            {
                m_Instance = null;
            }
        }
    }
}