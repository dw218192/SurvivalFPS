using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace SurvivalFPS.Core.Inventory
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

        //the runtime section index of this item in the inventory
        public int inventoryIndex { get; set; }

        private ItemInstance() { }

        public ItemInstance(InventoryItemTemplate itemTemplate, ItemProperty itemProperty)
        {
            this.itemTemplate = itemTemplate;
            this.itemProperty = itemProperty;
            inventoryIndex = -1;
        }
    }
}