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
    public class PlayerWeaponController : PlayerController
    {
        [SerializeField] private List<WeaponConfig> m_Weapons;
        [SerializeField] private bool m_AutoReload;

        //TODO: consider removing manually coupling arm & hand animators
        [Header("debug")]
        [SerializeField] private Animator m_HandAnimator;
        [SerializeField] private Animator m_ArmAnimator;
        [SerializeField] private RuntimeAnimatorController m_ArmAndHandController;
        private List<AnimatorOverrideController> m_HandAndArmOverrides = new List<AnimatorOverrideController>();
        //overrides for hand/arm corresponding to each weapon

        //internal variables
        private WeaponConfig m_CurrentWeapon;
        [SerializeField] private int m_CurrentWeaponAmmo;
        private int m_CurrentWeaponIndex = 0;
        private int m_LastWeaponIndex = -1;

        private PlayerManager m_PlayerManager;
        private FirstPersonController m_FPSController;
        private PlayerAnimatorManager m_PlayerAnimManager;

        private float m_EquipTimer;
        private bool m_ControlEnabled = true;

        //public properties
        public bool fireLeadingEdge { get; private set; }
        public WeaponConfig currentWeapon { get { return m_CurrentWeapon; } }

        //delegates
        public event Action<WeaponConfig> weaponChanged;

        //warning: this script may need to be executed after PlayerManager!
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

                m_HandAndArmOverrides.Add(new AnimatorOverrideController());
            }

            //generate overrides for the arm and hand
            for (int i = 0; i < m_HandAndArmOverrides.Count; i++)
            {
                SetHandAndArmAnimationOverride(m_HandAndArmOverrides[i], m_Weapons[i].animatorController);
            }

            SetCurrentWeapon(0);
        }

        private void Update()
        {
            if (!m_ControlEnabled) return;

            if (m_EquipTimer > 0.0f)
            {
                m_EquipTimer -= Time.deltaTime;
            }

            //if we have a weapon
            if (m_CurrentWeapon)
            {
                m_CurrentWeaponAmmo = m_CurrentWeapon.currentAmmo;

                //wait for the weapon to be fully equipped
                if (m_EquipTimer <= 0.0f)
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

                    if (Input.GetButtonDown("Last Weapon") && m_LastWeaponIndex != -1)
                    {
                        SetCurrentWeapon(m_LastWeaponIndex);
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
                        if (i == m_CurrentWeaponIndex) continue;

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
            if (m_CurrentWeapon)
            {
                m_LastWeaponIndex = m_CurrentWeaponIndex;
                //disable the mesh and the animator of the previous weapon
                m_CurrentWeapon.isActive = false;
            }

            m_CurrentWeapon = m_Weapons[index];
            m_CurrentWeaponIndex = index;

            //enable the weapon mesh and the weapon animator
            m_CurrentWeapon.isActive = true;

            //change the runtime controller of the hand and the arm
            //do not simply assign the weapon override controller,
            //because weapon anim controller is different from FPS anim controller
            //in terms of state machine behaviours
            m_ArmAnimator.runtimeAnimatorController = m_HandAndArmOverrides[index];
            m_HandAnimator.runtimeAnimatorController = m_HandAndArmOverrides[index];

            //put the weapon in the correct position
            PutCurrentWeaponInHand();

            //inform listeners
            weaponChanged(m_CurrentWeapon);

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

        private void SetHandAndArmAnimationOverride(AnimatorOverrideController armAndHandOverride, AnimatorOverrideController weaponOverride)
        {
            if (armAndHandOverride == null || weaponOverride == null)
            {
                Debug.LogWarning(typeof(PlayerWeaponController) + " - SetHandAndArmAnimationOverride: override controller is null");
                return;
            }

            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(weaponOverride.overridesCount);
            weaponOverride.GetOverrides(overrides);

            armAndHandOverride.runtimeAnimatorController = m_ArmAnimator.runtimeAnimatorController;
            armAndHandOverride.ApplyOverrides(overrides);
        }

#region controller interface implementation
        public override void StopControl()
        {
            m_ControlEnabled = false;
        }

        public override void ResumeControl()
        {
            m_ControlEnabled = true;
        }
#endregion
    }
}
