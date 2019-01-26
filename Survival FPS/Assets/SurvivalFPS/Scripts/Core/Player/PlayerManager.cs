using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SurvivalFPS.Core.FPS;
using SurvivalFPS.AI;

namespace SurvivalFPS.Core
{
    [RequireComponent(typeof(AudioManager))]
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
        public class PlayerAttributeSettings
        {
            public float maxHealth;
        }

        [SerializeField] PlayerAttributeSettings m_PlayerAttributeSettings;

        //internal variables
        //runtime
        [SerializeField] private float m_Health;
        private int m_InformationKey;
        //other
        private Collider m_Collider = null;
        private FirstPersonController m_FPSController = null;
        private GameSceneManager m_GameSceneManager = null;

        //properties
        public int informationKey { get { return m_InformationKey; } }
        public Transform weaponSocket { get { return m_WeaponSocket; } }

        // Use this for initialization
        void Start()
        {
            m_Collider = GetComponent<Collider>();
            m_FPSController = GetComponent<FirstPersonController>();
            m_GameSceneManager = GameSceneManager.Instance;

            if (m_GameSceneManager != null)
            {
                PlayerInfo info = new PlayerInfo();
                info.playerCamera = m_Camera;
                info.playerManager = this;
                info.playerAudioManager = GetComponent<AudioManager>();
                info.playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
                info.playerMotionController = GetComponent<FirstPersonController>();
                info.playerWeaponController = GetComponent<PlayerWeaponController>();
                info.collider = m_Collider;
                info.meleeTrigger = m_MeleeTrigger;

                m_GameSceneManager.RegisterPlayer(m_Collider.GetInstanceID(), info);
            }

            m_Health = m_PlayerAttributeSettings.maxHealth;
            m_InformationKey = m_Collider.GetInstanceID();
        }

        public void TakeDamage(float amountPerSec)
        {
            m_Health = Mathf.Max(m_Health - (amountPerSec * Time.deltaTime), 0.0f);

            if (m_CameraBloodEffect != null)
            {
                m_CameraBloodEffect.minBloodAmount = (1.0f - m_Health / 100.0f);
                m_CameraBloodEffect.bloodAmount = Mathf.Min(m_CameraBloodEffect.minBloodAmount + 0.3f, 1.0f);
            }
        }
    }
}
