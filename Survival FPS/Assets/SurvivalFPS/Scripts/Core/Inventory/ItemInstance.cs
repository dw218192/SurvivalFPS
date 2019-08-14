using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

using UnityEngine;

using System;

namespace SurvivalFPS.Core.Inventory
{
    [Serializable]
    public struct ItemStatus
    {
        public uint remainingUseCnt { get; set; }
        public float quality { get; set; }
    }

    /// <summary>
    /// The runtime instance of an inventory item
    /// </summary>
    [Serializable]
    public class ItemInstance : ISerializable
    {
        public InventoryItemTemplate itemTemplate;
        public ItemStatus itemStatus;

        //the runtime section index of this item in the inventory
        public int inventoryIndex { get; set; }

        private ItemInstance() { }

        public ItemInstance(InventoryItemTemplate itemTemplate)
        {
            this.itemTemplate = itemTemplate;
            itemStatus = new ItemStatus();
            itemStatus.remainingUseCnt = itemTemplate.consumptionLimit;
            itemStatus.quality = itemTemplate.startingQuality;

            inventoryIndex = -1;
        }

        protected ItemInstance(SerializationInfo info, StreamingContext context)
        {
            string address = info.GetString("itemTemplateAssetAddress");
            if(!GameAssetManager.GetAsset(address, out itemTemplate))
            {
                Debug.LogWarning("asset address " + address + " cannot be loaded");
            }
            
            itemStatus = (ItemStatus)info.GetValue("itemStatus", typeof(ItemStatus));
            inventoryIndex = info.GetInt32("inventoryIndex");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("itemTemplateAssetAddress", itemTemplate.itemName);
            info.AddValue("itemStatus", itemStatus, typeof(ItemStatus));
            info.AddValue("inventoryIndex", inventoryIndex);
        }
    }
}