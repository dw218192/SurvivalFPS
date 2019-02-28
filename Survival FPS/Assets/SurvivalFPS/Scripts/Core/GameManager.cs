﻿using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using SurvivalFPS.Core.Audio;
using SurvivalFPS.Core.UI;
using SurvivalFPS.Core.Weapon;
using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        [Serializable]
        private class SceneSetup
        {
            [Header("Persistent Managers")]
            [SerializeField] private AudioManager m_AudioManager;
            [SerializeField] private MenuManager m_MenuManager;
            [Header("Scene Managers")]
            [SerializeField] private BulletHoleManager m_BulletHoleManager;
            [Header("Other")]
            [SerializeField] private EventSystem m_EventSystem;

            public void SetupManagers()
            {
                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = GetType().GetFields(bindingFlags);

                foreach (FieldInfo field in fields)
                {
                    UnityEngine.Object managerPrefab = field.GetValue(this) as UnityEngine.Object;

                    if (managerPrefab != null && !UnityEngine.Object.FindObjectOfType(managerPrefab.GetType()))
                    {
                        UnityEngine.Object.Instantiate(managerPrefab);
                    }
                }
            }
        }

        [SerializeField] private SceneSetup m_SceneSetup;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            m_SceneSetup.SetupManagers();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            m_SceneSetup.SetupManagers();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                MenuManager menuManager = MenuManager.Instance;
                if (menuManager.GetActiveMenu() != PauseMenu.Instance)
                {
                    MenuManager.Instance.OpenMenu(PauseMenu.Instance);
                }
                else
                {
                    PauseMenu.Instance.OnResumePressed();
                }
            }
        }
    }
}
