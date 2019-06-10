using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using UnityEngine;

using SurvivalFPS.Core.UI;
using SurvivalFPS.Messaging;
using SurvivalFPS.Saving;

namespace SurvivalFPS.Core.Inventory
{
    public class PlayerInventorySystem : MonoBehaviour
    {
        public interface ISection
        {
            SectionType sectionType { get; }
            int size { get; }
        }

        [Serializable]
        private sealed class Section : ISection
        {
            [SerializeField] private SectionType m_SectionType = SectionType.Item;
            [SerializeField] private int m_Size = 10;
            public SectionType sectionType { get { return m_SectionType; } }
            public int size { get { return m_Size; } }

            public List<ItemInstance> content = new List<ItemInstance>();

            public int FindEmptySpot()
            {
                for(int i=0; i<size; i++)
                {
                    if (content[i] == null) return i;
                }

                return -1;
            }
        }

        [Serializable]
        private sealed class InventorySaveData : ISerializable
        {
            public Section[] sections;

            public InventorySaveData() { }

            private InventorySaveData(SerializationInfo info, StreamingContext context)
            {
                sections = (Section[]) info.GetValue("sections", typeof(Section[]));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("sections", sections, typeof(Section[]));
            }
        }

        [SerializeField] private bool m_UseFileData;
        [SerializeField] private Section[] m_Sections;

        public ISection[] GetSectionInfo()
        {
            return m_Sections.ToArray();
        }

        private void Awake()
        {
            bool successfulLoad = false;

            if(m_UseFileData)
            {
                successfulLoad = LoadInventory();
            }

            if(!successfulLoad)
            {
                foreach (Section sectionSetting in m_Sections)
                {
                    for (int i = 0; i < sectionSetting.size; i++)
                    {
                        sectionSetting.content.Add(null);
                    }
                }
            }

            //sort the array based on enum value
            m_Sections.OrderBy(sectionSetting => (int)sectionSetting.sectionType);

            Messenger.AddListener(M_EventType.OnGameSaving, SaveInventory);
        }

        /// <summary>
        /// Gets the capacity of a particular inventory section
        /// </summary>
        /// <param name="sectionType"></param>
        /// <returns>returns -1 if the section does not exist</returns>
        public int GetSectionCapacity(SectionType sectionType)
        {
            Section section = m_Sections[(int)sectionType];
            return section == null ? -1 : section.size;
        }

        /// <summary>
        /// Removes the item entirely.
        /// </summary>
        public void RemoveItem(SectionType sectionType, int index)
        {
            Section section = m_Sections[(int)sectionType];

            if (section == null)
            {
                Debug.LogWarning("PlayerInventorySystem.RemoveItem(): attempt to remove an item in an unknown section");
                return;
            }

            if (index < 0 || index >= section.content.Count - 1)
            {
                Debug.LogWarning("PlayerInventorySystem.RemoveItem(): attempt to remove an item with an invalid index");
                return;
            }

            //broadcast the event
            InventoryEventData eventData = new InventoryEventData(sectionType, index, section.content[index]);
            Messenger.Broadcast(M_DataEventType.OnInventoryItemRemoved, eventData);

            section.content[index] = null;
        }

        /// <summary>
        /// Adds the item to the player inventory.
        /// </summary>
        /// <returns>The item inventory id if the item can be added. -1 if the item cannot be added </returns>
        public int AddItem(ItemInstance item)
        {
            Section section = m_Sections[(int)item.itemTemplate.sectionType];

            if (section == null)
            {
                return -1;
            }

            int emptyIndex = section.FindEmptySpot();

            if (emptyIndex != -1) 
            {
                section.content[emptyIndex] = item;

                //broadcast the event
                InventoryEventData eventData = new InventoryEventData(item.itemTemplate.sectionType, emptyIndex, item);
                Messenger.Broadcast(M_DataEventType.OnInventoryItemAdded, eventData);
            }

            return emptyIndex;
        }

        public void DeleteSaveFile()
        {
            BinarySaver.Delete(SaveFileNameConfig.inventorySaveFileName);
        }

        private void SaveInventory()
        {
            InventorySaveData inventorySaveData = new InventorySaveData();
            inventorySaveData.sections = m_Sections;

            BinarySaver.Save(inventorySaveData, SaveFileNameConfig.inventorySaveFileName);
        }

        private bool LoadInventory()
        {
            InventorySaveData inventorySaveData;
            if ( BinarySaver.Load(out inventorySaveData, SaveFileNameConfig.inventorySaveFileName) )
            {
                m_Sections = inventorySaveData.sections;

                for (int i=0; i<m_Sections.Length; i++)
                {
                    List<ItemInstance> items = m_Sections[i].content;

                    for (int j=0; j<items.Count; j++)
                    {
                        //broadcast the event
                        InventoryEventData eventData = new InventoryEventData(m_Sections[i].sectionType, j, items[j]);
                        Messenger.Broadcast(M_DataEventType.OnInventoryItemAdded, eventData);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
