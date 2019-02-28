using System;
using UnityEngine;

using SurvivalFPS.Core.LevelManagement;

namespace SurvivalFPS.Core.UI
{
    public class PauseMenu : GameMenu<PauseMenu>
    {
        //events
        public static event Action gamePaused;
        public static event Action gameResumed;

        public override void Init()
        {
            base.Init();
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
            Time.timeScale = 0;

            gamePaused();
        }

        public void OnResumePressed()
        {
            Time.timeScale = 1;
            base.OnBackPressed();

            gameResumed();
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

        public void OnQuitPressed()
        {
            Application.Quit();
        }
    }
}