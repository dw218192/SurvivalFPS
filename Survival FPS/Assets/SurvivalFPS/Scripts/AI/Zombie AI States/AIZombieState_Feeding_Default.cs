using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Feeding_Default : AIZombieState
    {
        [SerializeField] [Range(80.0f, 300.0f)] private float m_TurnSpeed = 5.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_BloodParticleBurstTime = 0.1f;
        [SerializeField] [Range(1, 100)] private int m_BloodParticleBurstAmount = 10;

        private int m_EatingAnimStateHash = Animator.StringToHash("Feeding_state");
        private int m_AnimCinematicLayer = -1;

        private Transform m_BloodParticleMount;
        private float m_BloodParticleBurstTimer;

        public override void Initialize()
        {
            base.Initialize();
            m_BloodParticleMount = m_ZombieStateMachine.bloodParticleMount;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            if(m_ZombieStateMachine)
            {
                if (m_AnimCinematicLayer == -1)
                {
                    m_AnimCinematicLayer = m_ZombieStateMachine.animator.GetLayerIndex("Cinematic");
                }

                m_ZombieStateMachine.speed = 0;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.attackType = 0;

                m_BloodParticleBurstTimer = 0.0f;
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
            if (m_ZombieStateMachine)
            {
                m_ZombieStateMachine.feeding = false;
            }
        }

        public override AIStateType GetStateType()
        {
            return AIStateType.Feeding;
        }

        public override AIStateType UpdateState()
        {
            m_BloodParticleBurstTimer += Time.deltaTime;

            if (m_ZombieStateMachine.isTargetReached)
            {
                m_ZombieStateMachine.feeding = true;
            }
            else
            {
                m_ZombieStateMachine.feeding = false;
                return AIStateType.Patrol;
            }

            if (m_ZombieStateMachine.satisfaction > 0.9f)
            {
                return AIStateType.Patrol;
            }

            if (m_ZombieStateMachine.visualThreat)
            {
                if (!m_ZombieStateMachine.visualThreat.IsCurrentTarget(m_ZombieStateMachine))
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                    return AIStateType.Alerted;
                }
            }

            else if (m_ZombieStateMachine.audioThreat)
            {
                if (!m_ZombieStateMachine.IsTargetRecentlyInvestigated(m_ZombieStateMachine.audioThreat))
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat);
                    return AIStateType.Alerted;
                }
            }

            if(m_ZombieStateMachine.animator.GetCurrentAnimatorStateInfo(m_AnimCinematicLayer).shortNameHash == m_EatingAnimStateHash)
            {
                m_ZombieStateMachine.satisfaction = Mathf.Min(1.0f, m_ZombieStateMachine.satisfaction + Time.deltaTime * m_ZombieStateMachine.replenishRate);
                
                if(m_BloodParticleBurstTimer > m_BloodParticleBurstTime)
                {
                    //blood effect
                    ParticleSystem bloodParticles = GameSceneManager.Instance.bloodParticleSystem;

                    if (bloodParticles && m_BloodParticleMount)
                    {
                        bloodParticles.transform.position = m_BloodParticleMount.transform.position;
                        bloodParticles.transform.rotation = m_BloodParticleMount.transform.rotation;
                        var mainModule = bloodParticles.main;
                        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
                        bloodParticles.Emit(m_BloodParticleBurstAmount);

                        m_BloodParticleBurstTimer = 0.0f;
                    }
                }

            }

            if(!m_ZombieStateMachine.useRootRotation)
            {
                if (m_ZombieStateMachine.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
                {
                    Vector3 targetPos = m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition;
                    targetPos.y = m_ZombieStateMachine.transform.position.y;
                    Quaternion newRot = Quaternion.LookRotation(m_ZombieStateMachine.navAgent.desiredVelocity);
                    m_ZombieStateMachine.transform.rotation = Quaternion.RotateTowards(m_ZombieStateMachine.transform.rotation, newRot, Time.deltaTime * m_TurnSpeed);
                }
            }

            return AIStateType.Feeding;
        }
    }
}
