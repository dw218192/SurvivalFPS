using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core.Weapon 
{
    
    //TODO: implement exceptions
    public class BulletHoleManager : SingletonBehaviour<BulletHoleManager>
    {
        [SerializeField] private BulletHole m_Prefab;
        [SerializeField] private int m_MaxCapacity = 100;

        private List<BulletHole> m_BulletHoleInstances;
        private int m_CurrentIndex = 0; 

        public BulletHole prefab { get { return m_Prefab; } set { m_Prefab = value; } }

        protected override void Awake()
        {
            base.Awake();
            m_BulletHoleInstances = new List<BulletHole>();
            for (int i = 0; i < m_MaxCapacity; i ++)
            {
                m_BulletHoleInstances.Add(null);
            }
        }

        // Increment the current index - a method is used for this so that every time it's incremented, we also check and make sure the index hasn't yet reached number of bullet holes in the pool
        private void IncrementIndex()
        {
            // Add 1 to the index - because this one really needed to have a comment...
            m_CurrentIndex++;

            // If the index reaches the number of elements in the list, we want to cycle back to the beginning
            if (m_CurrentIndex >= m_BulletHoleInstances.Count)
                m_CurrentIndex = 0;
        }

        // Place the next bullet hole at the specified position and rotation
        public void PlaceBulletHole(Vector3 pos, Vector3 normal, GameObject target)
        {
            // Make sure the current bullet hole still exists
            VerifyBulletHole();

            // Start by clearing the parent.  This prevents problems with the transform inherited from previous parents when the bullet hole GameObject is re-parented
            m_BulletHoleInstances[m_CurrentIndex].transform.parent = null;
            m_BulletHoleInstances[m_CurrentIndex].AttachToParent(pos, normal, target.transform);
            m_BulletHoleInstances[m_CurrentIndex].isActive = true;

            // Now increment our index so the oldest bullet holes will always be the first to be re-used
            IncrementIndex();
        }

        // Verify that the specified bullet hole still exists
        private void VerifyBulletHole()
        {
            // If the bullet hole at the current index has been destroyed, instantiate a new one
            if (m_BulletHoleInstances[m_CurrentIndex] == null)
            {
                m_BulletHoleInstances[m_CurrentIndex] = GameObject.Instantiate(m_Prefab);
                m_BulletHoleInstances[m_CurrentIndex].isActive = false;
            }
        }
    }
}
