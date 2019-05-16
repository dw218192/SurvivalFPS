using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SurvivalFPS.Core.Weapon;

namespace SurvivalFPS.Core.Inventory
{
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
        [SerializeField] private uint m_StackSize; //TODO

        public uint stackSize { get { return m_StackSize; } }
        public string itemName { get { return m_ItemName; } }
        public GameObject physicalRepresentation { get { return m_PhysicalRepresentation; } }
        public Sprite itemSprite { get { return m_ItemSprite; } }
        public SectionType sectionType { get { return m_SectionType; } }

        public abstract void Use(PlayerManager player);
    }

    public class EquippableItemTemplate : InventoryItemTemplate
    {
        public override void Use(PlayerManager player)
        {
            throw new NotImplementedException();
        }
    }

    public class PowerUpItemTemplate : InventoryItemTemplate
    {
        public override void Use(PlayerManager player)
        {
            throw new NotImplementedException();
        }
    }
}
