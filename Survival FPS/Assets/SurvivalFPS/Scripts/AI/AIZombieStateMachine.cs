using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

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
        [SerializeField] [Range(0, 100)] private int m_StartingUpperBodyDamage;
        //what's the zombie's starting lower body damage
        [SerializeField] [Range(0, 100)] private int m_StartingLowerBodyDamage;
        [SerializeField] [Range(0, 100)] private int m_UpperBodyDamageThreshold;
        //how much damage the zombie needs to receive before it starts limping?
        [SerializeField] [Range(0, 30)] private int m_LowerBodyLimpThreshold;
        //how much damage the zombie needs to receive before it starts crawling?
        [SerializeField] [Range(31, 60)] private int m_LowerBodyCrawlThreshold;
        //how much damage the zombie needs to receive before it loses its ability to move?
        [SerializeField] [Range(61, 80)] private int m_LowerBodyNoLegThreshold;
        //how much damage the zombie needs to receive before all it can do is twitching?
        [SerializeField] [Range(81, 100)] private int m_IncapacitatedThreshold;

        [SerializeField] [Range(0.0f, 0.02f)] private float m_ReplenishRate;
        [SerializeField] [Range(0.0f, 0.0005f)] private float m_DepletionRate;
        //transforms
        [SerializeField] Transform m_BloodParticleMount; //used for blood particle effects
        [SerializeField] Transform m_HipBoneAlignmentTransform; //used in ragdoll
        [SerializeField] Transform m_HipTransform;
        //--- reanimation variables ---
        [SerializeField] float m_ReanimBlendTime = 1.5f; //how long it takes to go from ragdoll to reanimate a zombie; used in ragdoll to reanimation
        [SerializeField] float m_ReanimWaitTime = 3.0f; //how long the zombie is in ragdoll before reanimation
        [SerializeField] float m_ragDollEndTime = -1.0f; //the time when the reanimation begins
        private IEnumerator m_ReanimationRoutine = null;
        private float m_MecanimTransitionTime = 0.1f; //hacky tiny delay to wait for the animator
        //in the late update function, don't try to perform lerp till the animator starts to play the animation
        private Vector3 m_RagdollHeadPos;
        private Vector3 m_RagdollFeetPos;
        private Vector3 m_RagdollHipPos;

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
        private int m_SpeedHash = -1;
        private int m_SeekingHash = -1;
        private int m_FeedingHash = -1;
        private int m_AttackHash = -1;
        private int m_CrawlingHash = -1;
        private int m_HitTriggerHash = -1;
        private int m_HitTypeHash = -1;
        private int m_ReanimFrontTriggerHash = -1;
        private int m_ReanimBackTriggerHash = -1;
        private int m_IncapacitatedHash = -1;
        private int m_NoLegHash = -1;
        private int m_StateHash = -1;

        // animator layer
        private int m_UpperBodyIndex = -1;
        private int m_LowerBodyIndex = -1;
        private int m_HitLayerIndex = -1;
        private float m_HitLayerWeight;

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
        /// is the upper body damaged enough to be displayed in the animation?
        /// </summary>
        public bool isUpperBodyDamaged { get { return m_UpperBodyDamage >= m_UpperBodyDamageThreshold; } }
        /// <summary>
        /// has the zombie received enough damage so that it should be limping or crawling?
        /// </summary>
        public bool shouldLimp { get { return m_LowerBodyDamage >= m_LowerBodyLimpThreshold; } }
        /// <summary>
        /// has the zombie received enough damage so that it should be crawling?
        /// </summary>
        public bool shouldCrawl { get { return m_LowerBodyDamage >= m_LowerBodyCrawlThreshold; } }
        /// <summary>
        /// after reanimated, has the zombie received enough damage so that it does not move anymore?
        /// </summary>
        public bool shouldNoLeg { get { return m_LowerBodyDamage >= m_LowerBodyNoLegThreshold; } }
        /// <summary>
        /// after reanimated,has the zombie received enough damage so that it should be twitching?
        /// </summary>
        public bool shouldIncapacitate { get { return (m_Attributes.totalHealth - m_Health) >= m_IncapacitatedThreshold; } }

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
        /// <summary>
        /// the weight of the hit animation layer
        /// </summary>
        /// <value>The hit layer weight.</value>
        public float hitLayerWeight { get { return m_HitLayerWeight; }  set { m_HitLayerWeight = value; } }

        protected override void Initialize()
        {
            base.Initialize();

            m_UpperBodyDamage = m_StartingUpperBodyDamage;
            m_LowerBodyDamage = m_StartingLowerBodyDamage;
            m_Health = Mathf.Max(0, m_Health - (m_UpperBodyDamage + m_LowerBodyDamage));

            if(m_Health <= 0)
            {
                Die();
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
                m_HitLayerIndex = gameSceneManager.hitLayerIndex;
                m_LowerBodyIndex = gameSceneManager.lowerbodyIndex;
                m_UpperBodyIndex = gameSceneManager.upperbodyIndex;

                m_ReanimFrontTriggerHash = gameSceneManager.ReanimateFrontParameterName_Hash;
                m_ReanimBackTriggerHash = gameSceneManager.ReanimateBackParameterName_Hash;
                m_IncapacitatedHash = gameSceneManager.IncapacitatedParameterName_Hash;
                m_NoLegHash = gameSceneManager.NoLegParameterName_Hash;

                m_StateHash = gameSceneManager.behaviourStateParameterName_Hash;
            }
        }

        protected override void EarlyUpdateStateMachine()
        {
            //if we are hadicapped after reanimation
            //pause the AI behaviours
            if (shouldIncapacitate || shouldNoLeg )
            {
                PauseMachine();
                TryChangeState(AIStateType.Dead);
            }

            if (m_IsDead)
            {
                if(m_CurrentStateType != AIStateType.Dead)
                {
                    TryChangeState(AIStateType.Dead);
                }
            }

            else if (m_AIBoneControlType == AIBoneControlType.Ragdoll)
            {
                if(m_CurrentStateType != AIStateType.Collapsed)
                {
                    TryChangeState(AIStateType.Collapsed);
                }
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
                m_Animator.SetInteger(m_StateHash, (int) m_CurrentStateType);
            }
        }

        protected void UpdateAnimatorDamage()
        {
            if (m_Animator != null)
            {
                //damage
                m_Animator.SetBool(m_CrawlingHash, shouldCrawl);

                if(m_LowerBodyIndex != -1)
                {
                    //only activate this layer if this zombie should limp and should not crawl
                    float weight = shouldLimp && !shouldCrawl ? 1.0f : 0.0f;
                    m_Animator.SetLayerWeight(m_LowerBodyIndex, weight);
                }

                if(m_UpperBodyIndex != -1)
                {
                    float weight = isUpperBodyDamaged && !shouldCrawl ? 1.0f : 0.0f;
                    m_Animator.SetLayerWeight(m_UpperBodyIndex, weight);
                }

                //what the zombie will be after reanimation
                m_Animator.SetBool(m_NoLegHash, shouldNoLeg);
                m_Animator.SetBool(m_IncapacitatedHash, shouldIncapacitate);
            }
        }

        /// <summary>
        /// Should be called only when the zombie is hit, not every frame
        /// </summary>
        public void UpdateAnimatorHit()
        {
            if (m_Animator != null)
            {
                if(m_HitLayerIndex != -1)
                {
                    m_Animator.SetLayerWeight(m_HitLayerIndex, m_HitLayerWeight);
                }

                //damage
                m_Animator.SetTrigger(m_HitTriggerHash);
                m_Animator.SetInteger(m_HitTypeHash, m_HitType);
            }
        }

        public void Reanimate()
        {
            if (m_ReanimationRoutine != null)
            {
                StopCoroutine(m_ReanimationRoutine);
            }

            m_ReanimationRoutine = _reanimateRoutine();
            StartCoroutine(m_ReanimationRoutine);
        }

        private IEnumerator _reanimateRoutine()
        {
            if (m_AIBoneControlType != AIBoneControlType.Ragdoll || !m_Animator) yield break;
            yield return new WaitForSeconds(m_ReanimWaitTime);

            m_ragDollEndTime = Time.time;
            m_AIBoneControlType = AIBoneControlType.RagdollToAnim;

            foreach (AIBodyPart bodyPart in m_BodyParts)
            {
                bodyPart.rigidBody.isKinematic = true;
                //save the current positions and rotations of the body parts
                bodyPart.positionEndOfRagdoll = bodyPart.transform.position;
                bodyPart.rotationEndOfRagdoll = bodyPart.transform.rotation;
                bodyPart.localRotationEndOfRagdoll = bodyPart.transform.localRotation;
            }

            //record ragdoll head and feet pos
            m_RagdollHeadPos = m_Animator.GetBoneTransform(HumanBodyBones.Head).position;
            m_RagdollFeetPos = (m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + m_Animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.5f;
            m_RagdollHipPos = m_HipBoneAlignmentTransform.position;

            //enable animator
            m_Animator.enabled = true;

            //facing up
            if(m_HipBoneAlignmentTransform.forward.y >= 0)
            {
                m_Animator.SetTrigger(m_ReanimBackTriggerHash);
            }
            //facing down
            else
            {
                m_Animator.SetTrigger(m_ReanimFrontTriggerHash);
            }

            yield break;
        }

        //after the animator has updated the bone transforms
        //before the transforms are applied
        protected override void LateUpdateStateMachine()
        {
            if (m_AIBoneControlType == AIBoneControlType.RagdollToAnim)
            {
                //move the game object to where the model really is
                if (Time.time <= m_ragDollEndTime + m_MecanimTransitionTime)
                {
                    //new hip pos to old hip pos
                    Vector3 newRootToRagdollRoot = m_RagdollHipPos - m_HipTransform.position;
                    Vector3 newRootPosition = transform.position + newRootToRagdollRoot;

                    int mask = -1;
                    if(GameSceneManager.Instance)
                    {
                        mask = GameSceneManager.Instance.geometryLayerMask;
                    }

                    //find the highest geometry above the new root position
                    RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.25f), Vector3.down, float.MaxValue, mask);
                    newRootPosition.y = float.MinValue;
                    foreach (RaycastHit hit in hits)
                    {
                        //if it's not our own parts
                        if (!hit.transform.IsChildOf(transform))
                        {
                            newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);
                        }
                    }

                    //find a valid nav mesh position closest to our new root position
                    NavMeshHit navMeshHit;
                    Vector3 baseOffset = Vector3.zero;
                    if (m_navAgent) baseOffset.y = m_navAgent.baseOffset;

                    if (NavMesh.SamplePosition(newRootPosition, out navMeshHit, 25.0f, NavMesh.AllAreas))
                    {
                        transform.position = navMeshHit.position + baseOffset;
                    }
                    else
                    {
                        transform.position = newRootPosition + baseOffset;
                    }
              
                    //old body orientation
                    Vector3 ragdollBodyDirection = m_RagdollHeadPos - m_RagdollFeetPos;
                    ragdollBodyDirection.y = 0.0f;

                    Vector3 newFeetPosition = 0.5f * (m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + m_Animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    Vector3 newBodyDirection = m_Animator.GetBoneTransform(HumanBodyBones.Head).position - newFeetPosition;
                    newBodyDirection.y = 0.0f;

                    //Try to match the rotations. Note that we can only rotate around Y axis, as the animated character must stay upright,
                    //hence setting the y components of the vectors to zero. 
                    transform.rotation *= Quaternion.FromToRotation(newBodyDirection.normalized, ragdollBodyDirection.normalized);
                }

                // Now interpolate between the ragdolled bone positions to desired bone positions of the first frame of the animator
                // Calculate Interpolation value
                // How much time has passed since the reanimation begins relative to the total blend time?
                float blendAmount = Mathf.Clamp01((Time.time - m_ragDollEndTime - m_MecanimTransitionTime) / m_ReanimBlendTime);

                // if we are killed in this process
                if (m_IsDead)
                {
                    return;
                }

                // Calculate blended bone positions by interplating between ragdoll bone snapshots and animated bone positions
                foreach (AIBodyPart bodyPart in m_BodyParts)
                {
                    if (bodyPart.transform == m_HipTransform)
                    {
                        bodyPart.transform.position = Vector3.Lerp(bodyPart.positionEndOfRagdoll, bodyPart.transform.position, blendAmount);
                    }

                    bodyPart.transform.rotation = Quaternion.Slerp(bodyPart.rotationEndOfRagdoll, bodyPart.transform.rotation, blendAmount);
                }

                // Conditional to exit reanimation mode
                if (Mathf.Approximately(blendAmount, 1.0f))
                {
                    OnReanimated();
                }
            }
        }

        private void OnReanimated()
        {
            m_AIBoneControlType = AIBoneControlType.Animated;
            if (m_navAgent) m_navAgent.enabled = true;
            if (m_Collider) m_Collider.enabled = true;

            TryChangeState(AIStateType.Alerted);
        }

        public override void RagDoll()
        {
            base.RagDoll();
        }

        private bool TryChangeState(AIStateType newStateType)
        {
            AIState newState;
            if(m_States.TryGetValue(newStateType, out newState))
            {
                ChangeState(newState);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
