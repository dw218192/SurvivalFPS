using System.Collections;
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
        public bool canOverwriteTarget { get { return m_CanOverwriteTarget; } }

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

                if (IsColliderVisible(zombie, out hitInfo, GameSceneManager.Instance.obstaclesLayerMask))
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

                Vector3 headToThreat = transform.position - head;
                float angle = Vector3.Angle(headToThreat, zombie.transform.forward);

                //outside the zombie's FOV
                if (angle > zombie.fov * 0.5f)
                {
                    return false;
                }

                RaycastHit[] hits = Physics.RaycastAll(head, headToThreat.normalized, zombie.sensorRadius * zombie.sight, layerMask);

                //find the closest collider that's hit
                float closestColliderDistance = float.MaxValue;
                Collider closestCollider = null;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.distance < closestColliderDistance)
                    {
                        //if it's the zombie's body part
                        if (hit.transform.gameObject.layer == GameSceneManager.Instance.zombieBodyPartLayer)
                        {
                            //if it's not our body part
                            if (zombie != GameSceneManager.Instance.GetAIStateMachineByColliderID(hit.collider.GetInstanceID()))
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


                //if (closestCollider)
                //   Debug.Log(closestCollider.name);

                //if the closest collider hit is the target, then the target is visible
                return (closestCollider && closestCollider.gameObject == gameObject);
            }

            return false;
        }
    }
}
