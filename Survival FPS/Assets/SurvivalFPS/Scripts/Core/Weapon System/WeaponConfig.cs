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
        [Serializable]
        public class RecoilData
        {
            public RecoilData() { }
            public RecoilData(RecoilData toCopy)
            {
                m_KickUpBase = toCopy.kickUpBase;
                m_KickLateralBase = toCopy.kickLateralBase;
                m_KickUpMax = toCopy.kickUpMax;
                m_KickLateralMax = toCopy.kickLateralMax;
                m_KickUpModifier = toCopy.kickUpModifier;
                m_KickLateralModifier = toCopy.kickLateralModifier;
                m_SideDirChange = toCopy.sideDirChange;
                m_RecoilResetModifier = toCopy.recoilResetModifier;
            }
            [Range(0.0f, 3.0f)] [SerializeField] private float m_KickUpBase = 2.0f; //start value of the up kick angle
            [Range(0.0f, 3.0f)] [SerializeField] private float m_KickLateralBase = 1.0f; //start value of the lateral kick angle
            [Range(5.0f, 60.0f)] [SerializeField] private float m_KickUpMax = 25.0f; //maximum value of the up kick angle
            [Range(5.0f, 35.0f)] [SerializeField] private float m_KickLateralMax = 5.0f; //maximum value of the lateral kick angle
            [Range(0.0f, 30.0f)] [SerializeField] private float m_KickUpModifier = 0.2f; //how significant the up kick is
            [Range(0.0f, 30.0f)] [SerializeField] private float m_KickLateralModifier = 0.4f; //how significant the lateral kick is
            [Range(1, 10)] [SerializeField] private int m_SideDirChange = 7; //how infrequent the gun will change lateral kick direction
            [Range(0.0f, 30.0f)] [SerializeField] private float m_RecoilResetModifier = 1.0f; //how quickly the gun will be stable again
            public float kickUpBase { get { return m_KickUpBase; } }
            public float kickLateralBase { get { return m_KickLateralBase; } }
            public float kickUpMax { get { return m_KickUpMax; } }
            public float kickLateralMax { get { return m_KickLateralMax; } }
            public float kickUpModifier { get { return m_KickUpModifier; } }
            public float kickLateralModifier { get { return m_KickLateralModifier; } }
            public int sideDirChange { get { return m_SideDirChange; } }
            public float recoilResetModifier { get { return m_RecoilResetModifier; } }
        }

        [Serializable]
        public class AccuracyData
        {
            public AccuracyData() { }
            public AccuracyData(AccuracyData toCopy) { }
            [Range(0.0f, 100.0f)] [SerializeField] private float m_BaseAccuracy = 80.0f;
            [Range(0.0f, 100.0f)] [SerializeField] private float m_AccuracyRecoveryRate = 50.0f;
            [Range(0.0f, 100.0f)] [SerializeField] private float m_AccuracyDropPerShot = 20.0f;
            public float baseAccuracy { get { return m_BaseAccuracy; } }
            public float accuracyRecoveryRate { get { return m_AccuracyRecoveryRate; } }
            public float accuracyDropPerShot { get { return m_AccuracyDropPerShot; } }
        }

        [SerializeField] protected GameObject m_GunModelPrefab; //an imported asset with model and animator
        [SerializeField] protected GameObject m_BulletHolePrefab; //TODO
        [SerializeField] protected RecoilData m_RecoilSetting;
        [SerializeField] protected AccuracyData m_AccuracySetting;
        [SerializeField] protected float m_Range;

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
        [SerializeField] protected AudioClip[] m_ReloadSounds;
        [SerializeField] protected AudioClip[] m_FireSounds;
        [SerializeField] protected AudioClip[] m_DryFireSounds;
        [SerializeField] protected AudioClip m_BringUpSound;
        [SerializeField] protected AudioClip m_WeaponEquipSound;

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
        public RecoilData recoilSettings { get { return m_RecoilSetting; } }
        public AccuracyData accuracySettings { get { return m_AccuracySetting; } }
        public float range { get { return m_Range; } }
        public int ammoCapacity { get { return m_AmmoCapacity; } }
        public float equipTime { get { return m_EquipTime; } }
        public float reloadTime { get { return m_ReloadTime; } }
        public float fireRate { get { return m_FireRate < 0 ? 1.0f / m_NumBulletsPerSec : m_FireRate; } }
        public bool recoilEnabled { get { return m_RecoilEnabled; } }
        public bool muzzleEffect { get { return m_MuzzleEffect; } }
        public bool spitShells { get { return m_SpitShells; } }
        
        //runtime properties
        public abstract int currentAmmo { get; }
        public abstract bool isActive { get; set; }
        public abstract bool isFiring { get; }
        public abstract bool isReloading { get; }



        /// <summary>
        /// this function will put the weapon in player's hand
        /// </summary>
        /// <param name="player"></param>
        public abstract void Initialize(FirstPersonController player);
        public abstract void Fire();
        public abstract void Reload();

        public void PlayFireSound()
        {
            if(m_FireSounds != null)
                m_AudioSource.PlayRandom(m_FireSounds);
        }
        public void PlayDryfireSound()
        {
            if(m_DryFireSounds != null)
                m_AudioSource.PlayRandom(m_DryFireSounds);
        }
        public void PlayReloadSound()
        {
            if(m_ReloadSounds != null)
                m_AudioSource.PlayRandom(m_ReloadSounds);
        }
        public void PlayBringUpSound()
        {
            if(m_BringUpSound != null)
            {
                m_AudioSource.PlayOneShot(m_BringUpSound);
            }
        }
        public void PlayWeaponEquipSound()
        {
            if (m_WeaponEquipSound != null)
            {
                m_AudioSource.PlayOneShot(m_WeaponEquipSound);
            }
        }
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
                //enable/disable the animator
                if (value)
                {
                    //enable the animator if it is previously disabled
                    m_WeaponBehaviour.animatorManager.EnableAnimator(m_GunAnimator);
                }
                else
                {
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
