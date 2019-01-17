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

        public override void TryFire()
        {
            base.TryFire();

            //if we have ammo
            if (m_CurrentAmmo > 0)
            {
                //if the timer has gone past the fire interval, or this is our first shot
                if (m_FireTimer >= m_WeaponConfig.fireRate || m_ShotsFired == 0)
                {
                    Fire();
                }
            }
        }

        public override void DryFire()
        {
            if( m_DryFireTimer >= m_WeaponConfig.fireRate * 6.0f )
            {
                m_AudioManager.PlayRandom(m_WeaponConfig.dryFireSounds);
                m_DryFireTimer = 0.0f;
            }
        }

        protected override void OnWeaponChanged(WeaponConfig weaponInfo)
        {
            base.OnWeaponChanged(weaponInfo);
        }
    }
}
