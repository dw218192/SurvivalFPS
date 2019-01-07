using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    public abstract class AIZombieState : AIState
    {
        /// <summary>
        /// should be called by the AIStateMachine in the start method
        /// </summary>
        public override void SetStateMachine(AIStateMachine machine)
        {
            if(machine.GetType() == typeof(AIZombieStateMachine))
            {
                base.SetStateMachine(machine);
                m_ZombieStateMachine = (AIZombieStateMachine) machine;
            }
        }

        protected AIZombieStateMachine m_ZombieStateMachine = null;

        /// <summary>
        /// called by the sensor script, when the sensor detects any zombie aggravators
        /// </summary>
        public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
        {
            if(m_ZombieStateMachine)
            {
                if(eventType != AITriggerEventType.Exit)
                {
                    ZombieAggravator zombieAggravator = other.GetComponent<ZombieAggravator>();

                    if(zombieAggravator)
                    {
                        zombieAggravator.TryBecomeThreat(m_ZombieStateMachine);
                    }
                }
            }
        }
    }
}