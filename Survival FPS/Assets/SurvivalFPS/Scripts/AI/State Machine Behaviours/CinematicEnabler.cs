using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class CinematicEnabler : AIStateMachineLink
    {
        public bool CinematicEnabledOnEnter;
        public bool CinematicEnabledOnExit;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(m_StateMachine)
            {
                m_StateMachine.cinematicEnabled = CinematicEnabledOnEnter;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StateMachine)
            {
                m_StateMachine.cinematicEnabled = CinematicEnabledOnExit;
            }
        }
    }
}