using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Weapon Config/Accuracy Data")]
    public class AccuracyData : ScriptableObject
    {
        [Range(0.0f, 100.0f)] [SerializeField] private float m_BaseAccuracy = 80.0f;
        [Range(0.0f, 100.0f)] [SerializeField] private float m_AccuracyRecoveryRate = 50.0f;
        [Range(0.0f, 100.0f)] [SerializeField] private float m_AccuracyDropPerShot = 20.0f;
        public float baseAccuracy { get { return m_BaseAccuracy; } }
        public float accuracyRecoveryRate { get { return m_AccuracyRecoveryRate; } }
        public float accuracyDropPerShot { get { return m_AccuracyDropPerShot; } }
    }
}