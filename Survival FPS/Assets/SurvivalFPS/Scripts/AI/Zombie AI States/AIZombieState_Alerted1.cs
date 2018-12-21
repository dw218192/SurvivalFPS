using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Alerted1 : AIZombieState
    {
        [SerializeField] [Range(1.0f,60.0f)] float m_MaxDuration;
        [SerializeField] float m_WaypointAngleThreshold = 10.0f;
        [SerializeField] float m_ThreatAngleThreshold = 10.0f;

        float m_Timer = 0.0f;

        public override AIStateType GetStateType()
        {
            return AIStateType.Alerted;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            if (m_ZombieStateMachine)
            {
                m_ZombieStateMachine.NavAgentControl(true, false);

                m_ZombieStateMachine.speed = 0;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = 0;

                m_Timer = m_MaxDuration;
            }
        }

        public override AIStateType UpdateState()
        {
            m_Timer -= Time.deltaTime;

            if (m_Timer <= 0.0f) return AIStateType.Patrol;

            if(m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }

            if (m_ZombieStateMachine.AudioThreat.type == AITargetType.Audio)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.AudioThreat);
                m_Timer = m_MaxDuration;
            }

            if (m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                m_Timer = m_MaxDuration;
            }

            if (m_ZombieStateMachine.AudioThreat.type == AITargetType.None 
                && m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }

            float angle;
            if(m_ZombieStateMachine.currentTargetType == AITargetType.Audio || m_ZombieStateMachine.currentTargetType == AITargetType.Visual_Light)
            {
                angle = MonobehaviourUtility.SignedAngleBetween(m_ZombieStateMachine.transform.forward, m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition - m_ZombieStateMachine.transform.position);

                if(m_ZombieStateMachine.currentTargetType == AITargetType.Audio && Mathf.Abs(angle) < m_ThreatAngleThreshold)
                {
                    return AIStateType.Pursuit;
                }

                if(Random.value < m_ZombieStateMachine.intelligence)
                {
                    m_ZombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    //randomly turn if the zombie is dumb
                    m_ZombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }
            }
            else if(m_ZombieStateMachine.currentTargetType == AITargetType.Waypoint)
            {
                angle = MonobehaviourUtility.SignedAngleBetween(m_ZombieStateMachine.transform.forward, m_ZombieStateMachine.navAgent.steeringTarget - m_ZombieStateMachine.transform.position);

                if (Mathf.Abs(angle) < m_WaypointAngleThreshold) return AIStateType.Patrol;
                m_ZombieStateMachine.seeking = (int)Mathf.Sign(angle);
            }

            return AIStateType.Alerted;
        }
    }
}

