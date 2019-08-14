using System;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SurvivalFPS.Core.Inventory;
using SurvivalFPS.Messaging;

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SurvivalFPS.Core.UI
{
    public class InventoryUI : GameMenu<InventoryUI>, IPointerClickHandler
    {
        [SerializeField] private Transform m_SectionParent;
        [SerializeField] private InventorySectionUI m_SectionPrefab;

        private PlayerInventorySystem m_PlayerInventory;
        private InventorySectionUI[] m_Sections;
        private int m_CurSectionIndex = -1;

        //item option menu related
        //prefab --> instance
        private Dictionary<ItemOptionMenu, ItemOptionMenu> m_ItemOptionMenuInstances = new Dictionary<ItemOptionMenu, ItemOptionMenu>();
        [SerializeField] private ItemOptionMenu m_CurActiveItemMenu;

        public static event Action inventoryMenuOpened;
        public static event Action inventoryMenuClosed;

        public override void Init()
        {
            base.Init();

            //subscriptions
            Messenger.AddPersistentListener<InventoryEventData>(M_DataEventType.OnInventoryItemRemoved, OnItemRemoved);
            Messenger.AddPersistentListener<InventoryEventData>(M_DataEventType.OnInventoryItemAdded, OnItemAdded);
        }

        public override void SceneInit(Scene scene)
        {
            m_PlayerInventory = FindObjectOfType<PlayerInventorySystem>();

            if (!m_PlayerInventory)
            {
                Debug.LogWarning("InventoryUI-Init: cannot find player inventory");
                return;
            }

            //make the UI script create sections based on player inventory system settings
            PlayerInventorySystem.ISection[] sections = m_PlayerInventory.GetSectionInfo();
            m_Sections = new InventorySectionUI[sections.Length];

            for (int i = 0; i < m_Sections.Length; i++)
            {
                InventorySectionUI section = Instantiate(m_SectionPrefab, m_SectionParent);
                section.Init(m_PlayerInventory.GetSectionCapacity(sections[i].sectionType), sections[i].sectionType);
                m_Sections[i] = section;
                m_Sections[i].gameObject.SetActive(false);
            }

            //sort the array based on enum value
            m_Sections.OrderBy(section => (int)section.type);

            //display section
            OnNextSectionClicked();
        }

        //called by backend code
        public void SetSlot(ItemInstance item, int itemIndex)
        {
            InventorySectionUI sectionUI = m_Sections[(int)item.itemTemplate.sectionType];
            sectionUI.SetSlot(item, itemIndex);
        }

        public void UnsetSlot(SectionType sectionType, int itemIndex)
        {
            InventorySectionUI sectionUI = m_Sections[(int)sectionType];
            sectionUI.UnsetSlot(itemIndex);
        }

        //menu manager events
        public override void OnEnterMenu()
        {
            base.OnEnterMenu();

            inventoryMenuOpened();
        }
 
        public override void OnLeaveMenu()
        {
            base.OnLeaveMenu();

            inventoryMenuClosed();
        }

        //UI event functions
        public void OnPrevSectionClicked()
        {
            //turn the current section off if it exists
            if (m_CurSectionIndex >= 0 && m_CurSectionIndex < m_Sections.Length)
            {
                m_Sections[m_CurSectionIndex].gameObject.SetActive(false);
            }

            //if the item option menu is active, turn it off
            if (m_CurActiveItemMenu && m_CurActiveItemMenu.gameObject.activeSelf)
            {
                m_CurActiveItemMenu.gameObject.SetActive(false);
            }

            m_CurSectionIndex = Mathf.Max(0, m_CurSectionIndex - 1);
            m_Sections[m_CurSectionIndex].gameObject.SetActive(true);
        }

        public void OnNextSectionClicked()
        {
            //turn the current section off if it exists
            if (m_CurSectionIndex >= 0 && m_CurSectionIndex < m_Sections.Length)
            {
                m_Sections[m_CurSectionIndex].gameObject.SetActive(false);
            }

            //if the item option menu is active, turn it off
            if (m_CurActiveItemMenu && m_CurActiveItemMenu.gameObject.activeSelf)
            {
                m_CurActiveItemMenu.gameObject.SetActive(false);
            }

            m_CurSectionIndex = Mathf.Min(m_Sections.Length - 1, m_CurSectionIndex + 1);
            m_Sections[m_CurSectionIndex].gameObject.SetActive(true);
        }

        //UI broadcasting system messages
        public void OnItemSlotRightClick(ItemInstance item, Vector2 clickPosition, Camera eventCamera)
        {
            if(m_CurActiveItemMenu && m_CurActiveItemMenu.gameObject.activeSelf)
            {
                m_CurActiveItemMenu.gameObject.SetActive(false);
            }

            if(item == null)
            {
                return;
            }
            
            ItemOptionMenu prefab = item.itemTemplate.optionMenuPrefab;

            if(prefab == null)
            {
                return;
            }

            ItemOptionMenu instance;

            if(!m_ItemOptionMenuInstances.TryGetValue(prefab, out instance))
            {
                instance = Instantiate(prefab, m_SectionParent);
                m_ItemOptionMenuInstances.Add(prefab, instance);
            }

            instance.gameObject.SetActive(true);
            m_CurActiveItemMenu = instance;

            //put the menu at the click point
            Vector2 point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)instance.transform.parent, clickPosition, eventCamera, out point);
            instance.rectTransform.anchoredPosition = point;

            //configure the onclick events
            instance.SetupMenu(item);
        }

        public void OnItemSlotLeftClick()
        {
            if (m_CurActiveItemMenu && m_CurActiveItemMenu.gameObject.activeSelf)
            {
                m_CurActiveItemMenu.gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                m_CurActiveItemMenu.gameObject.SetActive(false);
            }
        }

        //messenger messages
        private void OnItemRemoved(InventoryEventData data)
        {
            UnsetSlot(data.sectionType, data.indexInSection);

            //disable the item otiton menu, if any
            if (m_CurActiveItemMenu && m_CurActiveItemMenu.gameObject.activeSelf)
            {
                m_CurActiveItemMenu.gameObject.SetActive(false);
            }
        }

        private void OnItemAdded(InventoryEventData data)
        {
            SetSlot(data.itemInstance, data.indexInSection);
        }
    }
}