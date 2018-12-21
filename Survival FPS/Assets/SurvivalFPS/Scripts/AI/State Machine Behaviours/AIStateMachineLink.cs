using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AIStateMachineLink : StateMachineBehaviour
    {
        protected AIStateMachine m_StateMachine = null;
        public AIStateMachine stateMachine { get { return m_StateMachine; } set { m_StateMachine = value; } }
    }
}
