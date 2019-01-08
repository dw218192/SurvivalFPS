using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Weapon
{
    public class AutomaticRifleBehaviour : WeaponBehaviour<AutomaticRifleConfig>
    {
        [SerializeField] private float m_TimeSinceLastFire = 0.0f;
        private bool m_StartElapseTimer = false;

        //accuracy vars
        private float m_CurrentAccuracy;

        //recoil vars
        private int m_CurrentDirectionCnt;
        private int m_Direction;
        private int m_ShotsFired = 0;

        //muzzle flash
        private ParticleSystem m_MuzzleFlash;

        public override void Initialize()
        {
            base.Initialize();
            m_CurrentAccuracy = m_WeaponConfig.accuracySettings.baseAccuracy;

            //set up particle effect
            m_MuzzleFlash = GameSceneManager.Instance.muzzleFlashParticleSystem;
            var mainModule = m_MuzzleFlash.main;
            mainModule.playOnAwake = false;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        protected void Update()
        {
            m_FireTimer += Time.deltaTime;

            if(m_StartElapseTimer)
            {
                m_TimeSinceLastFire += Time.deltaTime;
            }

            AccuracyRecovery();
        }

        protected void LateUpdate()
        {
            m_AnimatorManager.SetBool(GameSceneManager.Instance.fireParameterNameHash, false);
            m_IsFiring = false;
        }

        public override void Fire()
        {
            if(m_FireTimer >= m_WeaponConfig.fireRate || m_ShotsFired == 0)
            {
                m_StartElapseTimer = true;
                m_IsFiring = true;

                if (m_CurrentAmmo <= 0)
                {
                    m_WeaponConfig.PlayDryfireSound(); 

                    //reset timers
                    m_TimeSinceLastFire = 0.0f;
                    m_FireTimer = 0.0f;
                    return;
                }

                //play fire animation
                m_AnimatorManager.SetBool(GameSceneManager.Instance.fireParameterNameHash, true);                
                m_AnimatorManager.Play(GameSceneManager.Instance.fireStateNameHash, -1);

                m_WeaponConfig.PlayFireSound();

                m_CurrentAmmo--;
                m_ShotsFired = m_WeaponConfig.ammoCapacity - m_CurrentAmmo;

                PerformRayCast();

                if (m_WeaponConfig.muzzleEffect)
                {
                    PlayMuzzleEffect();
                }

                if (m_WeaponConfig.recoilEnabled)
                {
                    Recoil();
                }

                if (m_WeaponConfig.spitShells)
                {
                    SpitShells();
                }

                //reset timers
                m_TimeSinceLastFire = 0.0f;
                m_FireTimer = 0.0f;
            }
        }

        public override void Recoil()
        {
            float kickUp;
            float kickLateral;
            Vector3 angle = Vector3.zero;

            WeaponConfig.RecoilData recoilData = m_WeaponConfig.recoilSettings;

            // This is the first round fired
            if (m_ShotsFired == 1) 
            {
                kickUp = recoilData.kickUpBase;
                kickLateral = recoilData.kickLateralBase;

                m_CurrentDirectionCnt = 0;
            }
            else
            {
                float extraKickUp = m_ShotsFired * (recoilData.kickUpModifier - m_TimeSinceLastFire * recoilData.recoilResetModifier);
                float extraKickLateral = m_ShotsFired * (recoilData.kickLateralModifier - m_TimeSinceLastFire * recoilData.recoilResetModifier);

                extraKickUp = Mathf.Max(0.0f, extraKickUp);
                extraKickLateral = Mathf.Max(0.0f, extraKickLateral);

                kickUp = recoilData.kickUpBase + extraKickUp * Time.deltaTime;
                kickLateral = recoilData.kickLateralBase + extraKickLateral * Time.deltaTime;
            }

            //kickUp = Mathf.Min(kickUp, m_Kickback_UpLimit);
            //kickLateral = Mathf.Min(kickLateral, m_Kickback_LateralLimit);
            /*
            const int NumKickbackLimiters = KickbackLimiter.Num();
            if (NumKickbackLimiters > 0)
            {
                const FKickbackRange&Range = KickbackLimiter(Min(NumKickbackLimiters - 1, (INT)iShotsFired));

                const FLOAT Reducer = appFrand() * (Range.Max - Range.Min) + Range.Min;

                kickUp *= Reducer;
                kickLateral *= Reducer;
            }

            angle = m_Player.punchAngle;
            */

            angle.x += kickUp;

            if (angle.x > recoilData.kickUpMax)
            {
                angle.x = recoilData.kickUpMax;
            }
            angle.x = - angle.x;

            if (m_Direction == 1)
            {
                angle.y += kickLateral;
                if (angle.y > recoilData.kickLateralMax)
                {
                    angle.y = recoilData.kickLateralMax;
                }
            }
            else
            {
                angle.y -= kickLateral;

                if (angle.y < - recoilData.kickLateralMax)
                {
                    angle.y = -recoilData.kickLateralMax;
                }
            }

            if (--m_CurrentDirectionCnt <= 0)
            {
                if (recoilData.sideDirChange != 0 && (UnityEngine.Random.Range(0, recoilData.sideDirChange) == 0))
                {
                    m_Direction = 1 - m_Direction;

                    m_CurrentDirectionCnt = 5;
                }
            }

            m_Player.punchAngle = angle;
//            Debug.Log(angle);
        }

        public override void SpitShells()
        {

        }

        public override void PlayMuzzleEffect()
        {
            if(m_MuzzleFlash)
            {
                m_MuzzleFlash.transform.parent = m_WeaponConfig.gunGameObject.transform;
                m_MuzzleFlash.transform.localPosition = m_WeaponConfig.fireStartSpot.position;

                m_MuzzleFlash.Emit(1);
            }
            Debug.Log(m_MuzzleFlash.transform.position);
        }

        private void PerformRayCast()
        {
            // Calculate accuracy for this shot
            float inaccuracy = (100.0f - m_CurrentAccuracy) / 1000.0f;

            Vector3 startPoint = m_WeaponConfig.gunGameObject.transform.TransformPoint(m_WeaponConfig.fireStartSpot.position);
            Vector3 direction = Camera.main.transform.TransformDirection(Vector3.forward);

            direction.x += UnityEngine.Random.Range(-inaccuracy, inaccuracy);
            direction.y += UnityEngine.Random.Range(-inaccuracy, inaccuracy);
            direction.z += UnityEngine.Random.Range(-inaccuracy, inaccuracy);
            m_CurrentAccuracy -= m_WeaponConfig.accuracySettings.accuracyDropPerShot;

            if (m_CurrentAccuracy <= 0.0f)
            {
                m_CurrentAccuracy = 0.0f;
            }

            // The ray that will be used for this shot
            Ray ray = new Ray(startPoint, direction);
            RaycastHit hit;

            Debug.DrawLine(startPoint, startPoint + direction, Color.red);

            if (Physics.Raycast(ray, out hit, m_WeaponConfig.range, GameSceneManager.Instance.shootableLayerMask))
            {
                // Damage
                if (BulletHoleManager.Instance)
                {
                    GameObject target = hit.collider.gameObject;
                    BulletHoleManager.Instance.PlaceBulletHole(hit.point, Quaternion.identity, target);
                }
            }
        }

        private void AccuracyRecovery()
        {
            m_CurrentAccuracy = Mathf.MoveTowards(m_CurrentAccuracy, m_WeaponConfig.accuracySettings.baseAccuracy, m_WeaponConfig.accuracySettings.accuracyRecoveryRate * Time.deltaTime);
        }

        protected override void OnWeaponChanged(WeaponConfig weaponInfo)
        {
            //TODO remove string ref
            if (weaponInfo == m_WeaponConfig)
            {
                m_AnimatorManager.Play("Bring Up Weapon", -1, 0.0f);
                m_WeaponConfig.PlayBringUpSound();
            }
        }
    }
}
