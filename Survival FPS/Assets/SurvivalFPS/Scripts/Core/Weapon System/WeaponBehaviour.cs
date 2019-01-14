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

        //set by config
        protected FirstPersonController m_Player;
        protected Animator m_Animator; //animator of this weapon only
        protected PlayerAnimatorManager m_AnimatorManager; //animator collection of the player
        protected AudioManager m_AudioManager; //audio manager of the player

        //states
        protected bool m_IsFiring;
        protected bool m_IsReloading;

        //public properties
        public FirstPersonController player { set { m_Player = value; } }
        public Animator animator { set { m_Animator = value; } }
        public AudioManager audioManager { set { m_AudioManager = value; }}
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

    /// <summary>
    /// Base class of all weapons;
    /// it is attached to the player game object and initialized by its corresponding config class.
    /// It implements basic functionalities which are:
    /// reloading / weapon bring up
    /// </summary>
    public abstract class WeaponBehaviour<T> : WeaponBehaviour where T : WeaponConfig
    {
        protected T m_WeaponConfig;
        public T weaponConfig { get { return m_WeaponConfig; } set { m_WeaponConfig = value; } }

        //muzzle flash
        protected ParticleSystem m_MuzzleFlash;

        //don't use these
        protected sealed override void Awake() { }
        protected sealed override void Start() { }

        public virtual void Initialize()
        {
            m_CurrentAmmo = m_WeaponConfig.ammoCapacity;

            //register events
            PlayerWeaponController weaponController = m_Player.GetComponent<PlayerWeaponController>();
            if (weaponController) weaponController.RegisterWeaponChangeEvent(OnWeaponChanged);
        
            //general settings of the muzzle flash particle effect
            m_MuzzleFlash = GameSceneManager.Instance.muzzleFlashParticleSystem;
            var mainModule = m_MuzzleFlash.main;
            mainModule.playOnAwake = false;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        public override void Reload()
        {
            //if we can reload
            if (m_CurrentAmmo < m_WeaponConfig.ammoCapacity)
            {
                //if we are not reloading
                if (m_ReloadingRoutine == null)
                {
                    m_ReloadingRoutine = _ReloadRoutine();
                    StartCoroutine(m_ReloadingRoutine);
                }
            }
        }

        private IEnumerator _ReloadRoutine()
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

            yield return new WaitForSecondsRealtime(0.6f);
            m_AudioManager.PlayRandom(() => m_WeaponConfig.isActive, m_WeaponConfig.reloadSounds);

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

        //callbacks
        protected virtual void OnWeaponChanged(WeaponConfig weaponInfo) 
        {
            //TODO remove string ref
            if (weaponInfo == m_WeaponConfig)
            {
                if (m_MuzzleFlash)
                {
                    var mainModule = m_MuzzleFlash.main;
                    //match the muzzle effect to the fire rate of this weapon
                    mainModule.startLifetime = m_WeaponConfig.fireRate;
                    //parent the effect to the weapon so that it will move with the weapon
                    m_MuzzleFlash.transform.parent = m_WeaponConfig.gunGameObject.transform;
                    m_MuzzleFlash.transform.localPosition = m_WeaponConfig.fireStartSpot.position;
                }

                m_AnimatorManager.Play("Bring Up Weapon", -1, 0.0f);

                m_AudioManager.PlayInSequence(0.1f, m_WeaponConfig.bringUpSound, m_WeaponConfig.weaponEquipSound);
            }
        }
    }
}
