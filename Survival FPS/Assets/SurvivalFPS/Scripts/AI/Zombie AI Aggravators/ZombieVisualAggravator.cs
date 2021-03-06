﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    [RequireComponent(typeof(Collider))]
    public class ZombieVisualAggravator : ZombieAggravator
    {
        //can this threat overwrite the target? i.e. the zombie will give up current target to pursue this threat
        [SerializeField] private bool m_CanOverwriteTarget;
        public bool canOverwriteTarget { get { return m_CanOverwriteTarget; } set { m_CanOverwriteTarget = value; } }

        private Collider m_Collider;
        public override Collider aggravatorCollider { get { return m_Collider; } }

        protected override void Awake()
        {
            m_Collider = GetComponent<Collider>();
        }

        public override void TryBecomeThreat(AIZombieStateMachine zombie)
        {
            if (CanBecomeThreat(zombie))
            {
                zombie.visualThreat = this;
            }
        }

        protected override bool CanBecomeThreat(AIZombieStateMachine zombie)
        {
            //if the zombie has no active visual threat or this threat is more significant than the current threats
            if (!zombie.visualThreat || m_OverridingList.CanOverride(zombie.visualThreat))
            {
                //if the this aggravator is visible
                RaycastHit hitInfo;

                if (IsColliderVisible(zombie, out hitInfo, GameSceneManager.Instance.visualRaycastLayerMask))
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

        protected virtual bool IsColliderVisible(AIZombieStateMachine zombie, out RaycastHit hitInfo, int layerMask)
        {
            hitInfo = new RaycastHit();

            if (zombie)
            {
                Vector3 head = zombie.sensorPosition;

                //if our head is inside the trigger of this visual aggravator
                //don't bother raycasting
                if (m_Collider.bounds.Contains(head))
                {
                    return true;
                }

                Vector3 headToThreat = transform.position - head;
                float angle = Vector3.Angle(headToThreat, zombie.transform.forward);

                //outside the zombie's FOV
                if (angle > zombie.fov * 0.5f)
                {
                    return false;
                }

                RaycastHit[] hits = Physics.RaycastAll(head, headToThreat.normalized, zombie.sensorRadius * zombie.sight, layerMask, QueryTriggerInteraction.Collide);

                //find the closest collider that's hit
                float closestColliderDistance = float.MaxValue;
                Collider closestCollider = null;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.distance < closestColliderDistance)
                    {
                        //if it's a zombie's body part
                        if (hit.transform.gameObject.layer == GameApplication.LayerData.aIBodyPartLayer)
                        {
                            //if it's not this zombie's body part
                            if (zombie == GameSceneManager.Instance.GetAIStateMachineByColliderID(hit.collider.GetInstanceID()))
                            {
                                continue;
                            }
                        }

                        closestColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }

                /*
                Debug.DrawRay(head, headToThreat, Color.red);
                if (closestCollider)
                   Debug.Log(closestCollider.name);
                */

                //if the closest collider hit is the target, then the target is visible
                return (closestCollider && closestCollider.gameObject == gameObject);
            }

            return false;
        }
    }
}
