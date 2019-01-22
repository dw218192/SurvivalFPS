using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Dead_Default : AIZombieState
    {
        public override AIStateType GetStateType()
        {
            return AIStateType.Dead;
        }

        public override AIStateType UpdateState()
        {
            return AIStateType.Dead;
        }
    }
}
