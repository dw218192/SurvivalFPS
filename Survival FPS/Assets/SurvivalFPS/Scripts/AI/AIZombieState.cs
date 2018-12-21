using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    public abstract class AIZombieState : AIState
    {
        /// <summary>
        /// should be called by the AIStateMachine in the start method
        /// </summary>
        public override void SetStateMachine(AIStateMachine machine)
        {
            if(machine.GetType() == typeof(AIZombieStateMachine))
            {
                base.SetStateMachine(machine);
                m_ZombieStateMachine = (AIZombieStateMachine) machine;
            }
        }

        protected AIZombieStateMachine m_ZombieStateMachine = null;
        protected int m_EntityLayerMask = -1;
        protected int m_BodyPartLayer = -1;
        protected int m_VisualAggrLayerMask = -1;

        private void Awake()
        {
            m_EntityLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1; //default, Player, AI Body Part layers
            m_VisualAggrLayerMask = LayerMask.GetMask("Visual Aggravators");
            m_BodyPartLayer = LayerMask.NameToLayer("AI Body Part");
        }

        /// <summary>
        /// called by the sensor script, when the sensor detects any zombie aggravators
        /// </summary>
        public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
        {
            if(m_ZombieStateMachine)
            {
                if(eventType != AITriggerEventType.Exit)
                {
                    AITargetType curType = m_ZombieStateMachine.VisualThreat.type;

                    //a player is detected
                    if(other.CompareTag("Player"))
                    {
                        float distanceToThreat = Vector3.Distance(m_ZombieStateMachine.sensorPosition, other.transform.position);

                        //player is of the highest priority, override other threats if necessary
                        if(curType != AITargetType.Visual_Player || (curType == AITargetType.Visual_Player && distanceToThreat < m_ZombieStateMachine.VisualThreat.distance))
                        {
                            RaycastHit hitInfo;
                            //if the player is in the line of sight
                            if(IsColliderVisible( other, out hitInfo, m_EntityLayerMask))
                            {
                                m_ZombieStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distanceToThreat);
                            }
                        }
                    }
                    else if(other.CompareTag("Flash Light") && curType != AITargetType.Visual_Player)
                    {
                        BoxCollider flashLightTrigger = (BoxCollider)other;

                        Vector3 flashLightPos, flashLightBoxSize;
                        MonobehaviourUtility.ConvertBoxColliderToWorldSpace(flashLightTrigger, out flashLightPos, out flashLightBoxSize);
                        float flashLightRange = flashLightBoxSize.z;

                        //distance between the zombie and the tip of the flashlight range
                        float distanceToThreat = Vector3.Distance(m_ZombieStateMachine.sensorPosition, flashLightPos);
                        float distanceFactor = distanceToThreat / flashLightRange;
                        distanceFactor += distanceFactor * (1.0f - m_ZombieStateMachine.sight);
                            
                        if (distanceFactor <= 1.0f)
                        {
                            if (distanceToThreat < m_ZombieStateMachine.VisualThreat.distance)
                            {
                                m_ZombieStateMachine.VisualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                            }
                        }
                    }
                    else if(other.CompareTag("AI Sound Emitter") && curType != AITargetType.Visual_Player)
                    {
                        SphereCollider soundTrigger = (SphereCollider)other;
                        if(soundTrigger)
                        {
                            Vector3 sensorPos = m_ZombieStateMachine.sensorPosition;
                            Vector3 soundPos; float soundRadius;
                            MonobehaviourUtility.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);

                            float distanceToThreat = Vector3.Distance(soundPos, sensorPos);
                            float distanceFactor = distanceToThreat / soundRadius;
                            distanceFactor += distanceFactor * (1.0f - m_ZombieStateMachine.hearing);
                            
                            if(distanceFactor <= 1.0f)
                            {
                                if (distanceToThreat < m_ZombieStateMachine.AudioThreat.distance)
                                {
                                    m_ZombieStateMachine.AudioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);
                                }
                            }
                        }
                    }
                    else if(other.CompareTag("AI Food") 
                        && curType != AITargetType.Visual_Player 
                        && curType != AITargetType.Visual_Light
                        && m_ZombieStateMachine.AudioThreat.type != AITargetType.None 
                        && m_ZombieStateMachine.satisfaction <= 0.9f)
                    {
                        float distanceToThreat = Vector3.Distance(m_ZombieStateMachine.sensorPosition, other.transform.position);

                        if(distanceToThreat < m_ZombieStateMachine.VisualThreat.distance)
                        {
                            RaycastHit hitInfo;
                            if(IsColliderVisible(other, out hitInfo, m_VisualAggrLayerMask))
                            {
                                m_ZombieStateMachine.VisualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToThreat);
                            }
                        }
                    }
                    //other types of aggravators to be added here
                }
            }
        }

        protected virtual bool IsColliderVisible(Collider other, out RaycastHit hitInfo, int playerLayerMask)
        {
            hitInfo = new RaycastHit();

            if (m_ZombieStateMachine)
            {
                Vector3 head = m_ZombieStateMachine.sensorPosition;
                Vector3 headToOther = other.transform.position - head;
                float angle = Vector3.Angle(headToOther, transform.forward);

                //outside the zombie's FOV
                if(angle > m_ZombieStateMachine.fov * 0.5f)
                {
                    return false;
                }

                RaycastHit[] hits = Physics.RaycastAll(head, headToOther.normalized, m_ZombieStateMachine.sensorRadius * m_ZombieStateMachine.sight, playerLayerMask);

                //find the closest collider that's hit
                float closestColliderDistance = float.MaxValue;
                Collider closestCollider = null;
                foreach (RaycastHit hit in hits)
                {
                    if(hit.distance < closestColliderDistance)
                    {
                        //if it's the zombie's body part
                        if (hit.transform.gameObject.layer == m_BodyPartLayer)
                        {
                            //if it's not our body part
                            if (m_ZombieStateMachine != GameSceneManager.Instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                            {
                                closestColliderDistance = hit.distance;
                                closestCollider = hit.collider;
                                hitInfo = hit;
                            }
                        }
                        else
                        {
                            closestColliderDistance = hit.distance;
                            closestCollider = hit.collider;
                            hitInfo = hit;
                        }
                    }
                }

                //if the closest collider hit is the target, then the target is visible
                return (closestCollider && closestCollider.gameObject == other.gameObject);
            }

            return false;
        }
    }
}