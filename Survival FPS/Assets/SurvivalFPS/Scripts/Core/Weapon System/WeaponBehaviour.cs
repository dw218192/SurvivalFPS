using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.FPS;

namespace SurvivalFPS.Core.Weapon
{
    /// <summary>
    /// a runtime component attached to the player game object,
    /// which should be managed by the config classes
    /// </summary>
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        protected int m_CurrentAmmo = 0;
        protected float m_FireTimer = 0.0f;

        protected FirstPersonController m_Player;
        protected Animator m_Animator; //animator of this weapon only
        protected PlayerAnimatorManager m_AnimatorManager; //animator collection of the player

        //states
        protected bool m_IsFiring;
        protected bool m_IsReloading;

        //public properties
        public FirstPersonController player { set { m_Player = value; } }
        public Animator animator { set { m_Animator = value; } }
        public PlayerAnimatorManager animatorManager { get { return m_AnimatorManager; } set { m_AnimatorManager = value; } }
        public int currentAmmo { get { return m_CurrentAmmo; } }
        public bool isFiring { get { return m_IsFiring; } }
        public bool isReloading { get { return m_IsReloading; } }

        protected IEnumerator m_ReloadingRoutine = null;

        public abstract void Fire();
        public abstract void Reload();
        public virtual void Recoil() { }
        public virtual void SpitShells() { }
        public virtual void PlayMuzzleEffect() { }

        protected virtual void Awake() { }
        protected virtual void Start() { }
    }

    public abstract class WeaponBehaviour<T> : WeaponBehaviour where T : WeaponConfig
    {
        protected T m_WeaponConfig;
        public T weaponConfig { get { return m_WeaponConfig; } set { m_WeaponConfig = value; } }

        //don't use these
        protected sealed override void Awake() { }
        protected sealed override void Start() { }

        public virtual void Initialize()
        {
            m_CurrentAmmo = m_WeaponConfig.ammoCapacity;
        }

        public override void Reload()
        {
            //if we can reload
            if (m_CurrentAmmo < m_WeaponConfig.ammoCapacity)
            {
                //if we are not reloading
                if (m_ReloadingRoutine == null)
                {
                    m_ReloadingRoutine = ReloadRoutine();
                    StartCoroutine(m_ReloadingRoutine);
                }
            }
        }

        private IEnumerator ReloadRoutine()
        {
            //make sure the weapon is active
            if (!m_WeaponConfig.isActive)
            {
                yield break;
            }

            m_FireTimer = -m_WeaponConfig.reloadTime;
            //set the parameter to begin the transition into the reloading animation
            if (m_AnimatorManager)
            {
                m_AnimatorManager.SetBool(GameSceneManager.Instance.reloadParameterHash, true);
            }

            //wait for the reload animation to begin playing
            while (m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != GameSceneManager.Instance.reloadStateNameHash)
            {
                //if the weapon got switched out in the middle of the reloading
                if (!m_WeaponConfig.isActive)
                {
                    ExitReloading();
                    yield break;
                }

                yield return null;
            }

            //the reloading animation starts playing
            m_IsReloading = true;
            yield return new WaitForSecondsRealtime(0.15f);

            m_WeaponConfig.PlayReloadSound();

            //only increment the ammo when the animation reaches a specific point (i.e. magazine changed)
            while (m_Animator.GetFloat(GameSceneManager.Instance.reloadCurveParameterName) < -0.5f)
            {
                //if the weapon got switched out in the middle of the reloading
                if (!m_WeaponConfig.isActive)
                {
                    ExitReloading();
                    yield break;
                }

                yield return null;
            }
            //we've finished reloading
            m_CurrentAmmo = m_WeaponConfig.ammoCapacity;
            ExitReloading();
        }

        private void ExitReloading()
        {
            m_FireTimer = 0.0f;

            //clean the flags
            m_ReloadingRoutine = null;
            m_IsReloading = false;

            //turn off the animator flag of this weapon and other related animators (arms/hands/etc)
            if (m_AnimatorManager)
            {
                m_Animator.SetBool(GameSceneManager.Instance.reloadParameterHash, false);
                m_AnimatorManager.SetBool(GameSceneManager.Instance.reloadParameterHash, false);
            }
        }
    }
}
