using System;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SurvivalFPS.Core.Inventory;
using SurvivalFPS.Core.FPS;
using UnityEngine.EventSystems;

namespace SurvivalFPS.Core.UI
{
    public class InventoryUI : GameMenu<InventoryUI>, IPointerClickHandler
    {
        [SerializeField] private Transform m_SectionParent;
        [SerializeField] private ItemOptionMenu m_ItemOptionMenuPrefab;
        [SerializeField] private InventorySectionUI m_SectionPrefab;

        private PlayerInventorySystem m_PlayerInventory;
        private InventorySectionUI[] m_Sections;
        private int m_CurSectionIndex = -1;

        //item option menu related
        private ItemOptionMenu m_ItemOptionMenu;
        private RectTransform m_ItemOptionMenuRect;

        public static event Action inventoryMenuOpened;
        public static event Action inventoryMenuClosed;

        public override void Init()
        {
            base.Init();

            m_PlayerInventory = FindObjectOfType<PlayerInventorySystem>();

            if(!m_PlayerInventory)
            {
                Debug.LogWarning("InventoryUI-Init: cannot find player inventory");
                return;
            }

            //make the UI script create sections based on player inventory system settings
            PlayerInventorySystem.ISection[] sections = m_PlayerInventory.GetSectionInfo();
            m_Sections = new InventorySectionUI[sections.Length];

            for (int i=0; i<m_Sections.Length; i++)
            {
                InventorySectionUI section = Instantiate(m_SectionPrefab, m_SectionParent);
                section.Init(m_PlayerInventory.GetSectionCapacity(sections[i].sectionType), sections[i].sectionType);
                m_Sections[i] = section;
                m_Sections[i].gameObject.SetActive(false);
            }

            //sort the array based on enum value
            m_Sections.OrderBy(section => (int)section.type);

            //configure the item option menu
            m_ItemOptionMenu = Instantiate(m_ItemOptionMenuPrefab, m_SectionParent);
            m_ItemOptionMenuRect = m_ItemOptionMenu.GetComponent<RectTransform>();
            m_ItemOptionMenuRect.pivot = new Vector2(0.0f, 1.0f);

            m_ItemOptionMenuRect.anchorMin = new Vector2(0.5f, 0.5f);
            m_ItemOptionMenuRect.anchorMax = new Vector2(0.5f, 0.5f);

            m_ItemOptionMenu.gameObject.SetActive(false);

            //display section
            OnNextSectionClicked();
        }

        //called by backend code
        public void SetSlot(ItemInstance item, int sectionIndex)
        {
            InventorySectionUI sectionUI = m_Sections[(int)item.itemTemplate.sectionType];
            sectionUI.SetSlot(item, sectionIndex);
        }

        //callbacks
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
            if (m_ItemOptionMenu.gameObject.activeSelf)
            {
                m_ItemOptionMenu.gameObject.SetActive(false);
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
            if (m_ItemOptionMenu.gameObject.activeSelf)
            {
                m_ItemOptionMenu.gameObject.SetActive(false);
            }

            m_CurSectionIndex = Mathf.Min(m_Sections.Length - 1, m_CurSectionIndex + 1);
            m_Sections[m_CurSectionIndex].gameObject.SetActive(true);
        }

        public void OnItemSlotRightClick(Vector2 clickPosition, Camera eventCamera)
        {
            m_ItemOptionMenu.gameObject.SetActive(true);

            //put the menu at the click point
            Vector2 point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_ItemOptionMenu.transform.parent, clickPosition, eventCamera, out point);
            m_ItemOptionMenuRect.anchoredPosition = point;

            //TODO: configure the onclick events
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                m_ItemOptionMenu.gameObject.SetActive(false);
            }
        }
    }
}