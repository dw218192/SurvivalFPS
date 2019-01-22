using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SurvivalFPS.AI
{
    /// <summary>
    /// state machine used by zombie AI characters
    /// </summary>
    public class AIZombieStateMachine : AIStateMachine
    {
        // Inspector Assigned
        [SerializeField] [Range(10.0f, 360.0f)] private float m_Fov = 50.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Sight = 0.5f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Hearing = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Aggression = 0.5f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Intelligence = 0.5f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Satisfaction = 1.0f;
        [SerializeField] private float m_SpeedDampTime = 1.0f;
        [SerializeField] private float m_MemoryRetainingTime;

        //what's the zombie's starting upper body damage
        [SerializeField] [Range(0, 100)] private int m_StartingUpperBodyDamage; //TODO
        //what's the zombie's starting lower body damage
        [SerializeField] [Range(0, 100)] private int m_StartingLowerBodyDamage; //TODO
        [SerializeField] [Range(0, 100)] private int m_UpperBodyDamageThreshold; //TODO
        //how much damage the zombie needs to receive before it starts limping?
        [SerializeField] [Range(0, 50)] private int m_LowerBodyLimpThreshold; //TODO
        //how much damage the zombie needs to receive before it starts crawling?
        [SerializeField] [Range(51, 100)] private int m_LowerBodyCrawlThreshold; //TODO

        [SerializeField] [Range(0.0f, 0.02f)] private float m_ReplenishRate;
        [SerializeField] [Range(0.0f, 0.0005f)] private float m_DepletionRate;
        [SerializeField] Transform m_BloodParticleMount;

        //--- runtime variables ---
        private int m_UpperBodyDamage; //current upper body damage
        private int m_LowerBodyDamage; //current lower body damage
        // animator variables
        private int m_Seeking = 0;
        private bool m_Feeding = false;
        private bool m_Crawling = false;
        private int m_AttackType = 0;
        private float m_Speed = 0.0f;
        private int m_HitType = 0;

        // animator param Hashes
        private int m_SpeedHash;
        private int m_SeekingHash;
        private int m_FeedingHash;
        private int m_AttackHash;
        private int m_CrawlingHash;
        private int m_HitTriggerHash;
        private int m_HitTypeHash;

        //memory system
        private Queue<ZombieAggravator> m_InvestigatedTargets = new Queue<ZombieAggravator>();
        private float m_MemoryTimer = 0.0f;
        public Queue<ZombieAggravator> memoryQueue { get { return m_InvestigatedTargets; } }

        public bool IsTargetRecentlyInvestigated(ZombieAggravator zombieAggravator)
        {
            return m_InvestigatedTargets.Contains(zombieAggravator);
        }

        public void AddInvestigatedTarget(ZombieAggravator zombieAggravator)
        {
            if(!m_InvestigatedTargets.Contains(zombieAggravator))
            {
                m_InvestigatedTargets.Enqueue(zombieAggravator);
            }
        }

        // Public Properties
        /// <summary>
        /// [between 0 and 360] an angle that defines the cone of a zombie's vision in its spherical sensor
        /// </summary>
        public float fov { get { return m_Fov; } }
        /// <summary>
        /// [between 0 and 1] the percentage of the zombie's sensory cone that marks how far the zombie can hear
        /// </summary>
        public float hearing { get { return m_Hearing; } }
        /// <summary>
        /// [between 0 and 1] the percentage of the zombie's sensory cone that marks how far the zombie can see
        /// </summary>
        public float sight { get { return m_Sight; } }
        /// <summary>
        /// the crawling parameter in the animator
        /// </summary>
        public bool crawling { get { return m_Crawling; } }
        /// <summary>
        /// [between 0 and 1] a zombie's tracking ability
        /// </summary>
        public float intelligence { get { return m_Intelligence; } }
        /// <summary>
        /// [between 0 and 1] a zombie's hunger meter
        /// </summary>
        public float satisfaction { get { return m_Satisfaction; } set { m_Satisfaction = value; } }
        /// <summary>
        /// [between 0 and 1] how agile the zombie is
        /// </summary>
        public float aggression { get { return m_Aggression; } set { m_Aggression = value; } }
        /// <summary>
        /// the attackType parameter in the animator
        /// </summary>
        public int attackType { get { return m_AttackType; } set { m_AttackType = value; } }
        /// <summary>
        /// the feeding parameter in the animator
        /// </summary>
        public bool feeding { get { return m_Feeding; } set { m_Feeding = value; } }
        /// <summary>
        /// the seeking parameter in the animator, which is essentially a turn on spot (1 - right, 0 - stationary, -1 - left)
        /// </summary>
        public int seeking { get { return m_Seeking; } set { m_Seeking = value; } }
        /// <summary>
        /// the speed parameter in the animator
        /// </summary>
        public float speed { get { return m_Speed; } set { m_Speed = value; } }
        /// <summary>
        /// [between 0 and 1] the rate at which the zombie can replenish its hunger meter
        /// </summary>
        public float replenishRate { get { return m_ReplenishRate; } }
        /// <summary>
        /// [between 0 and 1] the rate at which the zombie depletes its hunger meter
        /// </summary>
        public float depletionRate { get { return m_DepletionRate; } }
        /// <summary>
        /// the transform for blood particles
        /// </summary>
        public Transform bloodParticleMount { get { return m_BloodParticleMount; } }
        /// <summary>
        /// has the zombie received enough damage so that it should be crawling?
        /// </summary>
        public bool shouldCrawl { get { return m_LowerBodyDamage >= m_LowerBodyCrawlThreshold; }}
        /// <summary>
        /// the zombie's current upper body damage
        /// </summary>
        /// <value>The upper body damage.</value>
        public int upperBodyDamage { get { return m_UpperBodyDamage; }  set { m_UpperBodyDamage = value; } } 
        /// <summary>
        /// the zombie's lower body damage
        /// </summary>
        /// <value>The lower body damage.</value>
        public int lowerBodyDamage { get { return m_LowerBodyDamage; }  set { m_LowerBodyDamage = value; } }
        /// <summary>
        /// the type of hit animation the zombie should be playing
        /// </summary>
        /// <value>The type of the hit.</value>
        public int hitType { get { return m_HitType; }  set { m_HitType = value; } }

        protected override void Initialize()
        {
            base.Initialize();

            m_UpperBodyDamage = m_StartingUpperBodyDamage;
            m_LowerBodyDamage = m_StartingLowerBodyDamage;
            m_Health = Mathf.Max(0, m_Health - (m_UpperBodyDamage + m_LowerBodyDamage));

            if(m_Health <= 0)
            {
                OnDeath();
            }

            //get the hashes for the params in the animator
            if(GameSceneManager.Instance)
            {
                GameSceneManager gameSceneManager = GameSceneManager.Instance;
                m_SpeedHash = gameSceneManager.speedParameterName_Hash;
                m_SeekingHash = gameSceneManager.seekingParameterName_Hash;
                m_FeedingHash = gameSceneManager.feedingParameterName_Hash;
                m_AttackHash = gameSceneManager.attackParameterName_Hash;
                m_CrawlingHash = gameSceneManager.crawlingParameterName_Hash;
                m_HitTypeHash = gameSceneManager.hitTypeParameterName_Hash;
                m_HitTriggerHash = gameSceneManager.hitParameterName_Hash;
            }
        }

        /// <summary>
        /// Refresh the animator with up-to-date values for its parameters
        /// </summary>
        protected override void UpdateStateMachine()
        {
            UpdateAnimator();
            UpdateAnimatorDamage();

            //memory
            m_MemoryTimer += Time.deltaTime;
            if(m_MemoryTimer > m_MemoryRetainingTime)
            {
                if(m_InvestigatedTargets.Count > 0)
                {
                    m_InvestigatedTargets.Dequeue();
                }
                m_MemoryTimer = 0.0f;
            }

            //hunger
            m_Satisfaction = Mathf.Max(0.0f,m_Satisfaction - Time.deltaTime * m_DepletionRate);
        }

        protected void UpdateAnimator()
        {
            if (m_Animator != null)
            {
                m_Animator.SetFloat(m_SpeedHash, m_Speed, m_SpeedDampTime, Time.deltaTime);
                m_Animator.SetBool(m_FeedingHash, m_Feeding);
                m_Animator.SetInteger(m_SeekingHash, m_Seeking);
                m_Animator.SetInteger(m_AttackHash, m_AttackType);
            }
        }

        protected void UpdateAnimatorDamage()
        {
            if (m_Animator != null)
            {
                //damage
                m_Animator.SetBool(m_CrawlingHash, shouldCrawl);
            }
        }

        /// <summary>
        /// Should be called only when the zombie is hit, not every frame
        /// </summary>
        public void UpdateAnimatorHit()
        {
            if (m_Animator != null)
            {
                //damage
                m_Animator.SetTrigger(m_HitTriggerHash);
                m_Animator.SetInteger(m_HitTypeHash, m_HitType);
            }
        }
    }
}
