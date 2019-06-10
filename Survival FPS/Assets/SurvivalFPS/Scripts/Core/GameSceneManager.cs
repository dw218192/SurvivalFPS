using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core;
using SurvivalFPS.Core.PlayerInteraction;
using SurvivalFPS.Core.FPS;
using SurvivalFPS.AI;
using SurvivalFPS.Utility;
using SurvivalFPS.Core.Audio;
using SurvivalFPS.Core.Inventory;

namespace SurvivalFPS
{
    public class PlayerInfo
    {
        public Collider collider = null;
        public CapsuleCollider meleeTrigger = null;
        public Camera playerCamera = null;
        public PlayerManager playerManager = null;
        public FirstPersonController playerMotionController = null;
        public PlayerAnimatorManager playerAnimatorManager = null;
        public PlayerWeaponController playerWeaponController = null;
        public PlayerInventorySystem playerInventorySystem = null;
    }

    /// <summary>
    /// a singleton manager that allows for fast lookup of AIs' colliders in the scene
    /// and other general information
    /// </summary>
    public class GameSceneManager : SingletonBehaviour<GameSceneManager>
    {
        [Header("Scene Effects")]
        [SerializeField] private ParticleSystem m_BloodParticleSystem;
        [SerializeField] private ParticleSystem m_MuzzleFlashParticleSystem;
        [SerializeField] private AudioCollection m_BulletHitSounds;
        [SerializeField] private AudioCollection m_PlayerInjuredSounds;
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
        [SerializeField] private string m_BehaviourStateParameterName;
        [Header("Zombie Animation Controller Layer Indices")]
        [SerializeField] private int m_UpperBodyLayer;
        [SerializeField] private int m_LowerBodyLayer;
        [SerializeField] private int m_HitLayer;
        [Header("Zombie Animation Controller State Names")]
        [SerializeField] private string m_FeedingStateName;
        [SerializeField] private string m_CrawlFeedingStateName;
        [Header("Player Animation Controller Parameters")]
        [SerializeField] private string m_ReloadParameterName;
        [SerializeField] private string m_ReloadCurveParameterName;
        [SerializeField] private string m_FireParameterName;
        [Header("Player Animation Controller State Names")]
        [SerializeField] private string m_BringUpStateName;
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
        private int m_BehaviourStateParameterName_Hash = -1;
        //Zombie Animation Controller State Names
        private int m_FeedingStateName_Hash = -1;
        private int m_CrawlFeedingStateName_Hash = -1;
        //Player Animation Controller Parameters
        private int m_ReloadParameterName_Hash = -1;
        private int m_ReloadCurveParameterName_Hash = -1;
        private int m_FireParameterName_Hash = -1;
        //Player Animation Controller State Names
        private int m_BringUpStateName_Hash = -1;
        private int m_ReloadStateName_Hash = -1;
        private int m_FireStateName_Hash = -1;
        //layers
        private int m_AIBodyPartLayer = -1;
        private int m_PlayerLayer = -1;
        private int m_AITriggerLayer = -1;
        private int m_AIEntityLayer = -1;
        private int m_AIEntityTriggerLayer = -1;
        private int m_InteractiveLayer = -1;
        private int m_VisualAggLayer = -1;
        private int m_AudioAggLayer = -1;

        //masks used in raycast mostly
        private int m_VisualRaycastLayerMask = -1;
        private int m_ShootableLayerMask = -1;
        private int m_GeometryLayerMask = -1;
        private int m_InteractiveLayerMask = -1;

        private Dictionary<int, AIStateMachine> m_StateMachines = new Dictionary<int, AIStateMachine>();
        private Dictionary<int, InteractiveItem> m_InteractiveItems = new Dictionary<int, InteractiveItem>();
        private Dictionary<int, PlayerInfo> m_PlayerInfos = new Dictionary<int, PlayerInfo>();

