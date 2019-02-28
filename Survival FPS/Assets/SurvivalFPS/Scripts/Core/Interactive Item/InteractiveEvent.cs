using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.PlayerInteraction
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Interactive Item Config/Interactive Event")]
    public class InteractiveEvent : InteractiveItemConfig
    {
        public override void Interact(PlayerManager player)
        {
            isInteracting = true;

            Debug.Log("haha");

            isInteracting = false;
        }
    }
}
