using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS.Core.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ItemOptionMenu : MonoBehaviour
    {
        public RectTransform rectTransform { get; private set; }
        public abstract void SetupMenu(ItemInstance item);
        protected PlayerManager m_Player;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            rectTransform.pivot = new Vector2(0.0f, 1.0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            m_Player = FindObjectOfType<PlayerManager>();
        }
    }
}