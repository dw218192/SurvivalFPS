using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.Weapon;
using System;

namespace SurvivalFPS.Core.FPS
{
    /// <summary>
    /// a controller that should be attached to whoever can use a weapon
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] private List<WeaponConfig> m_Weapons;
        [SerializeField] private bool m_AutoReload;

        //TODO: consider removing manually coupling arm & hand animators
        [Header("debug")]
        [SerializeField] private Animator m_HandAnimator;
        [SerializeField] private Animator m_ArmAnimator;
        private AnimatorOverrideController m_HandAndArmOverride; //override for hand/arm

        //internal variables
        private WeaponConfig m_CurrentWeapon;
        [SerializeField] private int m_CurrentWeaponAmmo;

        private PlayerManager m_PlayerManager;
        private FirstPersonController m_FPSController;
        private PlayerAnimatorManager m_PlayerAnimManager;

        private float m_EquipTimer;

        //public properties
        public bool fireLeadingEdge { get; private set; }
        public WeaponConfig currentWeapon { get { return m_CurrentWeapon; } }

        //delegates
        private event Action<WeaponConfig> OnWeaponChanged;
        public void RegisterWeaponChangeEvent(Action<WeaponConfig> action)
        {
            OnWeaponChanged += action;
        }

        //warning: this script needs to be executed after PlayerManager!
        private void Start()
        {
            m_PlayerManager = gameObject.GetComponent<PlayerManager>();
            int key = m_PlayerManager.informationKey;
            PlayerInfo info = GameSceneManager.Instance.GetPlayerInfo(key);

            m_PlayerAnimManager = info.playerAnimatorManager;
            m_FPSController = info.playerMotionController;

            m_PlayerAnimManager.AddAnimator(m_HandAnimator);
            m_PlayerAnimManager.AddAnimator(m_ArmAnimator);

            //initialize weapon configs and behaviors
            foreach (WeaponConfig weapon in m_Weapons)
            {
                weapon.Initialize(m_PlayerManager);
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
                    fireLeadingEdge = Input.GetButtonDown("Fire1");

                    bool canFire = !m_FPSController.running && !m_CurrentWeapon.isReloading;

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
            //do not simply assign, because weapon anim controller is different from FPS anim controller
            //in terms of state machine behaviours
            SetHandAndArmAnimationOverride();

            //put the weapon in the correct position
            PutCurrentWeaponInHand();

            //inform listeners
            OnWeaponChanged(m_CurrentWeapon);

            //set the timer
            m_EquipTimer = m_CurrentWeapon.equipTime;
        }

        private void PutCurrentWeaponInHand()
        {
            m_HandAnimator.transform.localPosition = m_CurrentWeapon.gripTransform.position;
            m_HandAnimator.transform.localRotation = m_CurrentWeapon.gripTransform.rotation;

            m_ArmAnimator.transform.localPosition = m_CurrentWeapon.gripTransform.position;
            m_ArmAnimator.transform.localRotation = m_CurrentWeapon.gripTransform.rotation;

            m_CurrentWeapon.gunGameObject.transform.localPosition = m_CurrentWeapon.gripTransform.position;
            m_CurrentWeapon.gunGameObject.transform.localRotation = m_CurrentWeapon.gripTransform.rotation;
        }

        private void SetHandAndArmAnimationOverride()
        {
            m_HandAndArmOverride = new AnimatorOverrideController();
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(m_CurrentWeapon.animatorController.overridesCount);
            m_CurrentWeapon.animatorController.GetOverrides(overrides);

            m_HandAndArmOverride.runtimeAnimatorController = m_ArmAnimator.runtimeAnimatorController;
            m_HandAndArmOverride.ApplyOverrides(overrides);

            m_ArmAnimator.runtimeAnimatorController = m_HandAndArmOverride;
            m_HandAnimator.runtimeAnimatorController = m_HandAndArmOverride;
        }
    }
}
