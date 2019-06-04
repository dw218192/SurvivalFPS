using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS.Core.UI
{
    public class ConsumableOptionMenu : ItemOptionMenu
    {
        [SerializeField] private Button m_UseButton;
        [SerializeField] private Button m_TrashButton;

        public override void SetupMenu(ItemInstance item)
        {
            InventoryItemTemplate inventoryItemTemplate = item.itemTemplate;

            m_UseButton.onClick.AddListener(() => inventoryItemTemplate.Use(m_Player, item));
            m_TrashButton.onClick.AddListener(() => m_Player.inventorySystem.RemoveItem(inventoryItemTemplate.sectionType, item.inventoryIndex));
        }
    }
}
