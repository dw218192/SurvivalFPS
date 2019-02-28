using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.PlayerInteraction
{
    public abstract class InteractiveItemConfig : ScriptableObject
    {
        [SerializeField] private float m_MaxInputInterval;
        [SerializeField] private float m_InputTime;
        [SerializeField] private string m_ItemName = "item";

        //config data property
        public string itemName { get { return m_ItemName; } }
        public float inputInterval { get { return m_MaxInputInterval; } }
        public float inputTime { get { return m_InputTime; } }

        //runtime property
        public bool isInteracting { get; protected set; }

        public abstract void Interact(PlayerManager player);
    }
}