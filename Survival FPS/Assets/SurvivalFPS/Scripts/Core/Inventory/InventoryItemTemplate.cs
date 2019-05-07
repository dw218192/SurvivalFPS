using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalFPS.Core.Inventory
{
    /// <summary>
    /// The data class that defines an item, essentially acting as the template of a type of item.
    /// </summary>
    public abstract class InventoryItemTemplate : ScriptableObject
    {
        [SerializeField] private Sprite m_ItemSprite;
        [SerializeField] private GameObject m_PhysicalRepresentation;
        [SerializeField] private uint m_StackSize;

        public GameObject physicalRepresentation { get { return m_PhysicalRepresentation; } }
        public Sprite itemSprite { get { return m_ItemSprite; } }

        public abstract void Use();
    }
}
