using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.Weapon;

namespace SurvivalFPS.AI
{
    /// <summary>
    /// A component that defines the body part of an AI;
    /// by default, it is capable of receiving damages from weapons
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class AIBodyPart : MonoBehaviour, IWeaponDamageable
    {
        [SerializeField] AIBodyPartType m_Type;

        private Collider m_Collider;
        private AIStateMachine m_Owner;
        private Rigidbody m_RigidBody;

        public Collider bodyPartCollider { get { return m_Collider; } }
        public Rigidbody rigidBody { get { return m_RigidBody; } }
        public AIStateMachine owner { get { return m_Owner; } set { m_Owner = value; } }

        public Vector3 positionEndOfRagdoll { get { return m_PositionEndOfRagdoll; } set { m_PositionEndOfRagdoll = value; } }
        public Quaternion rotationEndOfRagdoll { get { return m_RotationEndOfRagdoll; } set { m_RotationEndOfRagdoll = value; } }
        public Quaternion localRotationEndOfRagdoll { get { return m_LocalRotationEndOfRagdoll; } set { m_LocalRotationEndOfRagdoll = value; } }

        //ragdoll reanimation info
        private Vector3 m_PositionEndOfRagdoll;
        private Quaternion m_RotationEndOfRagdoll;
        private Quaternion m_LocalRotationEndOfRagdoll;


        /// <summary>
        /// Gets the damage multiplier. (multiplier >= 0.0)
        /// </summary>
        /// <returns>The damage multiplier.</returns>
        private float GetDamageMultiplier()
        {
            switch(m_Type)
            {
                case AIBodyPartType.Head: return 2.0f;
                case AIBodyPartType.UpperBody: return 1.0f;//0.09f;
                case AIBodyPartType.UpperBodyLimb:
                case AIBodyPartType.LowerBodyLimb: return 0.5f;
                case AIBodyPartType.LowerBody: return 0.5f;
            }

            return 0.0f;
        }

        /// <summary>
        /// if it's a zombie,
        /// (does not set health) sets the owner's lower/upper body part damage variables
        /// </summary>
        private void SetBodyPartDamage(int damage)
        {
            AIZombieStateMachine zombieStateMachine = m_Owner as AIZombieStateMachine;

            if(zombieStateMachine)
            {
                switch (m_Type)
                {
                    case AIBodyPartType.UpperBody:
                    case AIBodyPartType.UpperBodyLimb:
                        {
                            zombieStateMachine.upperBodyDamage += damage;
                            return;
                        }
                    case AIBodyPartType.LowerBodyLimb:
                    case AIBodyPartType.LowerBody:
                        {
                            zombieStateMachine.lowerBodyDamage += damage;
                            return;
                        }
                }
            }
        }

        /// <summary>
        /// if it's a zombie
        /// sets the owner's hit type according to the direction from which it received damage
        /// sets the weight of the hit animation layer according to the weapon's power
        /// updates the zombie's animator with these values
        /// </summary>
        private void SetHitAnim(float angle, float weight)
        {
            AIZombieStateMachine zombieStateMachine = m_Owner as AIZombieStateMachine;

            if (zombieStateMachine)
            {
                int hitType = 0;

                switch (m_Type)
                {
                    case AIBodyPartType.Head:
                        {
                            if (angle < -10) hitType = 1; //right head hit
                            else if (angle > 10) hitType = 3; //left head hit
                            else hitType = 2; //front head hit
                            break;
                        }
                    case AIBodyPartType.UpperBody:
                    case AIBodyPartType.UpperBodyLimb:
                        {
                            if (angle < -20) hitType = 4; //right upper body hit
                            else if (angle > 20) hitType = 6; //left upper body hit
                            else hitType = 5; //front upper body hit
                            break;
                        }
                    case AIBodyPartType.LowerBodyLimb:
                    case AIBodyPartType.LowerBody:
                        {
                            //no animation available
                            break;
                        }
                }

                if(hitType != 0)
                {
                    zombieStateMachine.hitLayerWeight = weight;
                    zombieStateMachine.hitType = hitType;
                    zombieStateMachine.UpdateAnimatorHit();
                }
            }
        }

        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            gameObject.layer = GameSceneManager.Instance.zombieBodyPartLayer;
        }

        public void TakeDamage(WeaponConfig weaponUsed, Vector3 hitPosition, Vector3 hitDirection, GameObject instigator)
        {
            DamageData damageData = weaponUsed.damageSetting;

            //reduce health
            int actualDamage = (int) (damageData.damagePerShot * GetDamageMultiplier());
            m_Owner.currentHealth -= actualDamage;
            SetBodyPartDamage(actualDamage);

            //hit effects (force, blood)
            PlayBloodSpecialEffect(damageData.impactBloodAmount);
            if (damageData.impactForce > 0.0f)
            {
                m_RigidBody.AddForce(hitDirection.normalized * damageData.impactForce, ForceMode.Impulse);
            }

            //die if the health falls below zero
            if (m_Owner.currentHealth <= 0)
            {
                m_Owner.Die();
            }

            //if it's a head shot
            if(m_Type == AIBodyPartType.Head)
            {
                m_Owner.RagDoll();
                return;
            }

            //play hit animations if it's other body parts
            if (m_Owner is AIZombieStateMachine)
            {
                //the attacker's relative position
                Vector3 attackerLocalPos = transform.InverseTransformPoint(instigator.transform.position);
                Vector3 hitLocalPos = transform.InverseTransformPoint(hitPosition);

                //if the attacker is in front of the zombie
                if(attackerLocalPos.z > 0)
                {
                    Vector3 attackerToHit = (hitPosition - instigator.transform.position).normalized;
                    float angle = Vector3.SignedAngle(attackerToHit, m_Owner.transform.forward, Vector3.up);
                    SetHitAnim(angle, damageData.impactForce/100.0f);
                }
            }
        }

        private void PlayBloodSpecialEffect(int amount)
        {
            if (GameSceneManager.Instance)
            {
                ParticleSystem bloodEffect = GameSceneManager.Instance.bloodParticleSystem;
                bloodEffect.transform.position = transform.position;
                var mainModule = bloodEffect.main;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
                bloodEffect.Emit(amount);
            }
        }
    }

}