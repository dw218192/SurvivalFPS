using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SurvivalFPS.AI;
using SurvivalFPS.Core.Weapon;

namespace SurvivalFPS.Core.FPS
{
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerStealthSystem : MonoBehaviour
    {
        [Serializable]
        private class VisualAggravationSettings
        {
            public bool isActive;
            public AggravatorData data;
        }

        [Serializable]
        private class AudioAggravationSettings
        {
            public bool isActive;
            public float baseAudibleRange;
            public AggravatorData data;
        }

        [SerializeField] private VisualAggravationSettings m_VisualAggravationSettings = new VisualAggravationSettings();
        [SerializeField] private AudioAggravationSettings m_AudioAggravationSettings = new AudioAggravationSettings();

        private FirstPersonController m_FirstPersonController;
        private PlayerWeaponController m_WeaponController;

        private ZombieAudioAggravator m_AudioAggravator;
        private ZombieVisualAggravator m_VisualAggravator;

        private CapsuleCollider m_VisualTrigger;
        private bool m_WasCrouching = false;

        private void Awake()
        {
            m_FirstPersonController = GetComponent<FirstPersonController>();
            m_WeaponController = GetComponent<PlayerWeaponController>();

            m_FirstPersonController.headBob.RegisterEvent(1.0f, TriggerFootStepAggravation, CurveControlledBob.HeadBobCallBackType.Vertical);

            if(m_WeaponController)
            {
                m_WeaponController.weaponFired += TriggerWeaponAggravation;
            }
        }

        private void Start()
        {
            if (m_VisualAggravationSettings.isActive && m_VisualAggravationSettings.data)
            {
                GameObject aggravatorHolder = new GameObject("VisualAggravator");
                aggravatorHolder.transform.parent = gameObject.transform;
                aggravatorHolder.transform.localPosition = Vector3.zero;
                aggravatorHolder.layer = GameApplication.LayerData.visualAggravatorLayer;

                CapsuleCollider playerCollider = GetComponent<CapsuleCollider>();

                m_VisualTrigger = aggravatorHolder.AddComponent<CapsuleCollider>();
                m_VisualTrigger.isTrigger = true;
                m_VisualTrigger.height = playerCollider.height;
                m_VisualTrigger.radius = playerCollider.radius;

                m_VisualAggravator = aggravatorHolder.AddComponent<ZombieVisualAggravator>();
                m_VisualAggravator.canOverwriteTarget = true;
                m_VisualAggravator.data = m_VisualAggravationSettings.data;
            }

            if (m_AudioAggravationSettings.isActive && m_AudioAggravationSettings.data)
            {
                GameObject aggravatorHolder = new GameObject("AudioAggravator");
                aggravatorHolder.transform.parent = gameObject.transform;
                aggravatorHolder.transform.localPosition = Vector3.zero;
                aggravatorHolder.layer = GameApplication.LayerData.audioAggravatorLayer;

                SphereCollider sphereCollider = aggravatorHolder.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = m_AudioAggravationSettings.baseAudibleRange;

                m_AudioAggravator = aggravatorHolder.AddComponent<ZombieAudioAggravator>();
                m_AudioAggravator.data = m_AudioAggravationSettings.data;
            }
        }

        private void Update()
        {
            if (!m_WasCrouching && m_FirstPersonController.crouching)
            {
                m_VisualTrigger.height /= 2.0f;
            }

            else if (m_WasCrouching && !m_FirstPersonController.crouching)
            {
                m_VisualTrigger.height *= 2.0f;
            }

            m_WasCrouching = m_FirstPersonController.crouching;
        }

        private void TriggerFootStepAggravation()
        {
            if (m_FirstPersonController.crouching)
            {
                return;
            }

            if (m_FirstPersonController.running)
            {
                m_AudioAggravator.EmitSound(0.5f, m_AudioAggravationSettings.baseAudibleRange * 4.0f, false);
                return;
            }
        }

        //TODO implement weapon fire event
        private void TriggerWeaponAggravation(WeaponConfig weaponInUse)
        {
            m_AudioAggravator.EmitSound(0.5f, m_AudioAggravationSettings.baseAudibleRange * 15.0f, false);
        }
    }
}