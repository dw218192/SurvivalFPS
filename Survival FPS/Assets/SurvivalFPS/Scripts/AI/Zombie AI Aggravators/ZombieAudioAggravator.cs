using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    [RequireComponent(typeof(SphereCollider))]
    public class ZombieAudioAggravator : ZombieAggravator
    {
        private SphereCollider m_SphereCollider;
        public override Collider aggravatorCollider { get { return m_SphereCollider; } }

        private void Awake()
        {
            m_SphereCollider = GetComponent<SphereCollider>();
        }

        public override void TryBecomeThreat(AIZombieStateMachine zombie)
        {
            if(!zombie.IsTargetRecentlyInvestigated(this))
            {
                if (CanBecomeThreat(zombie))
                {
                    zombie.audioThreat = this;
                }
            }
        }

        protected override bool CanBecomeThreat(AIZombieStateMachine zombie)
        {
            //if the zombie has no active visual threat or this threat is more significant than the current threats
            if (!zombie.audioThreat || m_OverridingList.CanOverride(zombie.audioThreat))
            {
                //if the zombie can hear the audio source
                if (IsAudible(zombie))
                {
                    //other conditions specific to an aggravator
                    if (AdditionalThreatCondition(zombie))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool AdditionalThreatCondition(AIZombieStateMachine zombie)
        {
            return true;
        }

        protected virtual bool IsAudible(AIZombieStateMachine zombie)
        {
            Vector3 soundPos; float soundRadius;
            MonobehaviourUtility.ConvertSphereColliderToWorldSpace(m_SphereCollider, out soundPos, out soundRadius);

            float distToZombie = Vector3.Distance(soundPos, zombie.transform.position);
            float actualZombieSensorRadius = zombie.sensorRadius * zombie.hearing;

            //if the sound can reach the zombie and the zombie's hearing range can reach the sound
            return (distToZombie <= actualZombieSensorRadius + soundRadius);
        }
    }
}
