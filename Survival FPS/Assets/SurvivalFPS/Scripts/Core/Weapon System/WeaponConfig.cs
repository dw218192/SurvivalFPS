using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;
using SurvivalFPS.Core.FPS;
using System;

namespace SurvivalFPS.Core.Weapon
{
    public abstract class WeaponConfig : ScriptableObject
    {
        //an imported asset with model and animator
        [SerializeField] protected GameObject m_GunModelPrefab;
        [SerializeField] protected GameObject m_BulletHolePrefab; //TODO

        //recoil settings
        [SerializeField] protected RecoilData m_RecoilSettingWhenStill;
        [SerializeField] protected RecoilData m_RecoilSettingWhenWalking;
        [SerializeField] protected RecoilData m_RecoilSettingWhenCrouching;
        //accuracy settings
        [SerializeField] protected AccuracyData m_AccuracySettingWhenStill;
        [SerializeField] protected AccuracyData m_AccuracySettingWhenWalking;
        [SerializeField] protected AccuracyData m_AccuracySettingWhenCrouching;
        //range of the weapon, not the effective range
        [SerializeField] protected float m_Range;
        //crosshair settings
        [SerializeField] private Texture2D m_CrossHairTexture;

        [SerializeField] protected RuntimeAnimatorController m_AnimatorController;
        [SerializeField] protected Transform m_GripTransform;
        [SerializeField] protected Transform m_FireStartSpot;
        [SerializeField] protected int m_AmmoCapacity;
        [SerializeField] protected float m_EquipTime;  //time it takes to equip this weapon in secs
        [SerializeField] protected float m_ReloadTime; //time it takes to reload this weapon in secs
        [SerializeField] protected int m_NumBulletsPerSec;
        [SerializeField] protected bool m_RecoilEnabled = true;
        [SerializeField] protected bool m_MuzzleEffect = true;
        [SerializeField] protected bool m_SpitShells = true;

        //sounds
        [SerializeField] private AudioClip[] m_ReloadSounds;
        [SerializeField] private AudioClip[] m_FireSounds;
        [SerializeField] private AudioClip[] m_DryFireSounds;
        [SerializeField] private AudioClip m_BringUpSound;
        [SerializeField] private AudioClip m_WeaponEquipSound;

        //set in initialization
        protected AudioSource m_AudioSource;
        protected GameObject m_GunGameObject; //the runtime gun game object
        protected SkinnedMeshRenderer m_GunMesh; //reference to its mesh renderer
        protected Animator m_GunAnimator;
        protected float m_FireRate = -1.0f;

        //properties
        public RuntimeAnimatorController animatorController { get { return m_AnimatorController; } }
        public Animator animator { get { return m_GunAnimator; } }
        public GameObject gunGameObject { get { return m_GunGameObject; } }
        public Transform gripTransform { get { return m_GripTransform; } }
        public Transform fireStartSpot { get { return m_FireStartSpot; } }
        public RecoilData recoilSettingsWhenStill { get { return m_RecoilSettingWhenStill; } }
        public RecoilData recoilSettingsWhenWalking { get { return m_RecoilSettingWhenWalking; } }
        public RecoilData recoilSettingsWhenCrouching { get { return m_RecoilSettingWhenCrouching; } }
        public AccuracyData accuracySettingsWhenStill { get { return m_AccuracySettingWhenStill; } }
        public AccuracyData accuracySettingsWhenWalking { get { return m_AccuracySettingWhenWalking; } }
        public AccuracyData accuracySettingsWhenCrouching { get { return m_AccuracySettingWhenCrouching; } }

        public float range { get { return m_Range; } }
        public Texture2D crossHairTexture { get { return m_CrossHairTexture; } }
        public int ammoCapacity { get { return m_AmmoCapacity; } }
        public float equipTime { get { return m_EquipTime; } }
        public float reloadTime { get { return m_ReloadTime; } }
        public float fireRate { get { return m_FireRate < 0 ? 1.0f / m_NumBulletsPerSec : m_FireRate; } }
        public bool recoilEnabled { get { return m_RecoilEnabled; } }
        public bool muzzleEffect { get { return m_MuzzleEffect; } }
        public bool spitShells { get { return m_SpitShells; } }
        public AudioClip[] reloadSounds { get { return m_ReloadSounds; } }
        public AudioClip[] fireSounds { get { return m_FireSounds; } }
        public AudioClip[] dryFireSounds { get { return m_DryFireSounds; } }
        public AudioClip bringUpSound { get { return m_BringUpSound; } }
        public AudioClip weaponEquipSound { get { return m_WeaponEquipSound; } }

        //runtime properties
        public abstract int currentAmmo { get; }
        public abstract bool isActive { get; set; }
        public abstract bool isFiring { get; }
        public abstract bool isReloading { get; }



        /// <summary>
        /// this function must be called before using the weapon system
        /// </summary>
        /// <param name="player"></param>
        public abstract void Initialize(FirstPersonController player);
        public abstract void Fire();
        public abstract void Reload();
    }

    public abstract class WeaponConfig<T> : WeaponConfig where T : WeaponBehaviour
    {
        protected T m_WeaponBehaviour;
        public T weaponBehaviour { get { return m_WeaponBehaviour; } set { m_WeaponBehaviour = value; } }

        //runtime properties
        public override int currentAmmo { get { return m_WeaponBehaviour.currentAmmo; } }
        public override bool isActive
        {
            get
            {
                if (m_GunMesh) return m_GunMesh.enabled;
                return false;
            }
            set
            {
                //enable/disable the animator so that it factors into SetX functions
                if (value)
                {
                    //enable the weapon behaviour that's attached to whoever is using it
                    m_WeaponBehaviour.enabled = true;
                    //enable the animator if it is previously disabled
                    m_WeaponBehaviour.animatorManager.EnableAnimator(m_GunAnimator);
                }
                else
                {
                    //enable the weapon behaviour that's attached to whoever is using it
                    m_WeaponBehaviour.enabled = false;
                    //disable the animator
                    m_WeaponBehaviour.animatorManager.DisableAnimator(m_GunAnimator);
                }

                //enable/disable the mesh
                m_GunMesh.enabled = value;
            }
        }
        public override bool isFiring { get { return m_WeaponBehaviour.isFiring; } }
        public override bool isReloading { get { return m_WeaponBehaviour.isReloading; } }

        protected void SetModel(FirstPersonController player)
        {
            m_GunGameObject = GameObject.Instantiate(m_GunModelPrefab, player.weaponSocket);
            m_GunMesh = m_GunGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        //---initialization functions---
        protected void SetAnimator(FirstPersonController player)
        {
            //the gun's animator
            m_GunAnimator = m_GunGameObject.GetComponent<Animator>();

            //set the links to the behaviour component
            weaponBehaviour.player = player;
            weaponBehaviour.animator = m_GunAnimator;
            weaponBehaviour.animatorManager = player.playerAnimatorManager;
            weaponBehaviour.audioManager = player.audioManager;

            //add the animator to the player animator manager
            player.playerAnimatorManager.AddAnimator(m_GunAnimator);

            //set the runtime controller of this gun animator
            m_GunAnimator.runtimeAnimatorController = m_AnimatorController;
        }
        protected void SetAudioSource(FirstPersonController player)
        {
            m_AudioSource = player.GetComponent<AudioSource>();
        }
        //-----------------------------
        public override void Fire()
        {
            weaponBehaviour.Fire();
        }
        public override void Reload()
        {
            weaponBehaviour.Reload();
        }
    }
}
