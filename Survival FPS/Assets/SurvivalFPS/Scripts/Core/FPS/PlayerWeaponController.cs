using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.Weapon;
using System;

namespace SurvivalFPS.Core.FPS
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] private List<WeaponConfig> m_Weapons;
        [SerializeField] private bool m_AutoReload;

        [Header("debug")]
        [SerializeField] private Animator m_HandAnimator;
        [SerializeField] private Animator m_ArmAnimator;
        [SerializeField] private int m_CurrentWeaponAmmo;

        //internal variables
        private WeaponConfig m_CurrentWeapon;
        private FirstPersonController m_PlayerController;
        private float m_EquipTimer;

        private event Action<WeaponConfig> OnWeaponChanged;
        public void RegisterWeaponChangeEvent(Action<WeaponConfig> action)
        {
            OnWeaponChanged += action;
        }

        private void Start()
        {
            m_PlayerController = gameObject.GetComponent<FirstPersonController>();

            m_PlayerController.playerAnimatorManager.AddAnimator(m_HandAnimator);
            m_PlayerController.playerAnimatorManager.AddAnimator(m_ArmAnimator);

            foreach (WeaponConfig weapon in m_Weapons)
            {
                weapon.Initialize(m_PlayerController);
                weapon.isActive = false;
            }

            SetCurrentWeapon(0);
        }

        private void Update()
        {
            if(m_EquipTimer > 0.0f)
            {
                m_EquipTimer -= Time.deltaTime;
            }

            //if we have a weapon
            if (m_CurrentWeapon)
            {
                m_CurrentWeaponAmmo = m_CurrentWeapon.currentAmmo;

                //wait for the weapon to be fully equipped
                if(m_EquipTimer <= 0.0f)
                {
                    bool canFire = !m_PlayerController.running && !m_CurrentWeapon.isReloading;

                    if (canFire && Input.GetButton("Fire1"))
                    {
                        m_CurrentWeapon.TryFire();
                        return;
                    }

                    if (Input.GetButtonDown("Reload"))
                    {
                        m_CurrentWeapon.Reload();
                        return;
                    }

                    if (m_AutoReload && m_CurrentWeapon.currentAmmo <= 0)
                    {
                        m_CurrentWeapon.Reload();
                        return;
                    }
                }

                if (!m_CurrentWeapon.isFiring)
                {
                    for (int i = 0; i < m_Weapons.Count; i++)
                    {
                        if (Input.GetKeyDown((KeyCode)(i + 49)))
                        {
                            SetCurrentWeapon(i);
                            return;
                        }
                    }
                }
            }
        }

        private void SetCurrentWeapon(int index)
        {
            if(m_CurrentWeapon)
            {
                //disable the mesh and the animator of the previous weapon
                m_CurrentWeapon.isActive = false;
            }

            m_CurrentWeapon = m_Weapons[index];

            //enable the weapon mesh and the weapon animator
            m_CurrentWeapon.isActive = true;

            //change the runtime controller of the hand and the arm
            m_ArmAnimator.runtimeAnimatorController = m_CurrentWeapon.animatorController;
            m_HandAnimator.runtimeAnimatorController = m_CurrentWeapon.animatorController;

            //put the weapon in the correct position
            PutCurrentWeaponInHand();

            //inform listeners
            OnWeaponChanged(m_CurrentWeapon);

            //set the timer
            m_EquipTimer = m_CurrentWeapon.equipTime;
        }

        public void PutCurrentWeaponInHand()
        {
            m_HandAnimator.transform.localPosition = m_CurrentWeapon.gripTransform.position;
            m_HandAnimator.transform.localRotation = m_CurrentWeapon.gripTransform.rotation;

            m_ArmAnimator.transform.localPosition = m_CurrentWeapon.gripTransform.position;
            m_ArmAnimator.transform.localRotation = m_CurrentWeapon.gripTransform.rotation;

            m_CurrentWeapon.gunGameObject.transform.localPosition = m_CurrentWeapon.gripTransform.position;
            m_CurrentWeapon.gunGameObject.transform.localRotation = m_CurrentWeapon.gripTransform.rotation;
        }
    }
}