        //public properties
        //special effects
        public ParticleSystem bloodParticleSystem { get { return m_BloodParticleSystem_Instance; } }
        public ParticleSystem muzzleFlashParticleSystem { get { return m_MuzzleFlashParticleSystem_Instance; } }
        public AudioCollection bulletHitSounds { get { return m_BulletHitSounds; } }
        public AudioCollection playerInjuredSounds { get { return m_PlayerInjuredSounds; } }
        //layer information
        public int aITriggerLayer { get { return m_AITriggerLayer; }}
        public int aIBodyPartLayer { get { return m_AIBodyPartLayer; } }
        public int playerLayer { get { return m_PlayerLayer; } }
        public int aIEntityLayer { get { return m_AIEntityLayer; }}
        public int aIEntityTriggerLayer { get { return m_AIEntityTriggerLayer; }}
        public int interactiveLayer { get { return m_InteractiveLayer; } }
        public int visualAggravatorLayer { get { return m_VisualAggLayer; } }
        public int audioAggravatorLayer { get { return m_AudioAggLayer; } }
        //layer masks
        public int visualRaycastLayerMask { get { return m_VisualRaycastLayerMask; } }
        public int shootableLayerMask { get { return m_ShootableLayerMask; } }
        public int geometryLayerMask { get { return m_GeometryLayerMask; } }
        public int interactiveLayerMask { get { return m_InteractiveLayerMask; } }
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
        public int behaviourStateParameterName_Hash { get { return m_BehaviourStateParameterName_Hash; } }
        //Zombie Animation Controller Layers
        public int upperbodyIndex { get { return m_UpperBodyLayer; }}
        public int lowerbodyIndex { get { return m_LowerBodyLayer; }}
        public int hitLayerIndex { get { return m_HitLayer; }}
        //player animator state name info
        public int feedingStateName_Hash { get { return m_FeedingStateName_Hash; } }
        public int crawlFeedingStateName_Hash { get { return m_CrawlFeedingStateName_Hash; } }
        //player animator controller info
        public string reloadParameterName { get { return m_ReloadParameterName; } }
        public string reloadCurveParameterName { get { return m_ReloadCurveParameterName; } }
        public int reloadParameterHash { get { return m_ReloadParameterName_Hash; } }
        public int reloadCurveParameterHash { get { return m_ReloadCurveParameterName_Hash; } }
        public int fireParameterNameHash { get { return m_FireParameterName_Hash; } }
        //player animator state name info
        public int bringUpStateNameHash { get { return m_BringUpStateName_Hash; } }
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
            m_BehaviourStateParameterName_Hash = Animator.StringToHash(m_BehaviourStateParameterName);

            m_PlayerLayer = LayerMask.NameToLayer("Player");
            m_AIBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
            m_AITriggerLayer = LayerMask.NameToLayer("AI Trigger");
            m_AIEntityLayer = LayerMask.NameToLayer("AI Entity");
            m_AIEntityTriggerLayer = LayerMask.NameToLayer("AI Entity Trigger");
            m_InteractiveLayer = LayerMask.NameToLayer("Interactive Items");
            m_AudioAggLayer = LayerMask.NameToLayer("Audio Aggravator");
            m_VisualAggLayer = LayerMask.NameToLayer("Visual Aggravator");

            m_GeometryLayerMask = LayerMask.GetMask("Obstacle");
            m_VisualRaycastLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator", "Obstacle");
            m_ShootableLayerMask = LayerMask.GetMask("AI Body Part", "Visual Aggravator", "Obstacle");
            m_InteractiveLayerMask = LayerMask.GetMask("Interactive Items", "AI Body Part", "Visual Aggravator", "Obstacle");

            m_ReloadParameterName_Hash = Animator.StringToHash(m_ReloadParameterName);
            m_ReloadCurveParameterName_Hash = Animator.StringToHash(m_ReloadCurveParameterName);
            m_FireParameterName_Hash = Animator.StringToHash(m_FireParameterName);

            m_FeedingStateName_Hash = Animator.StringToHash(m_FeedingStateName);
            m_CrawlFeedingStateName_Hash = Animator.StringToHash(m_CrawlFeedingStateName);

            m_BringUpStateName_Hash = Animator.StringToHash(m_BringUpStateName);
            m_ReloadStateName_Hash = Animator.StringToHash(m_ReloadStateName);
            m_FireStateName_Hash = Animator.StringToHash(m_FireStateName);
        }

#region Game Scene Item Registration Functions
        // these functions should be called from other scripts that wish to register
        // themselves in the game scene manager


        /// <summary>
        /// Registers an AI in the scene; the key is the InstanceID of that collider, and the statemachine is its owner (the AI)
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
        /// given a collider's instance ID, returns its AI owner
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

            return null;
        }

        /// <summary>
        /// Registers an interactive item in the scene; the key is the InstanceID of that collider
        /// </summary>
        /// <param name="key"> the InstanceID of the collider </param>
        /// <param name="iteractiveItem"> the interactive item that owns the collider </param>
        public void RegisterInteractiveItemByColliderID(int key, InteractiveItem iteractiveItem)
        {
            if (!m_InteractiveItems.ContainsKey(key))
            {
                m_InteractiveItems[key] = iteractiveItem;
            }
        }

        /// <summary>
        /// given a collider's instance ID, returns the interactive item it belongs to
        /// </summary>
        /// <param name="key"> the InstanceID of the collider </param>
        /// <returns></returns>
        public InteractiveItem GetInteractiveItemByColliderID(int key)
        {
            InteractiveItem item = null;
            if(m_InteractiveItems.TryGetValue(key, out item))
            {
                return item;
            }

            return null;
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

