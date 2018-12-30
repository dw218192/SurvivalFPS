using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Light))]
    public class Flashlight : ZombieVisualAggravator
    {
        [SerializeField] private float m_Range;
        private BoxCollider m_BoxCollider;
        private Light m_light;

        protected override void Awake()
        {
            base.Awake();
            m_BoxCollider = GetComponent<BoxCollider>();
            m_light = GetComponent<Light>();

            m_light.range = m_Range;

            m_BoxCollider.center = new Vector3(0.0f, 0.0f, m_Range / 2.0f);
            Vector3 size = m_BoxCollider.size;
            size.z = m_Range;
            m_BoxCollider.size = size;
        }

        protected override bool AdditionalThreatCondition(AIZombieStateMachine zombie)
        {
            return true;
        }

        protected override bool IsColliderVisible(AIZombieStateMachine zombie, out RaycastHit hitInfo, int layerMask)
        {
            if (zombie)
            {
                Vector3 head = zombie.sensorPosition;

                Vector3 lightTriggerCenter = transform.TransformPoint( m_BoxCollider.center );
                Vector3 lightTriggerEdge1 = transform.TransformPoint( m_BoxCollider.center + (  m_Range / 2.0f) * Vector3.forward * 0.9f );
                Vector3 lightTriggerEdge2 = transform.TransformPoint( m_BoxCollider.center + (- m_Range / 2.0f) * Vector3.forward * 0.9f );

                Vector3 headToLightTriggerCenter = lightTriggerCenter - head;
                Vector3 headToLightTriggerEdge1 = lightTriggerEdge1 - head;
                Vector3 headToLightTriggerEdge2 = lightTriggerEdge2 - head;

                bool IsFlashLightVisible = (IsTheClosestHit(zombie, head, headToLightTriggerEdge1, out hitInfo, layerMask)
                    || IsTheClosestHit(zombie, head, headToLightTriggerCenter, out hitInfo, layerMask)
                    || IsTheClosestHit(zombie, head, headToLightTriggerEdge2, out hitInfo, layerMask));

                Debug.DrawLine(head, head + headToLightTriggerEdge1);
                Debug.DrawLine(head, head + headToLightTriggerEdge2);
                Debug.DrawLine(head, head + headToLightTriggerCenter);

                return IsFlashLightVisible;

            }

            hitInfo = new RaycastHit();
            return false;
        }

        private bool IsTheClosestHit(AIZombieStateMachine zombie, Vector3 rayCastStart, Vector3 rayCastDir, out RaycastHit hitInfo, int layerMask)
        {
            hitInfo = new RaycastHit();

            float angle = Vector3.Angle(rayCastDir, zombie.transform.forward);

            //outside the zombie's FOV
            if (angle > zombie.fov * 0.5f)
            {
                return false;
            }

            RaycastHit[] hits = Physics.RaycastAll(rayCastStart, rayCastDir.normalized, zombie.sensorRadius * zombie.sight, layerMask);

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
                        if (zombie != GameSceneManager.Instance.GetAIStateMachineByColliderID(hit.rigidbody.GetInstanceID()))
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
            return (closestCollider && closestCollider.gameObject == gameObject);
        }
    }
}

