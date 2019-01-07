using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    [RequireComponent(typeof(Renderer))]
    public class BulletHole : MonoBehaviour
    {
        private Renderer m_Renderer;
        /*
        public float lifetime = 28.0f;              // The amount of time before the bullet hole disappears entirely
        public float startFadeTime = 10.0f;         // The amount of time before the bullet hole starts to fade
        private float timer;                        // A timer to keep track of how long this bullet has been in existence
        public float fadeRate = 0.001f;             // The rate at which the bullet will fade out         
        private Color targetColor;                  // The color to which the bullet hole wants to change
        */

        public bool isActive { get { return m_Renderer.enabled; } set { m_Renderer.enabled = value; } }

        // Use this for initialization
        void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
        }

        // Make the bullet hole "stick" to the object it hit by parenting it
        public void AttachToParent(Transform parent)
        {
            transform.parent = parent;
        }
    }
}