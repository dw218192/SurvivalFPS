using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using SurvivalFPS.Core.Inventory;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core.UI
{
    public class InventorySectionUI : MonoBehaviour
    {
        [SerializeField] private ItemSlot m_ItemSlotPrefab;
        [SerializeField] private Transform m_SlotsParent;
        [SerializeField] private Text m_SectionHeaderText;
        [SerializeField] private RectTransform m_SlotsSlidingArea;

        private List<ItemSlot> m_ItemSlots = new List<ItemSlot>();
        private bool m_Initiated = false;
        private GridLayoutGroup m_GridLayoutGroup;

        public SectionType type { get; private set; }

        public void Init(int numSlots, SectionType type)
        {
            if (m_Initiated) return;

            for(int i=0; i<numSlots; i++)
            {
                ItemSlot slot = Instantiate(m_ItemSlotPrefab, m_SlotsParent);
                m_ItemSlots.Add(slot);
            }

            this.type = type;
            m_SectionHeaderText.text = type.ToString();

            //grid pattern init
            m_GridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
            m_SlotsSlidingArea.anchorMin = new Vector2(0.0f, 1.0f);
            m_SlotsSlidingArea.anchorMax = new Vector2(1.0f, 1.0f);
            m_SlotsSlidingArea.SetLeft(50.0f);
            m_SlotsSlidingArea.SetRight(50.0f);

            m_GridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            int numCols = Mathf.FloorToInt( m_SlotsSlidingArea.rect.width / (m_GridLayoutGroup.cellSize.x + m_GridLayoutGroup.spacing.x) );
            int numRows = Mathf.CeilToInt( (float)m_ItemSlots.Count / m_GridLayoutGroup.constraintCount);
            float slidingAreaHeight = numRows * m_GridLayoutGroup.cellSize.y +
                (numRows - 1) * m_GridLayoutGroup.spacing.y +
                m_GridLayoutGroup.padding.top +
                m_GridLayoutGroup.padding.bottom + 30.0f;
            Vector2 size = m_SlotsSlidingArea.sizeDelta;
            size.y = slidingAreaHeight;
            m_SlotsSlidingArea.sizeDelta = size;

            m_Initiated = true;
        }

        public void SetSlot(ItemInstance item, int index)
        {
            m_ItemSlots[index].SetItem(item);
        }
    }
}
