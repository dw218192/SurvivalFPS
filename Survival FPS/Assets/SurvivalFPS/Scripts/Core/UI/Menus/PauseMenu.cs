using System;
using UnityEngine;

using SurvivalFPS.Core.LevelManagement;
using SurvivalFPS.Messaging;

namespace SurvivalFPS.Core.UI
{
    public class PauseMenu : GameMenu<PauseMenu>
    {
        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
            Time.timeScale = 0;

            Messenger.Broadcast(M_EventType.OnGamePaused);
        }

        public void OnResumePressed()
        {
            Time.timeScale = 1;
            base.OnBackPressed();

            Messenger.Broadcast(M_EventType.OnGameResumed);
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1;
            LevelLoader.ReloadCurScene();
            base.OnBackPressed();
        }

        public void OnMainMenuPressed()
        {
            Time.timeScale = 1;
            LevelLoader.LoadMainMenuScene();
            base.OnBackPressed();
        }

        public void OnSavePressed()
        {
            Messenger.Broadcast(M_EventType.OnGameSaving);
        }

        public void OnQuitPressed()
        {
            Application.Quit();
        }
    }
}