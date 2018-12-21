using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.AI;
using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    /// <summary>
    /// a singleton manager that allows for fast lookup of AIs' colliders in the scene
    /// </summary>
    public class GameSceneManager : SingletonBehaviour<GameSceneManager>
    {
        private Dictionary<int, AIStateMachine> m_StateMachines = new Dictionary<int, AIStateMachine>();

        /// <summary>
        /// registers an AI's collider in the scene; the key is the InstanceID of that collider, and the statemachine is its owner (the AI)
        /// </summary>
        /// <param name="key"> the InstanceID of the collider </param>
        /// <param name="stateMachine"> the AI that owns the collider </param>
        public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
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
        public AIStateMachine GetAIStateMachine(int key)
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
    }
}

