using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.LevelManagement;


namespace SurvivalFPS.Core.UI
{
    public class MainMenu : GameMenu<MainMenu>
    {
        public void OnPlayPressed()
        {
            MenuManager.Instance.CloseCurrentMenu();
            LevelLoader.LoadNextSceneAsync();
        }

        public void OnQuitPressed()
        {
            Application.Quit();
        }
    }
}