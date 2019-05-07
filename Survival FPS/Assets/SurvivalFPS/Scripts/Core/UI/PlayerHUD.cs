using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

using SurvivalFPS.Core.PlayerInteraction;
using SurvivalFPS.Core.FPS;

namespace SurvivalFPS.Core.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        //UI references
        [SerializeField] private Text m_HealthText;
        [SerializeField] private Text m_StaminaText;
        [SerializeField] private Text m_InteractiveItemText;
        [SerializeField] private Image m_InteractionProgressImg;

        //format
        [SerializeField] private string m_InteractiveItemStartingString = "Press E to interact with ";
        [SerializeField] private string m_HealthStartingString = "Health: ";
        [SerializeField] private string m_StaminaStartingString = "Stamina: ";

        // Start is called before the first frame update
        void Start()
        {
            PlayerManager playerManager = FindObjectOfType<PlayerManager>();

            if(playerManager)
            {
                playerManager.playerInteractionEvent += OnPlayerInteractionEvent;
                playerManager.interactionProgressChanged += OnInteractiveProgressChanged;

                playerManager.healthChanged += OnPlayerHealthChanged;
                playerManager.staminaChanged += OnPlayerStaminaChanged;

                m_HealthText.text = m_HealthStartingString + " " + playerManager.maxHealth.ToString();
                m_StaminaText.text = m_StaminaStartingString + " " + playerManager.maxStamina.ToString();
            }

            m_InteractiveItemText.text = "";
            m_InteractionProgressImg.type = Image.Type.Filled;
            m_InteractionProgressImg.fillAmount = 0.0f;
        }

        private void OnPlayerInteractionEvent(InteractiveItem item, InteractionEventType eventType)
        {
            if (!item) return;

            //if the player is looking at an item in a valid range
            if(eventType == InteractionEventType.OnPlayerLookAtInRange)
            {
                InteractiveItemConfig itemConfig = item.inputConfig;

                if (itemConfig == null)
                {
                    return;
                }

                if(itemConfig.useDefaultPrompt)
                {
                    m_InteractiveItemText.text = m_InteractiveItemStartingString + item.itemName;
                }
                else
                {
                    m_InteractiveItemText.text = itemConfig.promptText + item.itemName;
                }
                return;
            }

            m_InteractiveItemText.text = "";
            return;
        }

        private void OnInteractiveProgressChanged(float newValue)
        {
            //the player has completed the input process
            //or the player has failed the interaction
            if(newValue < 0.0f || Mathf.Approximately(newValue, 1.0f))
            {
                m_InteractionProgressImg.fillAmount = 0.0f;
                return;
            }

            m_InteractionProgressImg.fillAmount = Mathf.MoveTowards(m_InteractionProgressImg.fillAmount, newValue, 12.0f * Time.deltaTime);
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

