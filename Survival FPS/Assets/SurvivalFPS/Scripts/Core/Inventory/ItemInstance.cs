using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace SurvivalFPS.Core.Inventory
{
    [Serializable]
    public class ItemStatus
    {
        public ItemStatus() { }
        public uint RemainingUseCnt { get; set; }
        public float quality { get; set; }
    }

    /// <summary>
    /// The runtime instance of an inventory item
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        public InventoryItemTemplate itemTemplate;
        public ItemStatus itemStatus;

        //the runtime section index of this item in the inventory
        public int inventoryIndex { get; set; }

        private ItemInstance() { }

        public ItemInstance(InventoryItemTemplate itemTemplate)
        {
            this.itemTemplate = itemTemplate;
            this.itemStatus = new ItemStatus();
            itemStatus.RemainingUseCnt = itemTemplate.consumptionLimit;
            itemStatus.quality = itemTemplate.startingQuality;

            inventoryIndex = -1;
        }
    }
}