using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.FPS;

namespace SurvivalFPS.Core.Weapon
{
    public class PlayerStateMachineLink : StateMachineBehaviour
    {
        /*
        private PlayerManager m_PlayerManager;
        public PlayerManager playerManager { set { m_PlayerManager = value; } }
        */

        protected PlayerWeaponController m_playerWeaponController;
        public PlayerWeaponController playerWeaponController { set { m_playerWeaponController = value; } }
    }
}