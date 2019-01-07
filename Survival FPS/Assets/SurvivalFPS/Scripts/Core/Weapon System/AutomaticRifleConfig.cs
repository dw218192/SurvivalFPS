using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.FPS;

namespace SurvivalFPS.Core.Weapon
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Weapon Config/Automatic Rifle Config")]
    public class AutomaticRifleConfig : WeaponConfig<AutomaticRifleBehaviour>
    {
        public override void Initialize(FirstPersonController player)
        {
            m_WeaponBehaviour = player.gameObject.AddComponent<AutomaticRifleBehaviour>();
            m_WeaponBehaviour.weaponConfig = this;
            m_WeaponBehaviour.Initialize();

            SetModel(player);
            SetAudioSource(player);
            SetAnimator(player);
        }
    }
}
