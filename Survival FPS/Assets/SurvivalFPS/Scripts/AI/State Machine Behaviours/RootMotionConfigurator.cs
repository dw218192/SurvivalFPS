using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class RootMotionConfigurator : AIStateMachineLink
    {
        [SerializeField] private bool m_UseRootPosition = false;
        [SerializeField] private bool m_UseRootRotation = false;

        private int m_RootPosition = 0;
        private int m_RootRotation = 0;

        private bool m_Incremented = false;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StateMachine)
            {
                int value = IntPowOfTwo(layerIndex);
                if (m_UseRootPosition)
                {
                    m_RootPosition = value;
                }
                else
                {
                    m_RootPosition = - value;
                }

                if (m_UseRootRotation)
                {
                    m_RootRotation = value;
                }
                else
                {
                    m_RootRotation = - value;
                }

                m_StateMachine.AddRootMotionRequest(m_RootPosition, m_RootRotation);
                m_Incremented = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StateMachine && m_Incremented)
            {
                m_StateMachine.AddRootMotionRequest(-m_RootPosition, -m_RootRotation);
            }
        }

        private int IntPowOfTwo(int power)
        {
            return 1 << power;
        }
    }
}
