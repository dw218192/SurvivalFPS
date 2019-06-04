using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS.Core.UI
{
    public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image m_SlotBackgroundImage;
        [SerializeField] private Image m_ItemImage;
        [SerializeField] private Text m_ItemText;

        private Sprite m_NoItemSprite;

        private RectTransform m_RectTransform;
        private float m_BkgImgColorAlpha;
        private ItemInstance m_Item;

        private void Awake()
        {
            m_NoItemSprite = m_ItemImage.sprite;
            m_ItemText.text = "";

            m_BkgImgColorAlpha = m_SlotBackgroundImage.color.a;
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void SetItem(ItemInstance item)
        {
            m_Item = item;
            m_ItemImage.sprite = item.itemTemplate.itemSprite;
            m_ItemText.text = item.itemTemplate.itemName;
        }

        public void UnsetItem()
        {
            m_ItemImage.sprite = m_NoItemSprite;
            m_ItemText.text = "";
            m_Item = null;
        }

        public void UseItem(PlayerManager player)
        {
            m_Item.itemTemplate.Use(player, m_Item);
        }

        #region interface implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_SlotBackgroundImage)
            {
                Color color = m_SlotBackgroundImage.color;
                color.a = m_BkgImgColorAlpha * 2f;
                m_SlotBackgroundImage.color = color;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_SlotBackgroundImage)
            {
                Color color = m_SlotBackgroundImage.color;
                color.a = m_BkgImgColorAlpha;
                m_SlotBackgroundImage.color = color;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                InventoryUI.Instance.OnItemSlotLeftClick();
            }
                
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                InventoryUI.Instance.OnItemSlotRightClick(m_Item, eventData.pressPosition, eventData.pressEventCamera);
            }
        }
        #endregion
    }
}
