using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Idle1 : AIZombieState
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
            if(m_ZombieStateMachine)
            {
                m_IdleTime = Random.Range(m_IdleTimeRange.x, m_IdleTimeRange.y);
                m_ZombieStateMachine.NavAgentControl(true, false);

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
            if(m_ZombieStateMachine)
            {
                if(m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                }

                if (m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }

                if (m_ZombieStateMachine.AudioThreat.type == AITargetType.Audio)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }

                if (m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
                {
                    if ((1.0f - m_ZombieStateMachine.satisfaction) > (m_ZombieStateMachine.VisualThreat.distance / m_ZombieStateMachine.sensorRadius))
                    {
                        m_StateMachine.SetTarget(m_StateMachine.VisualThreat);
                        return AIStateType.Pursuit;
                    }
                }

                m_Timer += Time.deltaTime;
                if (m_Timer > m_IdleTime) return AIStateType.Patrol;
            }

            return AIStateType.Idle;
        }
    }

}