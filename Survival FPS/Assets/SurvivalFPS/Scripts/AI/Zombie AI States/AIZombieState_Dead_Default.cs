using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.AI
{
    /// <summary>
    /// This state is not meant to be turned into from other states;
    /// it is set by the state machine only after the zombie is dead.
    /// </summary>
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
