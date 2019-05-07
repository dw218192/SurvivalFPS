using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.Inventory;

using System;

namespace SurvivalFPS.Core.PlayerInteraction
{
    [Serializable]
    public class ItemProperty
    {
        public float quality;
    }

    [Serializable]
    public class ItemInstance
    {
        public InventoryItemTemplate itemTemplate;
        public ItemProperty itemProperty;
        public int inventoryIndex;

        public ItemInstance(InventoryItemTemplate itemTemplate, ItemProperty itemProperty)
        {
            this.itemTemplate = itemTemplate;
            this.itemProperty = itemProperty;
            inventoryIndex = -1;
        }
    }

    public class Pickable : InteractiveItem
    {
        [SerializeField] private InventoryItemTemplate m_ItemTemplate;
        [SerializeField] private int m_PickupLimit = 1;

        private ItemInstance m_ItemInstance;

        protected override void Initialize()
        {
            base.Initialize();
            m_ItemInstance = CreateItem();
            Instantiate(m_ItemTemplate.physicalRepresentation, transform, false);
        }

        //TODO: stack size problem
        protected override void OnBeginInteract(PlayerManager playerManager)
        {
            base.OnBeginInteract(playerManager);

            if (!playerManager.inventorySystem)
            {
                return;
            }

            m_ItemInstance.inventoryIndex = playerManager.inventorySystem.AddItem(m_ItemInstance);

            if(m_ItemInstance.inventoryIndex == -1)
            {
                return;
            }

            m_PickupLimit = Mathf.Max(0, m_PickupLimit - 1);

            if (m_PickupLimit == 0)
            {
                Destroy(gameObject);
            }

            //create another item
            m_ItemInstance = CreateItem();
            EndInteraction();
        }

        private ItemInstance CreateItem()
        {
            return new ItemInstance(m_ItemTemplate, new ItemProperty());
        }
    }
}