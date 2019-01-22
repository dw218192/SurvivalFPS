using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Weapon Config/Damage Data")]
    public class DamageData : ScriptableObject
    {
        [SerializeField] [Range(0, 100)] private int m_DamagePerShot = 30;
        [SerializeField] [Range(0.0f, 100.0f)] private float m_PenetratingPower = 10.0f; //TODO
        [SerializeField] [Range(0.0f, 100.0f)] private float m_ImpactForce = 10.0f;
        [SerializeField] [Range(10, 100)] private int m_ImpactBloodAmount = 60;

        public int damagePerShot { get { return m_DamagePerShot; } }
        public float penetratingPower { get { return m_PenetratingPower; } }
        public float impactForce { get { return m_ImpactForce; } }
        public int impactBloodAmount { get { return m_ImpactBloodAmount; } }
    }
}