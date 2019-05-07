using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.PlayerInteraction
{
    /// <summary>
    /// The data class which defines how an interactive item should be interacted
    /// e.g. input time/ input mechanism/ input limits/ HUD prompt descriptions/ interaction
    /// </summary>
    [CreateAssetMenu(menuName = "SurvivalFPS/Interactive Item/Interactive Item Config")]
    public class InteractiveItemConfig : ScriptableObject
    {
        [Header("Input Settings")]
        [SerializeField] bool m_UseDefaultPrompt = true;
        [SerializeField] protected string m_PrtomptText = "";
        [SerializeField] protected float m_InputTime = 1;
        [SerializeField] protected int m_MaxInteractionNum = 1;

        //config data property
        public bool useDefaultPrompt { get { return m_UseDefaultPrompt; } }
        public string promptText { get { return m_PrtomptText; } }
        public float inputTime { get { return m_InputTime; } }
        public int interactionLimit { get { return m_MaxInteractionNum; } }
    }
}