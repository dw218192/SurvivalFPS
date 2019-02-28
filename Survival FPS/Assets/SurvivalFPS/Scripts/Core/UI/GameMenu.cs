using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core.UI
{
    public abstract class GameMenu : MonoBehaviour
    {
        [SerializeField] protected List<Button> m_Buttons = new List<Button>();

        public virtual void Init()
        {
            
        }

        public virtual void OnEnterMenu()
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
        private static MenuType _instance;
        public static MenuType Instance { get { return _instance; } }

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = (MenuType)this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}