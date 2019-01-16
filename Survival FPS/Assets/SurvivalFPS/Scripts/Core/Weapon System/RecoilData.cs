using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Weapon Config/Recoil Data")]
    public class RecoilData : ScriptableObject
    {
        [Range(0.0f, 3.0f)] [SerializeField] private float m_KickUpBase = 2.0f; //start value of the up kick angle
        [Range(0.0f, 3.0f)] [SerializeField] private float m_KickLateralBase = 1.0f; //start value of the lateral kick angle
        [Range(5.0f, 90.0f)] [SerializeField] private float m_KickUpMax = 25.0f; //maximum value of the up kick angle
        [Range(5.0f, 35.0f)] [SerializeField] private float m_KickLateralMax = 5.0f; //maximum value of the lateral kick angle
        [Range(0.0f, 30.0f)] [SerializeField] private float m_KickUpModifier = 0.2f; //how significant the up kick is
        [Range(0.0f, 30.0f)] [SerializeField] private float m_KickLateralModifier = 0.4f; //how significant the lateral kick is
        [Range(1, 10)] [SerializeField] private int m_SideDirChange = 7; //how infrequent the gun will change lateral kick direction
        [Range(0.0f, 30.0f)] [SerializeField] private float m_RecoilResetModifier = 1.0f; //how quickly the gun will be stable again
        public float kickUpBase { get { return m_KickUpBase; } }
        public float kickLateralBase { get { return m_KickLateralBase; } }
        public float kickUpMax { get { return m_KickUpMax; } }
        public float kickLateralMax { get { return m_KickLateralMax; } }
        public float kickUpModifier { get { return m_KickUpModifier; } }
        public float kickLateralModifier { get { return m_KickLateralModifier; } }
        public int sideDirChange { get { return m_SideDirChange; } }
        public float recoilResetModifier { get { return m_RecoilResetModifier; } }
    }
}