using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    /// <summary>
    /// This state is not meant to be turned into from other states;
    /// it is set by the state machine only after the zombie
    /// is in a ragdoll state but not dead
    /// </summary>
    public class AIZombieState_Collapsed_Default : AIZombieState
    {
        [SerializeField] private float m_TimeToReaim = 2.0f;
        private float m_ReanimTimer;
        private bool m_Reanimated;

        public override void OnEnterState()
        {
            base.OnEnterState();
            m_ReanimTimer = 0.0f;
            m_Reanimated = false;
        }

        public override AIStateType GetStateType()
        {
            return AIStateType.Collapsed;
        }

        public override AIStateType UpdateState()
        {
            m_ReanimTimer += Time.deltaTime;

            if (m_ZombieStateMachine.currentHealth > 0 
                && m_ReanimTimer >= m_TimeToReaim 
                && m_ZombieStateMachine.curBoneControlType != AIBoneControlType.RagdollToAnim
                && !m_Reanimated)
            {
                m_ZombieStateMachine.Reanimate();
                m_Reanimated = true;
            }

            return AIStateType.Collapsed;
        }
    }
}
