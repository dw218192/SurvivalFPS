using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public abstract class AIState : MonoBehaviour
    {
        public virtual void SetStateMachine(AIStateMachine machine) { m_StateMachine = machine; }

        public virtual void Initialize() { }
        public virtual void OnEnterState()
        {
            Debug.Log(GetType().ToString() + "- enter");
        }
        public virtual void OnExitState() 
        {
            Debug.Log(GetType().ToString() + "- exit"); 
        }

        public virtual void OnAnimatorUpdated()
        {
            if (m_StateMachine.useRootPosition)
            {
                m_StateMachine.navAgent.velocity = m_StateMachine.animator.deltaPosition / Time.deltaTime;
            }

            if (m_StateMachine.useRootRotation)
            {
                m_StateMachine.transform.rotation = m_StateMachine.animator.rootRotation;
            }
            else
            {
                RotateAI();
            }
        }

        /// <summary>
        /// How should the AI be rotated if root rotation is not enabled?
        /// </summary>
        protected virtual void RotateAI() 
        {
            Quaternion newRot;
            if (m_StateMachine.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
            {
                newRot = Quaternion.LookRotation(m_StateMachine.navAgent.desiredVelocity, transform.up);
                m_StateMachine.transform.rotation = Quaternion.RotateTowards(m_StateMachine.transform.rotation, newRot, Time.deltaTime * m_StateMachine.turnSpeed);
            }
        }

        public virtual void OnAnimatorIKUpdated() { }
        public virtual void OnTriggerEvent( AITriggerEventType eventType, Collider other ) { }
        public virtual void OnReachDestination() { }
        public virtual void OnLeaveDestination() { }

        public abstract AIStateType UpdateState();
        public abstract AIStateType GetStateType();

        protected AIStateMachine m_StateMachine;
    }
}