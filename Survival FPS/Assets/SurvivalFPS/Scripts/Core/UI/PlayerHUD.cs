using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalFPS.Core.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        //UI references
        [SerializeField] private Text m_HealthText;
        [SerializeField] private Text m_StaminaText;
        [SerializeField] private Text m_InteractiveItemText;
        //format
        [SerializeField] private string m_HealthStartingString;
        [SerializeField] private string m_StaminaStartingString;

        // Start is called before the first frame update
        void Start()
        {
            PlayerManager playerManager = FindObjectOfType<PlayerManager>();

            if(playerManager)
            {
                playerManager.RegisterHealthChangeEvent(OnPlayerHealthChanged);
                playerManager.RegisterStaminaChangeEvent(OnPlayerStaminaChanged);

                m_HealthText.text = m_HealthStartingString + " " + playerManager.maxHealth.ToString();
                m_StaminaText.text = m_StaminaStartingString + " " + playerManager.maxStamina.ToString();
            }
        }

        private void OnPlayerHealthChanged(int newValue)
        {
            m_HealthText.text = m_HealthStartingString + " " + newValue.ToString();
        }

        private void OnPlayerStaminaChanged(int newValue)
        {
            m_StaminaText.text = m_StaminaStartingString + " " + newValue.ToString();
        }
    }
}

