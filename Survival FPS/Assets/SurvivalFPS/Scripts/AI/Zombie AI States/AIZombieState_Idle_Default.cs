using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Idle_Default : AIZombieState
    {
        [SerializeField] Vector2 m_IdleTimeRange = new Vector2(10.0f, 60.0f);

        private float m_IdleTime = 0.0f;
        private float m_Timer = 0.0f;

        public override AIStateType GetStateType()
        {
            return AIStateType.Idle;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            if (m_ZombieStateMachine && !m_ZombieStateMachine.IsDead)
            {
                m_IdleTime = Random.Range(m_IdleTimeRange.x, m_IdleTimeRange.y);
                //m_ZombieStateMachine.NavAgentControl(true, false);

                m_ZombieStateMachine.speed = 0;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = 0;
                m_ZombieStateMachine.ClearTarget();
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
        }

        public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
        {
            base.OnTriggerEvent(eventType, other);
        }

        public override AIStateType UpdateState()
        {
            if (!m_ZombieStateMachine)
            {
                return AIStateType.Idle;
            }

            if (m_ZombieStateMachine.visualThreat)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                return AIStateType.Alerted;
            }

            if (m_ZombieStateMachine.audioThreat)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat);
                return AIStateType.Alerted;
            }
            
            m_Timer += Time.deltaTime;
            if (m_Timer > m_IdleTime)
            {
                return AIStateType.Patrol;
            }

            return AIStateType.Idle;
        }
    }
}


