using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using SurvivalFPS.Core.Audio;
using SurvivalFPS.Core.FPS;
using SurvivalFPS.AI;

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

        [Serializable]
        private class PlayerAttributeSettings
        {
            public float MaxHealth;
        }

        [SerializeField] PlayerAttributeSettings m_PlayerAttributeSettings;

        //internal variables
        //runtime
        private float m_Health;

        //key to get player info from the game scene manager
        private int m_InformationKey;

        //other
        private Collider m_Collider = null;
        private FirstPersonController m_FPSController = null;

        //properties
        public int informationKey { get { return m_InformationKey; } }
        public Transform weaponSocket { get { return m_WeaponSocket; } }

        //ref to managers
        private GameSceneManager m_GameSceneManager = null;
        private AudioManager m_AudioManager = null;

        //injured sounds
        private AudioCollection m_PlayerSounds;
        private float m_NextPainSoundTime = 0.0f;
        private float m_PainSoundOffset = 0.35f;

        //delegates
        private event Action<int> OnHealthChanged;

        public void RegisterStaminaChangeEvent(Action<int> action)
        {
            if (!m_FPSController)
            {
                m_FPSController = GetComponent<FirstPersonController>();
                if (!m_FPSController) return;
            }

            m_FPSController.OnStaminaChanged += action;
        }

        public void RegisterHealthChangeEvent(Action<int> action)
        {
            OnHealthChanged += action;
        }

        //public properties
        public float maxHealth { get { return m_PlayerAttributeSettings.MaxHealth; } }
        public float maxStamina { get { return m_FPSController ? m_FPSController.maxStamina : 0.0f; } }

        // Use this for initialization
        void Start()
        {
            m_Collider = GetComponent<Collider>();
            m_FPSController = GetComponent<FirstPersonController>();
            m_GameSceneManager = GameSceneManager.Instance;
            m_AudioManager = AudioManager.Instance;

            if (m_GameSceneManager != null)
            {
                PlayerInfo info = new PlayerInfo();
                info.playerCamera = m_Camera;
                info.playerManager = this;
                info.playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
                info.playerMotionController = m_FPSController;
                info.playerWeaponController = GetComponent<PlayerWeaponController>();
                info.collider = m_Collider;
                info.meleeTrigger = m_MeleeTrigger;

                m_GameSceneManager.RegisterPlayer(m_Collider.GetInstanceID(), info);

                m_PlayerSounds = m_GameSceneManager.playerInjuredSounds;
            }

            m_Health = m_PlayerAttributeSettings.MaxHealth;
            m_InformationKey = m_Collider.GetInstanceID();
        }

        public void TakeDamage(float amountPerSec)
        {
            m_Health = Mathf.Max(m_Health - (amountPerSec * Time.deltaTime), 0.0f);

            if (OnHealthChanged != null)
            {
                OnHealthChanged(Mathf.RoundToInt(m_Health));
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
    }
}
