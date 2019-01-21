using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    [RequireComponent(typeof(Renderer))]
    public class BulletHole : MonoBehaviour
    {
        //clipping prevention
        private Vector3 m_LastBulletHolePos;
        private float m_MinDistance = 10.0f; //if two bullet holes are within this distance
        private float m_HeightOffset = 0.0001f; //higher a bullet hole a little bit to prevent clipping

        private Renderer m_Renderer;

        public bool isActive { get { return m_Renderer.enabled; } set { m_Renderer.enabled = value; } }

        // Use this for initialization
        void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            m_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // Make the bullet hole "stick" to the object it hit by parenting it
        public void AttachToParent(Vector3 pos, Vector3 normal, Transform parent)
        {
            //set the position and rotation of the bullet hole
            pos = pos + normal * m_HeightOffset;
            transform.position = pos;
            transform.rotation = Quaternion.FromToRotation(transform.up, normal);

            transform.parent = parent;
        }
    }
}