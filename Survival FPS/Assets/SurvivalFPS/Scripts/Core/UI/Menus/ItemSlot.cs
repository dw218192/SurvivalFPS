using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.PlayerInteraction;

namespace SurvivalFPS.Core.UI
{
    public class ItemSlot : MonoBehaviour
    {
        private ItemInstance m_Item;

        public void SetItem()
        {
            
        }

        public void UnsetItem()
        {
            
        }

        public void UseItem()
        {
            m_Item.itemTemplate.Use();
        }
    }
}
