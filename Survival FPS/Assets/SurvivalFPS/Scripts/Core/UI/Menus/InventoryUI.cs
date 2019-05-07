using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.UI
{
    public class InventoryUI : GameMenu<InventoryUI>
    {
        public static event Action inventoryMenuOpened;
        public static event Action inventoryMenuClosed;

        public override void Init()
        {
            base.Init();
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();

            inventoryMenuOpened();
        }

        public override void OnLeaveMenu()
        {
            base.OnLeaveMenu();

            inventoryMenuClosed();
        }
    }
}
