using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using UnityEngine;

using SurvivalFPS.Core.Audio;
using SurvivalFPS.Core.FPS;
using SurvivalFPS.AI;

using SurvivalFPS.Core.UI;
using SurvivalFPS.Core.PlayerInteraction;
using SurvivalFPS.Core.Inventory;
using SurvivalFPS.Messaging;

namespace SurvivalFPS.Core
{
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(PlayerAnimatorManager))]
    [DisallowMultipleComponent]
    public class PlayerManager : MonoBehaviour, IAIDamageable
    {
        // Inspector Assigned
        [SerializeField] private Transform m_WeaponSocket;
        [SerializeField] private CapsuleCollider m_MeleeTrigger = null;
        [SerializeField] private CameraBloodEffect m_CameraBloodEffect = null;
        [SerializeField] private Camera m_Camera = null;

        //an audio trigger for zombies so that they can go to investigate the sound
        [SerializeField] private ZombieAudioAggravator m_AudioAggravator; 

        [Serializable]
        private class PlayerAttributeSettings
        {
            public float MaxHealth;
        }

        [SerializeField] private PlayerAttributeSettings m_PlayerAttributeSettings = new PlayerAttributeSettings();

        //private internal variables
        private float m_Health;

        //key to get player info from the game scene manager
        private int m_InformationKey;

        //other
        private Collider m_Collider = null;

        //controllers
        private FirstPersonController m_FPSController = null;
        private PlayerWeaponController m_WeaponController = null;
        private PlayerInteractionController m_InteractionController = null;
        private PlayerInventorySystem m_PlayerInventorySystem = null;
        private IPlayerController[] m_PlayerControllers;

        //properties
        public int informationKey { get { return m_InformationKey; } }
        public Camera playerCamera { get { return m_Camera; } }
        public Transform weaponSocket { get { return m_WeaponSocket; } }
        public PlayerWeaponController weaponController { get { return m_WeaponController; } }
        public PlayerInventorySystem inventorySystem { get { return m_PlayerInventorySystem; } }

        //ref to managers
        private GameSceneManager m_GameSceneManager = null;
        private AudioManager m_AudioManager = null;

        //injured sounds
        private AudioCollection m_PlayerSounds;
        private float m_NextPainSoundTime = 0.0f;
        private float m_PainSoundOffset = 0.35f;

        //delegates
        public event Action<int> staminaChanged
        {
            add
            {
                if (m_FPSController)
                {
                    m_FPSController.staminaChanged += value;
                }
            }

            remove
            {
                if (m_FPSController)
                {
                    m_FPSController.staminaChanged -= value;
                }
            }
        }

        public event Action<int> healthChanged;

        /// <summary>
        /// subscribe to this event to be informed at different states of a player interaction
        /// </summary>
        public event Action<InteractiveItem, InteractionEventType> playerInteractionEvent
        {
            add
            {
                if (m_InteractionController)
                {
                    m_InteractionController.interactionEvent += value;
                }
            }

            remove
            {
                if (m_InteractionController)
                {
                    m_InteractionController.interactionEvent -= value;
                }
            }
        }

        /// <summary>
        /// subscribe to this event to be informed of the progress of the item interaction,
        /// if there is any. -1 will be passed once right after the interaction fails.
        /// </summary>
        public event Action<float> interactionProgressChanged
        {
            add
            {
                if (m_InteractionController)
                {
                    m_InteractionController.interactiveProgressChanged += value;
                }
            }

            remove
            {
                if (m_InteractionController)
                {
                    m_InteractionController.interactiveProgressChanged -= value;
                }
            }
        }

        //public properties
        public float maxHealth { get { return m_PlayerAttributeSettings.MaxHealth; } }
        public float maxStamina { get { return m_FPSController ? m_FPSController.maxStamina : 0.0f; } }

        private void Awake()
        {
            //component setup and communication with the game scene manager
            m_Collider = GetComponent<Collider>();
            m_FPSController = GetComponent<FirstPersonController>();
            m_WeaponController = GetComponent<PlayerWeaponController>();
            m_InteractionController = GetComponent<PlayerInteractionController>();
            m_PlayerInventorySystem = GetComponent<PlayerInventorySystem>();
            m_GameSceneManager = GameSceneManager.Instance;
            m_AudioManager = AudioManager.Instance;

            if (m_GameSceneManager != null)
            {
                PlayerInfo info = new PlayerInfo();
                info.playerCamera = m_Camera;
                info.playerManager = this;
                info.playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
                info.playerMotionController = m_FPSController;
                info.playerWeaponController = m_WeaponController;
                info.playerInventorySystem = m_PlayerInventorySystem;
                info.collider = m_Collider;
                info.meleeTrigger = m_MeleeTrigger;

                m_InformationKey = m_Collider.GetInstanceID();
                m_GameSceneManager.RegisterPlayer(m_InformationKey, info);

                m_PlayerSounds = m_GameSceneManager.playerInjuredSounds;
            }
        }

        // Use this for initialization
        private void Start()
        {
            m_Health = m_PlayerAttributeSettings.MaxHealth;
            m_PlayerControllers = Array.ConvertAll(GetComponents(typeof(IPlayerController)), (Component compoent) => (IPlayerController)compoent);

            //subscribe to game pause event
            Messenger.AddListener(M_EventType.OnGamePaused, OnGamePaused);
            Messenger.AddListener(M_EventType.OnGameResumed, OnGameResumed);

            InventoryUI.inventoryMenuOpened += OnInventoryOpened;
            InventoryUI.inventoryMenuClosed += OnInventoryClosed;
        }

        public void TakeDamage(float amountPerSec)
        {
            m_Health = Mathf.Max(m_Health - (amountPerSec * Time.deltaTime), 0.0f);

            if (healthChanged != null)
            {
                healthChanged(Mathf.RoundToInt(m_Health));
            }

            //camera blood
            if (m_CameraBloodEffect != null)
            {
                m_CameraBloodEffect.minBloodAmount = (1.0f - m_Health / 100.0f);
                m_CameraBloodEffect.bloodAmount = Mathf.Min(m_CameraBloodEffect.minBloodAmount + 0.3f, 1.0f);
            }

            //play damage sounds
            if(m_PlayerSounds && m_NextPainSoundTime < Time.time)
            {
                AudioClip clip = m_PlayerSounds[0];
                m_NextPainSoundTime = Time.time + clip.length;

                m_AudioManager.PlayOneShotSoundDelayed(m_PlayerSounds.audioGroup,
                                                       clip,
                                                       transform.position,
                                                       m_PlayerSounds.volume,
                                                       m_PlayerSounds.spatialBlend,
                                                       m_PainSoundOffset,
                                                       m_PlayerSounds.priority);
            }
        }

        public void OnGamePaused()
        {
            foreach (IPlayerController playerController in m_PlayerControllers)
            {
                playerController.StopControl();
            }
        }

        public void OnGameResumed()
        {
            foreach (IPlayerController playerController in m_PlayerControllers)
            {
                playerController.ResumeControl();
            }
        }

        public void OnInventoryOpened()
        {
            foreach (IPlayerController playerController in m_PlayerControllers)
            {
                playerController.StopControl();
            }
        }

        public void OnInventoryClosed()
        {
            foreach (IPlayerController playerController in m_PlayerControllers)
            {
                playerController.ResumeControl();
            }
        }
    }
}
