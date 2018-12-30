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

        [Header("Animation Parameters")]
        [SerializeField] private string m_RightHandAttackParameterName;
        [SerializeField] private string m_LeftHandAttackParameterName;
        [SerializeField] private string m_MouthAttackParameterName;

        private int m_RightHandAttackParameterNameHash;
        private int m_LeftHandAttackParameterNameHash;
        private int m_MouthAttackParameterNameHash;

        private Dictionary<int, AIStateMachine> m_StateMachines = new Dictionary<int, AIStateMachine>();
        private int m_ZombieBodyPartLayer;
        private int m_PlayerLayer;
        private int m_ObstaclesLayerMask;

        //public properties
        //special effects
        public ParticleSystem bloodParticleSystem { get { return m_BloodParticleSystem; } }
        //layer information
        public int zombieBodyPartLayer { get { return m_ZombieBodyPartLayer; } }
        public int playerLayer { get { return m_PlayerLayer; } }
        public int obstaclesLayerMask { get { return m_ObstaclesLayerMask; } }
        public string rightHandAttackParameterName { get { return m_RightHandAttackParameterName; } }
        public string leftHandAttackParameterName { get { return m_LeftHandAttackParameterName; } }
        public string mouthAttackParameterName { get { return m_MouthAttackParameterName; } }

        protected override void Awake()
        {
            base.Awake();
            m_PlayerLayer = LayerMask.NameToLayer("Player");
            m_ZombieBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
            m_ObstaclesLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator", "Obstacle");

            m_RightHandAttackParameterNameHash = Animator.StringToHash(m_RightHandAttackParameterName);
            m_LeftHandAttackParameterNameHash = Animator.StringToHash(m_LeftHandAttackParameterName);
            m_MouthAttackParameterNameHash =  Animator.StringToHash(m_MouthAttackParameterName);
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
                    val = stateMachine.animator.GetFloat(m_RightHandAttackParameterNameHash);
                }
                else if (name.Equals(m_LeftHandAttackParameterName))
                {
                    val = stateMachine.animator.GetFloat(m_LeftHandAttackParameterNameHash);
                }
                else if (name.Equals(m_MouthAttackParameterName))
                {
                    val = stateMachine.animator.GetFloat(m_MouthAttackParameterNameHash);
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

