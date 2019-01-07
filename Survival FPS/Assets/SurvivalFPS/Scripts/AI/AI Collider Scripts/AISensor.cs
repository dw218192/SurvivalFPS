using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AISensor : MonoBehaviour
    {
        private AIStateMachine m_Owner;
        public AIStateMachine owner { get { return m_Owner; } set { m_Owner = value; } }

        private void OnTriggerEnter(Collider other)
        {
            if (m_Owner)
                m_Owner.OnTriggerEvent(AITriggerEventType.Enter, other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (m_Owner)
                m_Owner.OnTriggerEvent(AITriggerEventType.Stay, other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_Owner)
                m_Owner.OnTriggerEvent(AITriggerEventType.Exit, other);
        }
    }
}
