using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class ZombieFood : ZombieVisualAggravator
    {
        protected override bool AdditionalThreatCondition(AIZombieStateMachine zombie)
        {
            if (zombie.audioThreat) return false;

            if (zombie.satisfaction < 0.5f)
            {
                return true;
            }

            return false;
        }
    }
}