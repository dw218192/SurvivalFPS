using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;
using SurvivalFPS.Core.FPS;
using SurvivalFPS.Core.Audio;

namespace SurvivalFPS.Core.Weapon
{
    public abstract class WeaponConfig : ScriptableObject
    {
        //an imported asset with model and animator
        [SerializeField] protected GameObject m_StaticModelPrefab;
        [SerializeField] protected GameObject m_FPSModelPrefab;
        //TODO: stealth settings
        //damage settings
        [SerializeField] protected DamageData m_DamageSetting;
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

        [SerializeField] protected AnimatorOverrideController m_AnimatorController;
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
        [SerializeField] private AudioCollection m_AudioCollection;

        //set in initialization
        protected AudioSource m_AudioSource;
        protected GameObject m_GunGameObject; //the runtime gun game object
        protected SkinnedMeshRenderer m_GunMesh; //reference to its mesh renderer
        protected Animator m_GunAnimator; //this gun's animator only
        protected PlayerAnimatorManager m_PlayerAnimManager; //the player's anim manager that manages enabled animators on its list
        protected float m_FireRate = -1.0f;

        //properties
        public AnimatorOverrideController animatorController { get { return m_AnimatorController; } }
        public Animator animator { get { return m_GunAnimator; } }
        public GameObject staticModelPrefab { get { return m_StaticModelPrefab; } }
        public GameObject gunGameObject { get { return m_GunGameObject; } }
        public Transform gripTransform { get { return m_GripTransform; } }
        public Transform fireStartSpot { get { return m_FireStartSpot; } }
        public RecoilData recoilSettingsWhenStill { get { return m_RecoilSettingWhenStill; } }
        public RecoilData recoilSettingsWhenWalking { get { return m_RecoilSettingWhenWalking; } }
        public RecoilData recoilSettingsWhenCrouching { get { return m_RecoilSettingWhenCrouching; } }
        public AccuracyData accuracySettingsWhenStill { get { return m_AccuracySettingWhenStill; } }
        public AccuracyData accuracySettingsWhenWalking { get { return m_AccuracySettingWhenWalking; } }
        public AccuracyData accuracySettingsWhenCrouching { get { return m_AccuracySettingWhenCrouching; } }
        public DamageData damageSetting { get { return m_DamageSetting; }}

        public float range { get { return m_Range; } }
        public Texture2D crossHairTexture { get { return m_CrossHairTexture; } }
        public int ammoCapacity { get { return m_AmmoCapacity; } }
        public float equipTime { get { return m_EquipTime; } }
        public float reloadTime { get { return m_ReloadTime; } }
        public float fireRate { get { return m_FireRate < 0 ? 1.0f / m_NumBulletsPerSec : m_FireRate; } }
        public bool recoilEnabled { get { return m_RecoilEnabled; } }
        public bool muzzleEffect { get { return m_MuzzleEffect; } }
        public bool spitShells { get { return m_SpitShells; } }
        public AudioCollection audioCollection { get { return m_AudioCollection; }}

        //runtime properties
        //they need to be abstract because weapon behavior is not defined at this level
        public abstract int currentAmmo { get; }
        public abstract bool isActive { get; set; }
        public abstract bool isFiring { get; }
        public abstract bool isReloading { get; }
        //TODO: silence
        public abstract bool isSilenced { get; }

        /// <summary>
        /// this function must be called before using the weapon system
        /// </summary>
        /// <param name="player"></param>
        public abstract void Initialize(PlayerManager player);
        public abstract void TryFire();
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
                    m_PlayerAnimManager.EnableAnimator(m_GunAnimator);
                }
                else
                {
                    //enable the weapon behaviour that's attached to whoever is using it
                    m_WeaponBehaviour.enabled = false;
                    //disable the animator
                    m_PlayerAnimManager.DisableAnimator(m_GunAnimator);
                }

                //enable/disable the mesh
                m_GunMesh.enabled = value;
            }
        }
        public override bool isFiring { get { return m_WeaponBehaviour.isFiring; } }
        public override bool isReloading { get { return m_WeaponBehaviour.isReloading; } }
        //TODO
        public override bool isSilenced { get { throw new System.NotImplementedException(); } }

        public override void Initialize(PlayerManager player)
        {
            m_WeaponBehaviour = player.gameObject.AddComponent<T>();
            m_PlayerAnimManager = player.GetComponent<PlayerAnimatorManager>();

            SetModel(player);
            SetAudioSource(player);
            SetAnimator(player);
        }

        //initialization functions
        private void SetModel(PlayerManager player)
        {
            m_GunGameObject = Instantiate(m_FPSModelPrefab, player.weaponSocket);
            m_GunMesh = m_GunGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            m_GunMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        //---initialization functions---
        private void SetAnimator(PlayerManager player)
        {
            //the gun's animator
            m_GunAnimator = m_GunGameObject.GetComponent<Animator>();

            int key = player.informationKey;
            PlayerInfo info = GameSceneManager.Instance.GetPlayerInfo(key);

            //set the links to the behaviour component
            weaponBehaviour.FPSController = info.playerMotionController;
            weaponBehaviour.animator = m_GunAnimator;
            weaponBehaviour.weaponController = info.playerWeaponController;
            weaponBehaviour.animatorManager = info.playerAnimatorManager;
            weaponBehaviour.playerCamera = info.playerCamera;

            //add the animator to the player animator manager
            info.playerAnimatorManager.AddAnimator(m_GunAnimator);

            //set the runtime controller of this gun animator
            m_GunAnimator.runtimeAnimatorController = m_AnimatorController;

            //initialize state machine behaviors
            PlayerStateMachineLink[] stateMachineBehaviors = m_GunAnimator.GetBehaviours<PlayerStateMachineLink>();
            foreach (PlayerStateMachineLink stateMachineBehavior in stateMachineBehaviors)
            {
                stateMachineBehavior.playerWeaponController = info.playerWeaponController;
            }
        }
        private void SetAudioSource(PlayerManager player)
        {
            m_AudioSource = player.GetComponent<AudioSource>();
        }
        //-----------------------------
        public override void TryFire()
        {
            weaponBehaviour.TryFire();
        }
        public override void Reload()
        {
            weaponBehaviour.Reload();
        }
    }
}
