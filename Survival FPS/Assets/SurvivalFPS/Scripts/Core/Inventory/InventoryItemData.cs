using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.UI;
using SurvivalFPS.Messaging;

namespace SurvivalFPS.Core.Inventory
{
    /// <summary>
    /// General payload format for inventory-related events
    /// </summary>
    public class InventoryEventData : MessengerEventData
    {
        public SectionType sectionType;
        public int indexInSection;
        public ItemInstance itemInstance;

        private InventoryEventData() { }
        public InventoryEventData(SectionType sectionType, int indexInSection, ItemInstance itemInstance)
        {
            this.sectionType = sectionType;
            this.indexInSection = indexInSection;
            this.itemInstance = itemInstance;
        }
    }

    //defines what general category an item belongs to
    //and what section in the inventory menu this item belongs to
    public enum SectionType
    {
        Item,
        Gear,
        Weapon
    }

    /// <summary>
    /// The data class that defines an item, essentially acting as the template of a type of item.
    /// </summary>
    public abstract class InventoryItemTemplate : ScriptableObject
    {
        [SerializeField] private SectionType m_SectionType;
        [SerializeField] private string m_ItemName;
        [SerializeField] private Sprite m_ItemSprite;
        [SerializeField] private GameObject m_PhysicalRepresentation;
        [SerializeField] private ItemOptionMenu m_ItemOptionMenuPrefab;
        [SerializeField] private uint m_StackSize; //TODO
        [SerializeField] private uint m_ConsumptionLimit;
        [SerializeField] private float m_StartingQuality;

        public ItemOptionMenu optionMenuPrefab { get { return m_ItemOptionMenuPrefab; } }
        public uint stackSize { get { return m_StackSize; } }
        public uint consumptionLimit { get { return m_ConsumptionLimit; } }
        public float startingQuality { get { return m_StartingQuality; }}
        public string itemName { get { return m_ItemName; } }
        public GameObject physicalRepresentation { get { return m_PhysicalRepresentation; } }
        public Sprite itemSprite { get { return m_ItemSprite; } }
        public SectionType sectionType { get { return m_SectionType; } }

        public virtual void Use(PlayerManager player, ItemInstance itemInstance)
        {
            itemInstance.itemStatus.RemainingUseCnt = (uint)Mathf.Max(0, (int)itemInstance.itemStatus.RemainingUseCnt - 1);

            OnItemBeingUsed(player, itemInstance);

            if(itemInstance.itemStatus.RemainingUseCnt == 0)
            {
                player.inventorySystem.RemoveItem(m_SectionType, itemInstance.inventoryIndex);
            }

            OnItemUsed(player, itemInstance);
        }

        protected abstract void OnItemBeingUsed(PlayerManager player, ItemInstance itemInstance);
        protected abstract void OnItemUsed(PlayerManager player, ItemInstance itemInstance);
    }

    public class EquippableItemTemplate : InventoryItemTemplate
    {
        protected override void OnItemBeingUsed(PlayerManager player, ItemInstance itemInstance)
        {
            throw new NotImplementedException();
        }

        protected override void OnItemUsed(PlayerManager player, ItemInstance itemInstance)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsumableItemTemplate : InventoryItemTemplate
    {
        protected override void OnItemBeingUsed(PlayerManager player, ItemInstance itemInstance)
        {
            throw new NotImplementedException();
        }

        protected override void OnItemUsed(PlayerManager player, ItemInstance itemInstance)
        {
            throw new NotImplementedException();
        }
    }
}
