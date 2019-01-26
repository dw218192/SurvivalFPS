using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core;
using SurvivalFPS.Core.FPS;
using SurvivalFPS.AI;
using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    //TODO
    public class PlayerInfo
    {
        public Collider collider = null;
        public CapsuleCollider meleeTrigger = null;
        public Camera playerCamera = null;
        public PlayerManager playerManager = null;
        public FirstPersonController playerMotionController = null;
        public PlayerAnimatorManager playerAnimatorManager = null;
        public AudioManager playerAudioManager = null;
        public PlayerWeaponController playerWeaponController = null;
    }

    /// <summary>
    /// a singleton manager that allows for fast lookup of AIs' colliders in the scene
    /// and other general information
    /// </summary>
    public class GameSceneManager : SingletonBehaviour<GameSceneManager>
    {
        [Header("Scene Special Effects")]
        [SerializeField] private ParticleSystem m_BloodParticleSystem;
        [SerializeField] private ParticleSystem m_MuzzleFlashParticleSystem;
        private ParticleSystem m_BloodParticleSystem_Instance;
        private ParticleSystem m_MuzzleFlashParticleSystem_Instance;
        [Header("Zombie Animation Controller Parameters")]
        [SerializeField] private string m_SpeedParameterName;
        [SerializeField] private string m_SeekingParameterName;
        [SerializeField] private string m_FeedingParameterName;
        [SerializeField] private string m_AttackParameterName;
        [SerializeField] private string m_HitParameterName;
        [SerializeField] private string m_HitTypeParameterName;
        [SerializeField] private string m_CrawlingParameterName;
        [SerializeField] private string m_RightHandAttackParameterName;
        [SerializeField] private string m_LeftHandAttackParameterName;
        [SerializeField] private string m_MouthAttackParameterName;
        [SerializeField] private string m_ReanimateFrontParameterName;
        [SerializeField] private string m_ReanimateBackParameterName;
        [SerializeField] private string m_IncapacitatedParameterName;
        [SerializeField] private string m_NoLegParameterName;
        [Header("Zombie Animation Controller Layer Indices")]
        [SerializeField] private int m_HitLayer;
        [Header("Zombie Animation Controller State Names")]
        [SerializeField] private string m_FeedingStateName;
        [Header("Player Animation Controller Parameters")]
        [SerializeField] private string m_ReloadParameterName;
        [SerializeField] private string m_ReloadCurveParameterName;
        [SerializeField] private string m_FireParameterName;
        [Header("Player Animation Controller State Names")]
        [SerializeField] private string m_ReloadStateName;
        [SerializeField] private string m_FireStateName;

        //hashes
        //Zombie Animation Controller Parameters
        private int m_SpeedParameterName_Hash = -1;
        private int m_SeekingParameterName_Hash = -1;
        private int m_FeedingParameterName_Hash = -1;
        private int m_AttackParameterName_Hash = -1;
        private int m_HitParameterName_Hash = -1;
        private int m_HitTypeParameterName_Hash = -1;
        private int m_CrawlingParameterName_Hash = -1;
        private int m_RightHandAttackParameterName_Hash = -1;
        private int m_LeftHandAttackParameterName_Hash = -1;
        private int m_MouthAttackParameterName_Hash = -1;
        private int m_ReanimateFrontParameterName_Hash = -1;
        private int m_ReanimateBackParameterName_Hash = -1;
        private int m_IncapacitatedParameterName_Hash = -1;
        private int m_NoLegParameterName_Hash = -1;
        //Player Animation Controller State Names
        private int m_FeedingStateName_Hash = -1;

        //Player Animation Controller Parameters
        private int m_ReloadParameterName_Hash = -1;
        private int m_ReloadCurveParameterName_Hash = -1;
        private int m_FireParameterName_Hash = -1;

        //Player Animation Controller State Names
        private int m_ReloadStateName_Hash = -1;
        private int m_FireStateName_Hash = -1;

        //layers
        private int m_ZombieBodyPartLayer = -1;
        private int m_PlayerLayer = -1;
        private int m_ObstaclesLayerMask = -1;
        private int m_ShootableLayerMask = -1;
        private int m_GeometryLayerMask = -1;

        private Dictionary<int, AIStateMachine> m_StateMachines = new Dictionary<int, AIStateMachine>();
        private Dictionary<int, PlayerInfo> m_PlayerInfos = new Dictionary<int, PlayerInfo>();

        //public properties
        //special effects
        public ParticleSystem bloodParticleSystem { get { return m_BloodParticleSystem_Instance; } }
        public ParticleSystem muzzleFlashParticleSystem { get { return m_MuzzleFlashParticleSystem_Instance; } }
        //layer information
        public int zombieBodyPartLayer { get { return m_ZombieBodyPartLayer; } }
        public int playerLayer { get { return m_PlayerLayer; } }
        public int obstaclesLayerMask { get { return m_ObstaclesLayerMask; } }
        public int shootableLayerMask { get { return m_ShootableLayerMask; } }
        public int geometryLayerMask { get { return m_GeometryLayerMask; } }
        //zombie animator controller Parameter info
        public int speedParameterName_Hash { get { return m_SpeedParameterName_Hash; } }
        public int seekingParameterName_Hash { get { return m_SeekingParameterName_Hash; } }
        public int feedingParameterName_Hash { get { return m_FeedingParameterName_Hash; } }
        public int attackParameterName_Hash { get { return m_AttackParameterName_Hash; } }
        public int hitParameterName_Hash { get { return m_HitParameterName_Hash; } }
        public int hitTypeParameterName_Hash { get { return m_HitTypeParameterName_Hash; } }
        public int crawlingParameterName_Hash { get { return m_CrawlingParameterName_Hash; } }
        public string rightHandAttackParameterName { get { return m_RightHandAttackParameterName; } }
        public string leftHandAttackParameterName { get { return m_LeftHandAttackParameterName; } }
        public string mouthAttackParameterName { get { return m_MouthAttackParameterName; } }
        public int ReanimateFrontParameterName_Hash { get { return m_ReanimateFrontParameterName_Hash; } }
        public int ReanimateBackParameterName_Hash { get { return m_ReanimateBackParameterName_Hash; } }
        public int IncapacitatedParameterName_Hash { get { return m_IncapacitatedParameterName_Hash; } }
        public int NoLegParameterName_Hash { get { return m_NoLegParameterName_Hash; } }
        //Zombie Animation Controller Layers
        public int hitLayerIndex { get { return m_HitLayer; }}
        //player animator state name info
        public int feedingStateName_Hash { get { return m_FeedingStateName_Hash; } }
        //player animator controller info
        public string reloadParameterName { get { return m_ReloadParameterName; } }
        public string reloadCurveParameterName { get { return m_ReloadCurveParameterName; } }
        public int reloadParameterHash { get { return m_ReloadParameterName_Hash; } }
        public int reloadCurveParameterHash { get { return m_ReloadCurveParameterName_Hash; } }
        public int fireParameterNameHash { get { return m_FireParameterName_Hash; } }
        //player animator state name info
        public int reloadStateNameHash { get { return m_ReloadStateName_Hash; } }
        public int fireStateNameHash { get { return m_FireStateName_Hash; } }


        protected override void Awake()
        {
            base.Awake();

            InstantiateEffects();
            ConvertStringsToHashes();
        }

        private void InstantiateEffects()
        {
            m_BloodParticleSystem_Instance = Instantiate(m_BloodParticleSystem);
            m_MuzzleFlashParticleSystem_Instance = Instantiate(m_MuzzleFlashParticleSystem);
        }

        private void ConvertStringsToHashes()
        {
            m_SpeedParameterName_Hash = Animator.StringToHash(m_SpeedParameterName);
            m_SeekingParameterName_Hash = Animator.StringToHash(m_SeekingParameterName);
            m_FeedingParameterName_Hash = Animator.StringToHash(m_FeedingParameterName);
            m_AttackParameterName_Hash = Animator.StringToHash(m_AttackParameterName);
            m_HitParameterName_Hash = Animator.StringToHash(m_HitParameterName);
            m_HitTypeParameterName_Hash = Animator.StringToHash(m_HitTypeParameterName);
            m_CrawlingParameterName_Hash = Animator.StringToHash(m_CrawlingParameterName);
            m_RightHandAttackParameterName_Hash = Animator.StringToHash(m_RightHandAttackParameterName);
            m_LeftHandAttackParameterName_Hash = Animator.StringToHash(m_LeftHandAttackParameterName);
            m_MouthAttackParameterName_Hash = Animator.StringToHash(m_MouthAttackParameterName);
            m_ReanimateFrontParameterName_Hash = Animator.StringToHash(m_ReanimateFrontParameterName);
            m_ReanimateBackParameterName_Hash = Animator.StringToHash(m_ReanimateBackParameterName);
            m_IncapacitatedParameterName_Hash = Animator.StringToHash(m_IncapacitatedParameterName);
            m_NoLegParameterName_Hash = Animator.StringToHash(m_NoLegParameterName);

            m_PlayerLayer = LayerMask.NameToLayer("Player");
            m_ZombieBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

            m_GeometryLayerMask = LayerMask.GetMask("Obstacle");
            m_ObstaclesLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator", "Obstacle");
            m_ShootableLayerMask = LayerMask.GetMask("AI Body Part", "Visual Aggravator", "Obstacle");

            m_ReloadParameterName_Hash = Animator.StringToHash(m_ReloadParameterName);
            m_ReloadCurveParameterName_Hash = Animator.StringToHash(m_ReloadCurveParameterName);
            m_FireParameterName_Hash = Animator.StringToHash(m_FireParameterName);

            m_ReloadStateName_Hash = Animator.StringToHash(m_ReloadStateName);
            m_FireStateName_Hash = Animator.StringToHash(m_FireStateName);
        }

#region Game Scene Information Registration Functions
        // these functions should be called from other scripts that wish to register
        // themselves in the game scene manager


        /// <summary>
        /// Registers an AI's collider in the scene; the key is the InstanceID of that collider, and the statemachine is its owner (the AI)
        /// </summary>
        /// <param name="key"> the InstanceID of the collider </param>
        /// <param name="stateMachine"> the AI that owns the collider </param>
        public void RegisterAIStateMachineByColliderID(int key, AIStateMachine stateMachine)
        {
            if (!m_StateMachines.ContainsKey(key))
            {
                m_StateMachines[key] = stateMachine;
            }
        }
        /// <summary>
        /// given a collider's instance ID, returns its owner
        /// </summary>
        /// <param name="key"> the InstanceID of the collider </param>
        /// <returns></returns>
        public AIStateMachine GetAIStateMachineByColliderID(int key)
        {
            AIStateMachine machine = null;
            if (m_StateMachines.TryGetValue(key, out machine))
            {
                return machine;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Registers the player.
        /// </summary>
        /// <param name="key"> the InstanceID of the player collider </param>
        /// <param name="playerInfo">Player.</param>
        public void RegisterPlayer(int key, PlayerInfo playerInfo)
        {
            if(!m_PlayerInfos.ContainsKey(key))
            {
                m_PlayerInfos[key] = playerInfo;
            }
        }

        /// <summary>
        /// Gets the player info.
        /// </summary>
        /// <returns>The player info.</returns>
        /// <param name="key">Key.</param>
        public PlayerInfo GetPlayerInfo(int key)
        {
            PlayerInfo info = null;
            if (m_PlayerInfos.TryGetValue(key, out info))
            {
                return info;
            }

            return null;
        }

        public float GetAnimatorFloatValue(AIStateMachine stateMachine, string name)
        {
            float val = 0.0f;

            if (stateMachine.animator)
            {
                if (name.Equals(m_RightHandAttackParameterName))
                {
                    val = stateMachine.animator.GetFloat(m_RightHandAttackParameterName_Hash);
                }
                else if (name.Equals(m_LeftHandAttackParameterName))
                {
                    val = stateMachine.animator.GetFloat(m_LeftHandAttackParameterName_Hash);
                }
                else if (name.Equals(m_MouthAttackParameterName))
                {
                    val = stateMachine.animator.GetFloat(m_MouthAttackParameterName_Hash);
                }
                else
                {
                    val = stateMachine.animator.GetFloat(name);
                }
            }

            return val;
        }
#endregion
    }
}

