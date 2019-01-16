using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    public class AutomaticRifleBehaviour : WeaponBehaviour<AutomaticRifleConfig>
    {
        /// <summary>
        /// called when the weapon controller initializes the weapons
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void OnWeaponChanged(WeaponConfig weaponInfo)
        {
            base.OnWeaponChanged(weaponInfo);
        }
    }
}
