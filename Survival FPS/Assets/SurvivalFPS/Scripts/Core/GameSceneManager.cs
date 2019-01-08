using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.AI;
using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    /// <summary>
    /// a singleton manager that allows for fast lookup of AIs' colliders in the scene
    /// and other general information
    /// </summary>
    public class GameSceneManager : SingletonBehaviour<GameSceneManager>
    {
        [Header("Scene Special Effects")]
        [SerializeField] private ParticleSystem m_BloodParticleSystem;
        [SerializeField] private ParticleSystem m_MuzzleFlashParticleSystem;

        [Header("Zombie Animation Controller Parameters")]
        [SerializeField] private string m_RightHandAttackParameterName;
        [SerializeField] private string m_LeftHandAttackParameterName;
        [SerializeField] private string m_MouthAttackParameterName;
        [Header("Player Animation Controller State Names")]
        [SerializeField] private string m_FeedingStateName;
        [Header("Player Animation Controller Parameters")]
        [SerializeField] private string m_ReloadParameterName;
        [SerializeField] private string m_ReloadCurveParameterName;
        [SerializeField] private string m_FireParameterName;
        [Header("Player Animation Controller State Names")]
        [SerializeField] private string m_ReloadStateName;
        [SerializeField] private string m_FireStateName;

        //hashes
        private int m_RightHandAttackParameterName_Hash = -1;
        private int m_LeftHandAttackParameterName_Hash = -1;
        private int m_MouthAttackParameterName_Hash = -1;

        private int m_FeedingStateName_Hash = -1;

        private int m_ReloadParameterName_Hash = -1;
        private int m_ReloadCurveParameterName_Hash = -1;
        private int m_FireParameterName_Hash = -1;

        private int m_ReloadStateName_Hash = -1;
        private int m_FireStateName_Hash = -1;

        //layers
        private int m_ZombieBodyPartLayer = -1;
        private int m_PlayerLayer = -1;
        private int m_ObstaclesLayerMask = -1;
        private int m_ShootableLayerMask = -1;

        private Dictionary<int, AIStateMachine> m_StateMachines = new Dictionary<int, AIStateMachine>();


        //public properties
        //special effects
        public ParticleSystem bloodParticleSystem { get { return m_BloodParticleSystem; } }
        public ParticleSystem muzzleFlashParticleSystem { get { return m_MuzzleFlashParticleSystem; } }
        //layer information
        public int zombieBodyPartLayer { get { return m_ZombieBodyPartLayer; } }
        public int playerLayer { get { return m_PlayerLayer; } }
        public int obstaclesLayerMask { get { return m_ObstaclesLayerMask; } }
        public int shootableLayerMask { get { return m_ShootableLayerMask; } }
        //zombie animator controller info
        public string rightHandAttackParameterName { get { return m_RightHandAttackParameterName; } }
        public string leftHandAttackParameterName { get { return m_LeftHandAttackParameterName; } }
        public string mouthAttackParameterName { get { return m_MouthAttackParameterName; } }
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
            m_PlayerLayer = LayerMask.NameToLayer("Player");
            m_ZombieBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
            m_ObstaclesLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator", "Obstacle");
            m_ShootableLayerMask = LayerMask.GetMask("AI Body Part", "Visual Aggravator", "Obstacle");

            m_RightHandAttackParameterName_Hash = Animator.StringToHash(m_RightHandAttackParameterName);
            m_LeftHandAttackParameterName_Hash = Animator.StringToHash(m_LeftHandAttackParameterName);
            m_MouthAttackParameterName_Hash =  Animator.StringToHash(m_MouthAttackParameterName);

            m_ReloadParameterName_Hash = Animator.StringToHash(m_ReloadParameterName);
            m_ReloadCurveParameterName_Hash = Animator.StringToHash(m_ReloadCurveParameterName);
            m_FireParameterName_Hash = Animator.StringToHash(m_FireParameterName);

            m_ReloadStateName_Hash = Animator.StringToHash(m_ReloadStateName);
            m_FireStateName_Hash = Animator.StringToHash(m_FireStateName);
        }

        /// <summary>
        /// registers an AI's collider in the scene; the key is the InstanceID of that collider, and the statemachine is its owner (the AI)
        /// </summary>
        /// <param name="key"> the InstanceID of the collider </param>
        /// <param name="stateMachine"> the AI that owns the collider </param>
        public void RegisterAIStateMachineByColliderID(int key, AIStateMachine stateMachine)
        {
            if(!m_StateMachines.ContainsKey(key))
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

        public float GetAnimatorFloatValue(AIStateMachine stateMachine, string name)
        {
            float val = 0.0f;

            if(stateMachine.animator)
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
    }
}

