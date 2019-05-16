using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using SurvivalFPS.Core.UI;
using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS.Core.FPS
{
    public class PlayerInventorySystem : MonoBehaviour
    {
        public interface ISection
        {
            SectionType sectionType { get; }
            int size { get; }
        }

        [Serializable]
        private class Section : ISection
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

        [SerializeField] private Section[] m_Sections;

        public ISection[] GetSectionInfo()
        {
            return m_Sections.ToArray();
        }

        private void Awake()
        {
            foreach(Section sectionSetting in m_Sections)
            {
                for (int i = 0; i < sectionSetting.size; i++)
                {
                    sectionSetting.content.Add(null);
                }
            }

            //sort the array based on enum value
            m_Sections.OrderBy(sectionSetting => (int)sectionSetting.sectionType);
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
                return;
            }

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
                //inform the UI menu
                InventoryUI.Instance.SetSlot(item, emptyIndex);
            }

            return emptyIndex;
        }
    }

}
