using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Pursuit_Default : AIZombieState
    {
        [SerializeField] [Range(0.0f, 3.0f)] private float m_PursuitSpeed = 1.0f;
        [SerializeField] [Range(80.0f, 300.0f)] private float m_TurnSpeed = 5.0f;

        //this multiplier * distance to target = after how many seconds should we repath 
        [SerializeField] private float m_RepathDistanceMultiplier = 0.035f;
        //minimum repath interval for visual threats
        [SerializeField] private float m_RepathVisualMinDuration = 0.05f;
        //maximum repath interval for visual threats
        [SerializeField] private float m_RepathVisualMaxDuration = 5.0f;
        //minimum repath interval for audio threats
        [SerializeField] private float m_RepathAudioMinDuration = 0.25f;
        //maximum repath interval for audio threats
        [SerializeField] private float m_RepathAudioMaxDuration = 5.0f;
        //maximum time period the zombie can stay in the pursuit
        [SerializeField] private float m_PursuitTime = 40.0f;

        private float m_Timer = 0.0f;
        private float m_RepathTimer = 0.0f;

        public override AIStateType GetStateType()
        {
            return AIStateType.Pursuit;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            if (m_ZombieStateMachine && !m_ZombieStateMachine.IsDead)
            {
                //update the animator
                //m_ZombieStateMachine.NavAgentControl(true, false);
                m_ZombieStateMachine.speed = m_PursuitSpeed;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = 0;

                m_Timer = 0.0f;

                if(m_ZombieStateMachine.navAgent)
                {
                    m_ZombieStateMachine.navAgent.isStopped = false;
                }
            }
        }

        public override AIStateType UpdateState()
        {
            if(!m_ZombieStateMachine)
            {
                return AIStateType.Pursuit;
            }

            m_Timer += Time.deltaTime;
            m_RepathTimer += Time.deltaTime;

            if (m_Timer >= m_PursuitTime)
            {
                return AIStateType.Patrol;
            }

            if (m_ZombieStateMachine.navAgent.pathPending)
            {
                m_ZombieStateMachine.speed = 0;
                return AIStateType.Pursuit;
            }
            else
            {
                m_ZombieStateMachine.speed = m_PursuitSpeed;
            }

            //if there is something wrong with the path or we do not have an active path now, move back to alerted state
            if (m_ZombieStateMachine.navAgent.isPathStale || m_ZombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                return AIStateType.Alerted;
            }

            //if the rotation is not handled by the animator, slowly rotate the zombie to face the threat
            if (!m_ZombieStateMachine.useRootRotation)
            {
                Quaternion newRot;
                if (m_ZombieStateMachine.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
                {
                    newRot = Quaternion.LookRotation(m_ZombieStateMachine.navAgent.desiredVelocity);
                    m_ZombieStateMachine.transform.rotation = Quaternion.RotateTowards(m_ZombieStateMachine.transform.rotation, newRot, Time.deltaTime * m_TurnSpeed);
                }
            }

            if (!m_ZombieStateMachine.isTargetReached)
            {
                if (m_ZombieStateMachine.audioThreat == null && m_ZombieStateMachine.visualThreat == null)
                {
                    if (m_ZombieStateMachine.hasTarget)
                    {
                        return AIStateType.Pursuit;
                    }
                    else
                    {
                        return AIStateType.Patrol;
                    }
                }

                if (m_ZombieStateMachine.visualThreat != null)
                {
                    if (m_ZombieStateMachine.visualThreat.IsCurrentTarget(m_ZombieStateMachine))
                    {
                        //if the position of the visual threat has changed
                        if (m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition != m_ZombieStateMachine.visualThreat.transform.position)
                        {
                            float distanceToThreat = Vector3.Distance(m_ZombieStateMachine.visualThreat.transform.position, m_ZombieStateMachine.transform.position);

                            //if it's time to repath
                            if (Mathf.Clamp(distanceToThreat * m_RepathDistanceMultiplier, m_RepathVisualMinDuration, m_RepathVisualMaxDuration) < m_RepathTimer)
                            {
                                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                                m_RepathTimer = 0.0f;
                            }
                        }

                        return AIStateType.Pursuit;
                    }
                    else
                    {
                        if (!m_ZombieStateMachine.hasTarget || m_ZombieStateMachine.hasTarget && m_ZombieStateMachine.visualThreat.canOverwriteTarget)
                        {
                            m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                            return AIStateType.Pursuit;
                        }
                        else
                        {
                            return AIStateType.Pursuit;
                        }
                    }
                }

                if (m_ZombieStateMachine.audioThreat != null)
                {
                    if (m_ZombieStateMachine.audioThreat.IsCurrentTarget(m_ZombieStateMachine))
                    {
                        //if the position of the audio threat has changed
                        if (m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition != m_ZombieStateMachine.audioThreat.transform.position)
                        {
                            float distanceToThreat = Vector3.Distance(m_ZombieStateMachine.audioThreat.transform.position, m_ZombieStateMachine.transform.position);

                            //if it's time to repath
                            if (Mathf.Clamp(distanceToThreat * m_RepathDistanceMultiplier, m_RepathAudioMinDuration, m_RepathAudioMaxDuration) < m_RepathTimer)
                            {
                                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat);
                                m_RepathTimer = 0.0f;
                            }
                        }

                        return AIStateType.Pursuit;
                    }
                    else
                    {
                        m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat);
                        return AIStateType.Pursuit;
                    }
                }
            }
            else
            {
                if (m_ZombieStateMachine.visualThreat == null && m_ZombieStateMachine.audioThreat == null)
                {
                    if (m_ZombieStateMachine.hasTarget)
                    {
                        m_ZombieStateMachine.ClearTarget();
                        return AIStateType.Alerted;
                    }
                }

                if (m_ZombieStateMachine.visualThreat != null)
                {
                    if (m_ZombieStateMachine.visualThreat.IsCurrentTarget(m_ZombieStateMachine))
                    {
                        //if it's food, go to the feeding state
                        if (m_ZombieStateMachine.visualThreat.GetType() == typeof(ZombieFood))
                        {
                            return AIStateType.Feeding;
                        }
                        //if it's food, go to the feeding state
                        else if (m_ZombieStateMachine.visualThreat.GetType() == typeof(Flashlight))
                        {
                            m_ZombieStateMachine.ClearTarget();
                            return AIStateType.Alerted;
                        }

                        else
                        {
                            if (m_ZombieStateMachine.isInMeeleRange)
                            {
                                return AIStateType.Attack;
                            }
                            else
                            {
                                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                                return AIStateType.Pursuit;
                            }
                        }
                    }
                    else
                    {
                        m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                        return AIStateType.Pursuit;
                    }
                }

                if (m_ZombieStateMachine.audioThreat != null)
                {
                    if (m_ZombieStateMachine.audioThreat.IsCurrentTarget(m_ZombieStateMachine))
                    {
                        m_ZombieStateMachine.AddInvestigatedTarget(m_ZombieStateMachine.audioThreat);
                        //handle specific audio threat cases
                        return AIStateType.Alerted;
                    }
                    else
                    {
                        m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat);
                        return AIStateType.Pursuit;
                    }
                }
            }

            return AIStateType.Pursuit;
        }
    }
}
