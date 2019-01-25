using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.FPS;

namespace SurvivalFPS.Core.Weapon
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Weapon Config/Automatic Rifle Config")]
    public class AutomaticRifleConfig : WeaponConfig<AutomaticRifleBehaviour>
    {
        public override void Initialize(PlayerManager player)
        {
            base.Initialize(player);
            m_WeaponBehaviour.weaponConfig = this;
            m_WeaponBehaviour.Initialize();
        }
    }
}
