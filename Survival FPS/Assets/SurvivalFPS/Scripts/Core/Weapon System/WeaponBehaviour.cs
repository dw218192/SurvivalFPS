using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.FPS;

namespace SurvivalFPS.Core.Weapon
{
    //interfaces
    public interface IWeaponDamageable
    {
        void TakeDamage(WeaponConfig weaponUsed, Vector3 hitPosition, Vector3 hitDirection, GameObject instigator);
    }

    /// <summary>
    /// a runtime component attached to the player game object,
    /// which should be managed by the config classes
    /// </summary>
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        protected int m_CurrentAmmo = 0;

        //states
        protected bool m_IsFiring;
        protected bool m_IsReloading;

        //private internal fields set by config
        protected FirstPersonController m_FPSController;
        protected Animator m_Animator; //animator of this weapon only
        protected PlayerAnimatorManager m_AnimatorManager; //animator collection of the player
        protected PlayerWeaponController m_WeaponController; //weapon controller
        protected GameSceneManager m_GameSceneManager; //game scene manager of the player
        protected Camera m_PlayerCamera; //central manager of the player
        public FirstPersonController FPSController { set { m_FPSController = value; } }
        public Animator animator { set { m_Animator = value; } }
        public PlayerAnimatorManager animatorManager { set { m_AnimatorManager = value; } }
        public PlayerWeaponController weaponController { set { m_WeaponController = value; }}
        public Camera playerCamera { set { m_PlayerCamera = value; }}

        public int currentAmmo { get { return m_CurrentAmmo; } }
        public bool isFiring { get { return m_IsFiring; } }
        public bool isReloading { get { return m_IsReloading; } }

        protected IEnumerator m_ReloadingRoutine = null;

        public abstract void TryFire();
        public abstract void Reload();
        public virtual void Recoil() { }
        public virtual void SpitShells() { }
        public virtual void PlayMuzzleEffect() { }

        protected virtual void Awake() { }
        protected virtual void Start() { }

