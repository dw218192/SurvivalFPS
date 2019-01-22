using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Attack_Default : AIZombieState
    {
        [SerializeField] [Range(0.0f, 2.0f)] float m_AttackMotionSpeed = 0.0f;
        [SerializeField] [Range(100.0f, 300.0f)] float m_TurnSpeed = 200.0f;

        [SerializeField] [Range(0.0f, 10.0f)] float m_LookAtWeight = 0.7f;
        [SerializeField] [Range(0.0f, 90.0f)] float m_LookAtWeightChangeRate = 50.0f;
        [SerializeField] [Range(0.0f, 90.0f)] float m_LookAtThreshold = 15.0f;

        private float m_CurrentLookAtWeight = 0.0f;
        [SerializeField] [Range(0.0f, 2.0f)] private float m_StoppingDistance = 1.0f;

        public override void OnEnterState()
        {
            base.OnEnterState();

            if(m_ZombieStateMachine && !m_ZombieStateMachine.IsDead)
            {
                //animator
                //m_ZombieStateMachine.NavAgentControl(true, false);
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = Random.Range(1, 101);

                //look at
                m_CurrentLookAtWeight = 0.0f;

                m_ZombieStateMachine.speed = 0.0f;
            }
        }

        public override AIStateType GetStateType()
        {
            return AIStateType.Attack;
        }

        public override AIStateType UpdateState()
        {
            if(!m_ZombieStateMachine || m_ZombieStateMachine.IsDead)
            {
                return AIStateType.Dead;
            }

            if (Vector3.Distance(m_ZombieStateMachine.transform.position, m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition) > m_StoppingDistance)
            {
                m_ZombieStateMachine.speed = m_AttackMotionSpeed;
            }
            else
            {
                m_ZombieStateMachine.speed = 0;
            }

            Vector3 targetPos;
            Quaternion newRot;

            //quickly rotate to face our target
            if (!m_ZombieStateMachine.useRootRotation)
            {
                targetPos = m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition;
                targetPos.y = m_ZombieStateMachine.transform.position.y;
                newRot = Quaternion.LookRotation(targetPos - m_ZombieStateMachine.transform.position);

                m_ZombieStateMachine.transform.rotation = Quaternion.RotateTowards(m_ZombieStateMachine.transform.rotation, newRot, m_TurnSpeed * Time.deltaTime);
            }

            //if we are in melee range
            if (m_ZombieStateMachine.isInMeeleRange)
            {
                //if there is no visual threat present; it is somehow out of our visual cone
                if (m_ZombieStateMachine.visualThreat == null)
                {
                    //if we still have a target set
                    if (m_ZombieStateMachine.hasTarget)
                    {
                        return AIStateType.Pursuit;
                    }
                    //or we've totally lost them
                    else
                    {
                        return AIStateType.Alerted;
                    }
                }

                //we have a visual threat
                else
                {
                    //if it is the same as our target
                    if (m_ZombieStateMachine.visualThreat.IsCurrentTarget(m_ZombieStateMachine))
                    {
                        //if its position has been changed, repath
                        if (m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition != m_ZombieStateMachine.visualThreat.transform.position)
                        {
                            m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                            return AIStateType.Attack;
                        }
                    }
                    else
                    {
                        //if it no longer the same target
                        //if it has the power of overwriting our current target, or we don't currently have a target at all
                        if (!m_ZombieStateMachine.hasTarget || m_ZombieStateMachine.hasTarget && m_ZombieStateMachine.visualThreat.canOverwriteTarget)
                        {
                            //set it to our target
                            m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                            return AIStateType.Pursuit;
                        }
                    }

                    //otherwise just attack
                    m_ZombieStateMachine.attackType = Random.Range(1, 101);
                    return AIStateType.Attack;
                }
            }

            //the target has escaped our melee range
            else
            {
                return AIStateType.Pursuit;
            }
        }

        public override void OnAnimatorIKUpdated()
        {
            base.OnAnimatorIKUpdated();

            if(m_ZombieStateMachine)
            {
                if(Vector3.Angle(m_ZombieStateMachine.transform.forward, m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition - m_ZombieStateMachine.transform.position) < m_LookAtThreshold)
                {
                    m_ZombieStateMachine.animator.SetLookAtPosition(m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition + Vector3.up);
                    m_CurrentLookAtWeight = Mathf.MoveTowards(m_CurrentLookAtWeight, m_LookAtWeight, Time.deltaTime * m_LookAtWeightChangeRate);
                }
                else
                {
                    m_CurrentLookAtWeight = Mathf.MoveTowards(m_CurrentLookAtWeight, 0.0f, Time.deltaTime * m_LookAtWeightChangeRate);
                }
                m_ZombieStateMachine.animator.SetLookAtWeight(m_CurrentLookAtWeight);
            }
        }
    }

}
