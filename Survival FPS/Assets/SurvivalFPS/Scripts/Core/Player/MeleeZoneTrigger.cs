using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.AI;

namespace SurvivalFPS.Core
{
    /// <summary>
    /// a component that notifies the zombies detected
    /// </summary>
    public class MeleeZoneTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            AIStateMachine zombie = GameSceneManager.Instance.GetAIStateMachineByColliderID(other.GetInstanceID());

            if (zombie)
            {
                zombie.isInMeeleRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            AIStateMachine zombie = GameSceneManager.Instance.GetAIStateMachineByColliderID(other.GetInstanceID());

            if (zombie)
            {
                zombie.isInMeeleRange = false;
            }
        }
    }
}
