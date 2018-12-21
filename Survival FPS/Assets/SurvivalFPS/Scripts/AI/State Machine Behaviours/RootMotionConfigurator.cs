﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class RootMotionConfigurator : AIStateMachineLink
    {
        [SerializeField] private int m_RootPosition = 0;
        [SerializeField] private int m_RootRotation = 0;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StateMachine)
            {
                m_StateMachine.AddRootMotionRequest(m_RootPosition, m_RootRotation);
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StateMachine)
            {
                m_StateMachine.AddRootMotionRequest(-m_RootPosition, -m_RootRotation);
            }
        }
    }
}
