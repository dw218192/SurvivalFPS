using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Weapon Config/Semi-automatic Rifle Config")]
    public class SemiAutoRifleConfig : WeaponConfig<SemiAutoRifleBehaviour>
    {
        public override void Initialize(PlayerManager player)
        {
            base.Initialize(player);
            m_WeaponBehaviour.weaponConfig = this;
            m_WeaponBehaviour.Initialize();
        }
    }
}
