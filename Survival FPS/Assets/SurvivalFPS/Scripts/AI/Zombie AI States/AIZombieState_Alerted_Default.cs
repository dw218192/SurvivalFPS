using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Alerted_Default : AIZombieState
    {
        [SerializeField] [Range(1.0f, 60.0f)] float m_MaxDuration;
        [SerializeField] float m_WaypointAngleThreshold = 10.0f;
        [SerializeField] float m_ThreatAngleThreshold = 10.0f;
        [SerializeField] [Range(0.5f, 1.5f)] float m_DirectionChangeTime;
        [SerializeField] float m_TurnSpeed = 40.0f;

        private float m_Timer = 0.0f;
        private float m_DirectionChangeTimer;

        public override AIStateType GetStateType()
        {
            return AIStateType.Alerted;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            if (m_ZombieStateMachine && !m_ZombieStateMachine.IsDead)
            {
                //m_ZombieStateMachine.NavAgentControl(true, false);

                m_ZombieStateMachine.speed = 0;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = 0;

                m_Timer = m_MaxDuration;
                m_DirectionChangeTimer = 0.0f;
            }
        }

        public override AIStateType UpdateState()
        {
            if (!m_ZombieStateMachine)
            {
                return AIStateType.Alerted;
            }

            m_Timer -= Time.deltaTime;
            m_DirectionChangeTimer += Time.deltaTime;

            if (m_Timer <= 0.0f)
            {
                return AIStateType.Patrol;
            }

            if (m_ZombieStateMachine.visualThreat)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
            }

            else if (m_ZombieStateMachine.audioThreat)
            {
                if (!m_ZombieStateMachine.IsTargetRecentlyInvestigated(m_ZombieStateMachine.audioThreat))
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat);
                    m_Timer = m_MaxDuration;
                }
            }

            //if it's a waypoint
            float angle;
            if (m_ZombieStateMachine.currentTargetType == AITargetType.Waypoint)
            {
                angle = MonobehaviourUtility.SignedAngleBetween(m_ZombieStateMachine.transform.forward, m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition - m_ZombieStateMachine.transform.position);

                if (Mathf.Abs(angle) < m_WaypointAngleThreshold)
                {
                    return AIStateType.Patrol;
                }

                SeekAround((int)Mathf.Sign(angle));
            }
            else
            {
                //if it's either a visual threat or an uninvestigated audio threat, and the threat is not reached
                if (!m_ZombieStateMachine.isTargetReached)
                {
                    //rotate the zombie to face the threat, and go to the pursuit state
                    angle = MonobehaviourUtility.SignedAngleBetween(m_ZombieStateMachine.transform.forward, m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition - m_ZombieStateMachine.transform.position);

                    if (Mathf.Abs(angle) < m_ThreatAngleThreshold)
                    {
                        return AIStateType.Pursuit;
                    }

                    //randomly turn if the zombie is dumb
                    if (Random.value < m_ZombieStateMachine.intelligence)
                    {
                        SeekAround((int)Mathf.Sign(angle));
                    }
                    else
                    {
                        SeekAround((int)Mathf.Sign(Random.Range(-1.0f, 1.0f)));
                    }
                }
                //if the threat is reached and we're in alerted state, meaning the threat has disappeared
                else
                {
                    //look around
                    SeekAround((int)Mathf.Sign(Random.Range(-1.0f, 1.0f)));
                }
            }

            return AIStateType.Alerted;
        }

        private void SeekAround(int direction)
        {
            if(m_DirectionChangeTimer > m_DirectionChangeTime)
            {
                m_ZombieStateMachine.seeking = direction;
                m_DirectionChangeTimer = 0.0f;
            }
        }

        protected override void RotateAI()
        {
            if (!m_ZombieStateMachine.useRootRotation)
            {
                Quaternion newRot;
                if (m_ZombieStateMachine.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
                {
                    if (Random.value < m_ZombieStateMachine.intelligence)
                    {
                        newRot = Quaternion.LookRotation(m_ZombieStateMachine.navAgent.desiredVelocity);
                    }
                    else
                    {
                        newRot = Quaternion.LookRotation(-m_ZombieStateMachine.navAgent.desiredVelocity);
                    }

                    m_ZombieStateMachine.transform.rotation = Quaternion.RotateTowards(m_ZombieStateMachine.transform.rotation, newRot, Time.deltaTime * m_TurnSpeed);
                }
            }
        }
    }
}