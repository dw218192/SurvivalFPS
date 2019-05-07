using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    /// <summary>
    /// anything that can attract the zombie
    /// </summary>
    public abstract class ZombieAggravator : MonoBehaviour
    {
        [SerializeField] protected AggravatorData m_OverridingList;
        public AggravatorData data { get { return m_OverridingList; }  set { m_OverridingList = value; }}
        public abstract Collider aggravatorCollider { get; }

        protected virtual void Awake() 
        {
            
        }
        protected virtual void Start() { }

        /// <summary>
        /// whether the current target of the zombie is this aggravator
        /// </summary>
        /// <param name="zombie"></param>
        /// <returns></returns>
        public bool IsCurrentTarget(AIZombieStateMachine zombie)
        {
            if(zombie.GetCurrentTarget().collider)
            {
                return aggravatorCollider.GetInstanceID() == zombie.GetCurrentTarget().collider.GetInstanceID();
            }

            return false;
        }

        public abstract void TryBecomeThreat(AIZombieStateMachine zombie);
        /// <summary>
        /// returns true if the threat is significant enough to replace the current potential threat
        /// </summary>
        /// <param name="zombie"></param>
        /// <returns></returns>
        protected abstract bool CanBecomeThreat(AIZombieStateMachine zombie);
    }
}