        public abstract void Initialize();
    }

    /// <summary>
    /// Base class of all weapons;
    /// it is attached to the player game object and initialized by its corresponding config class.
    /// It implements basic functionalities which are:
    /// reloading / weapon bring up
    /// </summary>
    public abstract class WeaponBehaviour<T> : WeaponBehaviour where T : WeaponConfig
    {
        protected T m_WeaponConfig;
        public T weaponConfig { get { return m_WeaponConfig; } set { m_WeaponConfig = value; } }

        //muzzle flash
        protected ParticleSystem m_MuzzleFlash;

        //accuracy vars
        [SerializeField] protected AccuracyData m_CurrentAccuracyData;
        [SerializeField] protected float m_CurrentAccuracy;

        //recoil vars
        [SerializeField] private RecoilData m_CurrentRecoilData;
        protected int m_CurrentDirectionCnt;
        protected int m_Direction;
        protected int m_ShotsFired = 0;

        //timers
        protected float m_FireTimer = 0.0f; //a timer that's usually used to determine whether the weapon can fire
        protected float m_DryFireTimer = 0.0f; //a timer for playing the dry fire sounds
        protected float m_TimeSinceLastFire = 0.0f; //how much time has elasped since the last time this weapon fired?
        private bool m_StartElapseTimer = false;

        //crosshair
        private Vector2[] m_CurCrossHairPositions;
        private float m_ScreenMaxSpreadDist = -1.0f;
        private float m_CrossHairSpeed = 100.0f;
        private int m_CrossHairWidth;
        private int m_CrossHairLength;


        //don't use these
        protected sealed override void Awake()
        {
            //reference to singleton managers
            m_GameSceneManager = GameSceneManager.Instance;
        }
        protected sealed override void Start() { }

        public override void Initialize()
        {
            m_CurrentAmmo = m_WeaponConfig.ammoCapacity;

            //register events
            m_WeaponController = m_FPSController.GetComponent<PlayerWeaponController>();
            if (m_WeaponController) m_WeaponController.RegisterWeaponChangeEvent(OnWeaponChanged);
        
            //general settings of the muzzle flash particle effect
            m_MuzzleFlash = m_GameSceneManager.muzzleFlashParticleSystem;
            var mainModule = m_MuzzleFlash.main;
            mainModule.playOnAwake = false;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;

            //accuracy and recoil initialization
            m_CurrentRecoilData = GetRecoilData();
            m_CurrentAccuracyData = GetAccuracyData();
            m_CurrentAccuracy = m_CurrentAccuracyData.baseAccuracy;

            //crosshair initialization
            m_CrossHairWidth = Screen.width / 400;
            m_CrossHairLength = Screen.height / 80;
            m_CurCrossHairPositions = CalculateBaseCrossHairPos();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            //clear the runtime fields if this weapon got switched out
            m_FireTimer = 0.0f;
            m_StartElapseTimer = false;
            m_TimeSinceLastFire = 0.0f;
            m_ShotsFired = 0;
        }

        protected virtual void Update()
        {
            //timer increment
            m_FireTimer += Time.deltaTime;
            m_DryFireTimer += Time.deltaTime;

            if (m_StartElapseTimer)
            {
                m_TimeSinceLastFire += Time.deltaTime;
            }

            //data update
            m_CurrentRecoilData = GetRecoilData();
            m_CurrentAccuracyData = GetAccuracyData();


            AccuracyRecovery();
        }

        public override void TryFire()
        {
            if (m_CurrentAmmo <= 0)
            {
                DryFire();
                return;
            }
        }

        protected void LateUpdate()
        {
            if (m_IsFiring)
            {
                m_AnimatorManager.SetBool(m_GameSceneManager.fireParameterNameHash, false);
                m_IsFiring = false;
            }
        }

        /// <summary>
        /// performs raycast, fires a single shot, plays corresponding effects
        /// and resets FireTimer and TimeSinceLastFire timer
        /// </summary>
        protected virtual void Fire()
        {
            m_StartElapseTimer = true;
            m_IsFiring = true;


            //play fire animation
            m_AnimatorManager.SetBool(m_GameSceneManager.fireParameterNameHash, true);
            m_AnimatorManager.Play(m_GameSceneManager.fireStateNameHash, -1);

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

        #region Reload
        public override void Reload()
        {
            //if we can reload
            if (m_CurrentAmmo < m_WeaponConfig.ammoCapacity)
            {
                //if we are not reloading
                if (m_ReloadingRoutine == null)
                {
                    m_ReloadingRoutine = _ReloadRoutine();
                    StartCoroutine(m_ReloadingRoutine);
                }
            }
        }

        private IEnumerator _ReloadRoutine()
        {
            //make sure the weapon is active
            if (!m_WeaponConfig.isActive)
            {
                yield break;
            }

            m_FireTimer = -m_WeaponConfig.reloadTime;

            //set the parameter to begin the transition into the reloading animation
            if (m_AnimatorManager)
            {
                m_AnimatorManager.SetBool(m_GameSceneManager.reloadParameterHash, true);
            }

            //wait for the reload animation to begin playing
            while (m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != m_GameSceneManager.reloadStateNameHash)
            {
                //if the weapon got switched out in the middle of the reloading
                if (!m_WeaponConfig.isActive)
                {
                    ExitReloading(true);
                    yield break;
                }

                yield return null;
            }

            //the reloading animation starts playing
            m_IsReloading = true;

            //only increment the ammo when the animation reaches a specific point (i.e. magazine changed)
            while (m_Animator.GetFloat(m_GameSceneManager.reloadCurveParameterName) < -0.5f)
            {
                //if the weapon got switched out in the middle of the reloading
                if (!m_WeaponConfig.isActive)
                {
                    ExitReloading(true);
                    yield break;
                }

                yield return null;
            }

            //we've finished reloading
            m_CurrentAmmo = m_WeaponConfig.ammoCapacity;
            ExitReloading(false);
        }

        private void ExitReloading(bool interrupted)
        {
            if (interrupted)
            {
                m_FireTimer = 0.0f;
            }

            //clean the flags
            m_ReloadingRoutine = null;
            m_IsReloading = false;

            //turn off the animator flag of this weapon and other related animators (arms/hands/etc)
            if (m_AnimatorManager)
            {
                m_Animator.SetBool(m_GameSceneManager.reloadParameterHash, false);
                m_AnimatorManager.SetBool(m_GameSceneManager.reloadParameterHash, false);
            }
        }
        #endregion

        #region Fire Helper Functions
        public abstract void DryFire();

        public override void Recoil()
        {
            float kickUp;
            float kickLateral;
            Vector3 angle = Vector3.zero;

            if (m_CurrentRecoilData == null)
            {
                return;
            }

            // This is the first round fired
            if (m_ShotsFired == 1)
            {
                kickUp = m_CurrentRecoilData.kickUpBase;
                kickLateral = m_CurrentRecoilData.kickLateralBase;

                m_CurrentDirectionCnt = 0;
            }
            else
            {
                float extraKickUp = m_ShotsFired * (m_CurrentRecoilData.kickUpModifier - m_TimeSinceLastFire * m_CurrentRecoilData.recoilResetModifier);
                float extraKickLateral = m_ShotsFired * (m_CurrentRecoilData.kickLateralModifier - m_TimeSinceLastFire * m_CurrentRecoilData.recoilResetModifier);

                extraKickUp = Mathf.Max(0.0f, extraKickUp);
                extraKickLateral = Mathf.Max(0.0f, extraKickLateral);

                kickUp = m_CurrentRecoilData.kickUpBase + extraKickUp * Time.deltaTime;
                kickLateral = m_CurrentRecoilData.kickLateralBase + extraKickLateral * Time.deltaTime;
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

            if (angle.x > m_CurrentRecoilData.kickUpMax)
            {
                angle.x = m_CurrentRecoilData.kickUpMax;
            }
            angle.x = -angle.x;

            if (m_Direction == 1)
            {
                angle.y += kickLateral;
                if (angle.y > m_CurrentRecoilData.kickLateralMax)
                {
                    angle.y = m_CurrentRecoilData.kickLateralMax;
                }
            }
            else
            {
                angle.y -= kickLateral;

                if (angle.y < -m_CurrentRecoilData.kickLateralMax)
                {
                    angle.y = -m_CurrentRecoilData.kickLateralMax;
                }
            }

            if (--m_CurrentDirectionCnt <= 0)
            {
                if (m_CurrentRecoilData.sideDirChange != 0 && (UnityEngine.Random.Range(0, m_CurrentRecoilData.sideDirChange) == 0))
                {
                    m_Direction = 1 - m_Direction;

                    m_CurrentDirectionCnt = 5;
                }
            }

            m_FPSController.punchAngle = angle;
        }

        private RecoilData GetRecoilData()
        {
            if (m_FPSController.crouching)
            {
                return m_WeaponConfig.recoilSettingsWhenCrouching;
            }

            if (m_FPSController.XZVelocity.magnitude < 0.1f)
            {
                return m_WeaponConfig.recoilSettingsWhenStill;
            }
            else
            {
                return m_WeaponConfig.recoilSettingsWhenWalking;
            }
        }

        private AccuracyData GetAccuracyData()
        {
            if (m_FPSController.crouching)
            {
                return m_WeaponConfig.accuracySettingsWhenCrouching;
            }

            if (m_FPSController.XZVelocity.magnitude < 0.1f)
            {
                return m_WeaponConfig.accuracySettingsWhenStill;
            }
            else
            {
                return m_WeaponConfig.accuracySettingsWhenWalking;
            }
        }

        protected void PerformRayCast()
        {
            // Calculate inaccuracy
            float inaccuracy = (100.0f - m_CurrentAccuracy) / 1000.0f;

            //local space
            Vector3 screenStartPoint = new Vector3(Screen.width / 2, Screen.height / 2, 0.5f);
            float localOffsetX, localOffsetY;
            localOffsetX = UnityEngine.Random.Range(-inaccuracy, inaccuracy);
            localOffsetY = UnityEngine.Random.Range(-inaccuracy, inaccuracy);
            Vector3 localDir = new Vector3(localOffsetX, localOffsetY, 1.0f);

            //calculate ray start and direction in world space
            Vector3 startPoint = m_PlayerCamera.ScreenToWorldPoint(screenStartPoint);
            Vector3 direction = m_PlayerCamera.transform.TransformDirection(localDir);

            m_CurrentAccuracy -= m_CurrentAccuracyData.accuracyDropPerShot;

            if (m_CurrentAccuracy <= 0.0f)
            {
                m_CurrentAccuracy = 0.0f;
            }

            // The ray that will be used for this shot
            Ray ray = new Ray(startPoint, direction);
            RaycastHit hit;

            //Debug.DrawLine(startPoint, startPoint + direction, Color.red);

            if (Physics.Raycast(ray, out hit, m_WeaponConfig.range, m_GameSceneManager.shootableLayerMask))
            {
                // bullet hole
                if (BulletHoleManager.Instance)
                {
                    GameObject target = hit.collider.gameObject;
                    if(!target.transform.root.GetComponent<BulletHoleException>())
                    {
                        BulletHoleManager.Instance.PlaceBulletHole(hit.point, hit.normal, target);
                    }
                }

                IWeaponDamageable weaponDamageable = hit.collider.GetComponent(typeof(IWeaponDamageable)) as IWeaponDamageable;
                if (weaponDamageable != null)
                {
                    weaponDamageable.TakeDamage(m_WeaponConfig, hit.point, direction, m_FPSController.gameObject);
                }
            }
        }

        public override void SpitShells()
        {

        }

        public override void PlayMuzzleEffect()
        {
            if (m_MuzzleFlash)
            {
                m_MuzzleFlash.Emit(1);
            }
        }
        #endregion

        protected void AccuracyRecovery()
        {
            m_CurrentAccuracy = Mathf.MoveTowards(m_CurrentAccuracy, m_CurrentAccuracyData.baseAccuracy, m_CurrentAccuracyData.accuracyRecoveryRate * Time.deltaTime);
        }

        //callbacks
        protected virtual void OnWeaponChanged(WeaponConfig weaponInfo) 
        {
            //TODO remove string ref
            if (weaponInfo == m_WeaponConfig)
            {
                if (m_MuzzleFlash)
                {
                    var mainModule = m_MuzzleFlash.main;
                    //match the muzzle effect to the fire rate of this weapon
                    mainModule.startLifetime = m_WeaponConfig.fireRate;
                    //parent the effect to the weapon so that it will move with the weapon
                    m_MuzzleFlash.transform.parent = m_WeaponConfig.gunGameObject.transform;
                    m_MuzzleFlash.transform.localPosition = m_WeaponConfig.fireStartSpot.position;
                }

                m_AnimatorManager.Play(m_GameSceneManager.bringUpStateNameHash, -1, 0.0f);
            }
        }

        //crosshair
        private void OnGUI()
        {
            CalculateSpread();

            Vector2[] crossHairBasePositions = CalculateBaseCrossHairPos();

            if(m_ScreenMaxSpreadDist > 0)
            {
                Vector2[] crossHairTargetPositions =
                {
                    crossHairBasePositions[0] + m_ScreenMaxSpreadDist * new Vector2 (0,-1),
                    crossHairBasePositions[1] + m_ScreenMaxSpreadDist * new Vector2 (0,1),
                    crossHairBasePositions[2] + m_ScreenMaxSpreadDist * new Vector2 (-1,0),
                    crossHairBasePositions[3] + m_ScreenMaxSpreadDist * new Vector2 (1,0)
                };

                m_CurCrossHairPositions[0] = Vector2.MoveTowards(m_CurCrossHairPositions[0], crossHairTargetPositions[0], m_CrossHairSpeed * Time.deltaTime);
                m_CurCrossHairPositions[1] = Vector2.MoveTowards(m_CurCrossHairPositions[1], crossHairTargetPositions[1], m_CrossHairSpeed * Time.deltaTime);
                m_CurCrossHairPositions[2] = Vector2.MoveTowards(m_CurCrossHairPositions[2], crossHairTargetPositions[2], m_CrossHairSpeed * Time.deltaTime);
                m_CurCrossHairPositions[3] = Vector2.MoveTowards(m_CurCrossHairPositions[3], crossHairTargetPositions[3], m_CrossHairSpeed * Time.deltaTime);
            }

            Vector2 size1 = new Vector2(m_CrossHairWidth, m_CrossHairLength);
            Vector2 size2 = new Vector2(m_CrossHairLength, m_CrossHairWidth);

            Rect rect1 = new Rect(m_CurCrossHairPositions[0], size1);
            Rect rect2 = new Rect(m_CurCrossHairPositions[1], size1);
            Rect rect3 = new Rect(m_CurCrossHairPositions[2], size2);
            Rect rect4 = new Rect(m_CurCrossHairPositions[3], size2);

            Texture2D texture = m_WeaponConfig.crossHairTexture;

            GUI.DrawTexture(rect1, texture);
            GUI.DrawTexture(rect2, texture);
            GUI.DrawTexture(rect3, texture);
            GUI.DrawTexture(rect4, texture);
        }

        //crosshair helpers
        private void CalculateSpread()
        {
            if (!m_PlayerCamera)
            {
                Debug.LogWarning("WeaponBehavior - player does not have a camera!");
                return;
            }

            Vector3 screenStartPoint = new Vector3(Screen.width / 2, Screen.height / 2, 0.5f);
            Vector3 startPoint = m_PlayerCamera.ScreenToWorldPoint(screenStartPoint);

            //cross-hair spread range calculation
            //if the local x offset is greater, 
            //imagine deflect the ray only by this offset, and use this to calculate the boundary of the crosshair
            Vector3 localMaxSpreadDir = new Vector3(- (100.0f - m_CurrentAccuracy) / 1000.0f, 0.0f, 0.5f);
            Vector3 maxSpreadDir = m_PlayerCamera.transform.TransformDirection(localMaxSpreadDir);

            Vector2 screenMaxSpread = m_PlayerCamera.WorldToScreenPoint(startPoint + maxSpreadDir.normalized * m_WeaponConfig.range);
            m_ScreenMaxSpreadDist = (screenMaxSpread - (Vector2)screenStartPoint).magnitude;
        }

        private Vector2[] CalculateBaseCrossHairPos()
        {
            int halfScreenWidth = Screen.width / 2, halfScreenHeight = Screen.height / 2;
            float halfCrosshairWidth = m_CrossHairWidth / 2.0f, halfCrosshairLength = m_CrossHairLength / 2.0f;

            Vector2[] crossHairBasePositions =
            {
                //up
                new Vector2(halfScreenWidth - halfCrosshairWidth, halfScreenHeight - m_CrossHairLength),
                //down
                new Vector2(halfScreenWidth - halfCrosshairWidth, halfScreenHeight                     ),
                //left
                new Vector2(halfScreenWidth - m_CrossHairLength , halfScreenHeight - halfCrosshairWidth),
                //right
                new Vector2(halfScreenWidth                     , halfScreenHeight - halfCrosshairWidth)
            };

            return crossHairBasePositions;
        }
    }
}
