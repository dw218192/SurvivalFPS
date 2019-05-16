using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS.Core.UI
{
    public abstract class ItemOptionMenu : MonoBehaviour
    {
        public abstract void SetupMenu(InventoryItemTemplate inventoryItemTemplate);
    }
}