using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    [CreateAssetMenu(menuName = ("SurvivalFPS/AI/Zombie Aggravator Data"))]
    public class AggravatorData : ScriptableObject
    {
        /// <summary>
        /// a list specifying the other aggravators that are less significant than this one
        /// </summary>
        [SerializeField] private List<AggravatorData> m_OverridingList = new List<AggravatorData>();
    }
}
