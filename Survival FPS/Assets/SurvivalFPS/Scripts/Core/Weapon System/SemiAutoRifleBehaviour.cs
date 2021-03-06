﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    public class SemiAutoRifleBehaviour : WeaponBehaviour<SemiAutoRifleConfig>
    {
        private bool m_CanFire = false;

        public override void TryFire()
        {
            base.TryFire();

            //if we have ammo
            if (m_CurrentAmmo > 0)
            {
                //if the timer has gone past the fire interval, or this is our first shot
                if (m_FireTimer >= m_WeaponConfig.fireRate || m_ShotsFired == 0)
                {
                    if(m_WeaponController.fireLeadingEdge)
                    {
                        Fire();
                    }
                }
            }
        }

        public override void DryFire()
        {
            if (m_WeaponController.fireLeadingEdge)
            {
                m_DryFireTimer = 0.0f;
            }
        }

        protected override void OnWeaponChanged(WeaponConfig weaponInfo)
        {
            base.OnWeaponChanged(weaponInfo);
        }
    }

}
