using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.Weapon;

namespace SurvivalFPS.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(CharacterJoint))]
    public class AIBodyPart : MonoBehaviour, IWeaponDamageable
    {
        [SerializeField] AIBodyPartType m_Type;

        private Collider m_Collider;
        private AIStateMachine m_Owner;
        private Rigidbody m_RigidBody;

        public Collider bodyPartCollider { get { return m_Collider; } }
        public Rigidbody rigidBody { get { return m_RigidBody; } }
        public AIStateMachine owner { get { return m_Owner; } set { m_Owner = value; } }

        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            gameObject.layer = GameSceneManager.Instance.zombieBodyPartLayer;
        }

        public void TakeDamage(WeaponConfig weaponUsed, Vector3 hitDirection)
        {


            m_RigidBody.AddForce(hitDirection.normalized * 3.0f, ForceMode.Impulse);
            m_Owner.currentHealth -= 34;
            PlayBloodSpecialEffect();

            bool shouldRagRoll = (m_Owner.currentHealth <= 0);

            if(shouldRagRoll)
            {
                m_Owner.OnDeath();
            }
        }

        private void PlayBloodSpecialEffect()
        {
            if (GameSceneManager.Instance)
            {
                ParticleSystem bloodEffect = GameSceneManager.Instance.bloodParticleSystem;
                bloodEffect.transform.position = transform.position;
                var mainModule = bloodEffect.main;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
                bloodEffect.Emit(60);
            }
        }
    }

}