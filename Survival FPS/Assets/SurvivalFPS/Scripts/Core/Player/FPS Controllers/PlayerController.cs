using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurvivalFPS.Core.FPS
{
    public interface IPlayerController
    {
        void StopControl();
        void ResumeControl();
    }

    [RequireComponent(typeof(PlayerManager))]
    public abstract class PlayerController : MonoBehaviour, IPlayerController
    {
        public abstract void ResumeControl();
        public abstract void StopControl();
    }
}


