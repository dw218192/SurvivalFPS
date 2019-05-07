using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SurvivalFPS.Core.PlayerInteraction;
using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS.Core.FPS
{
    public class PlayerInventorySystem : MonoBehaviour
    {
        [SerializeField] private int m_InventorySize;
        private List<ItemInstance> m_Items;

        private void Awake()
        {
            m_Items = new List<ItemInstance>();

            for (int i = 0; i < m_InventorySize; i ++)
            {
                m_Items.Add(null);
            }
        }

        /// <summary>
        /// Removes the item entirely.
        /// </summary>
        public void RemoveItem(int index)
        {
            m_Items[index] = null;
        }

        /// <summary>
        /// Adds the item to the player inventory.
        /// </summary>
        /// <returns>The item inventory id if the item can be added. -1 if the item cannot be added </returns>
        public int AddItem(ItemInstance item)
        {
            int emptyIndex = FindEmptySlot();

            if (emptyIndex != -1) 
            {
                m_Items[emptyIndex] = item;
            }

            return emptyIndex;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < m_InventorySize; i++)
            {
                if (m_Items[i] == null) return i;
            }

            return -1;
        }
    }

}
