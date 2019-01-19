using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    [RequireComponent(typeof(Collider))]
    public class AIDamageTrigger : MonoBehaviour
    {
        [SerializeField] private int m_BloodParticleBurstAmount = 10;

        private AIStateMachine m_Owner;
        private string m_AnimatorDamageParameter;

        public AIStateMachine owner { get { return m_Owner; } set { m_Owner = value; } }
        public string animatorDamageParameter { get { return m_AnimatorDamageParameter; } set { m_AnimatorDamageParameter = value; } }

        private void OnTriggerStay(Collider other)
        {
            //if (other.gameObject.layer == GameSceneManager.Instance.playerLayer)

            IAIDamageable aIDamageable = other.gameObject.GetComponent(typeof(IAIDamageable)) as IAIDamageable;
            if(aIDamageable != null)
            {
                if(m_Owner)
                {
                    float damageAmount = GameSceneManager.Instance.GetAnimatorFloatValue(m_Owner, animatorDamageParameter);
                    if(damageAmount > 0.0f)
                    {
                        PlayBloodParticalEffect();
                        aIDamageable.TakeDamage(m_Owner.damagePerSec);
                    }
                }
            }
        }

        private void PlayBloodParticalEffect()
        {
            ParticleSystem bloodEffect = GameSceneManager.Instance.bloodParticleSystem;
            if (bloodEffect)
            {
                bloodEffect.transform.position = transform.position;
                bloodEffect.transform.rotation = transform.rotation;

                var mainModule = bloodEffect.main;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

                bloodEffect.Emit(m_BloodParticleBurstAmount);
            }
        }
    }

}
