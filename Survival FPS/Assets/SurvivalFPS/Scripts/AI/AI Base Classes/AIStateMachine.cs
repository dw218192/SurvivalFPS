using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

namespace SurvivalFPS.AI
{
    //interfaces
    public interface IAIDamageable
    {
        void TakeDamage(float amountPerSec);
    }

    //enums
    //who is controlling the bones now?
    public enum AIBoneControlType { Animated, Ragdoll, RagdollToAnim }
    public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead, Collapsed }
    public enum AITargetType { None, Waypoint, Aggravator }
    public enum AITriggerEventType { Enter, Stay, Exit }
    public enum AIBodyPartType { None, Head, UpperBody, UpperBodyLimb, LowerBody, LowerBodyLimb }

    //Info class for AI targets
    public class AITarget
    {
        private AITargetType m_Type;
        private Collider m_Collider;
        private Vector3 m_LastKnownPosition;
        private float m_Distance;
        private float m_Time;

        public AITargetType type { get { return m_Type; } }
        public Collider collider { get { return m_Collider; } }
        public Vector3 lastKnownPosition { get { return m_LastKnownPosition; } set { m_LastKnownPosition = value; } }
        public float distance { get { return m_Distance; } set { m_Distance = value; } }
        public float time { get { return m_Time; } }

        public void Set(AITargetType targetType, Collider targetCollider, Vector3 targetPosition, float distanceToTarget)
        {
            m_Type = targetType;
            m_Collider = targetCollider;
            m_LastKnownPosition = targetPosition;
            m_Distance = distanceToTarget;
            m_Time = Time.time;
        }

        public void Clear()
        {
            m_Type = AITargetType.None;
            m_Collider = null;
            m_LastKnownPosition = Vector3.zero;
            m_Distance = Mathf.Infinity;
            m_Time = 0.0f;
        }
    }

    /// <summary>
    /// the brain of the AI in this game. It should be in the AI entity layer that can only collide with what is in either the AI entity trigger or AI trigger layer
    /// </summary>
    
    [SelectionBase]
    public abstract class AIStateMachine : MonoBehaviour
    {
        //configuration classes
        [Serializable]
        public class AIAttributes
        {
            [Range(0, 100)] public float damagePerSec = 10;
            [Range(0, 100)] public int totalHealth = 100;
            [Range(10, 360)] public float turnSpeed = 60; //turnning speed when not using root rot
        }
        //AI default attributes
        [SerializeField] protected AIAttributes m_Attributes;

        //----public fields set by child states each frame----
        [HideInInspector] public ZombieVisualAggravator visualThreat = null;
        [HideInInspector] public ZombieAudioAggravator audioThreat = null;

        //----protected internals----
        protected Dictionary<AIStateType, AIState> m_States = new Dictionary<AIStateType, AIState>();
        protected AITarget m_Target = new AITarget(); //actual target of the AI
        protected AIState m_CurrentState = null;
        protected AIBoneControlType m_AIBoneControlType = AIBoneControlType.Animated;
        //flags
        protected bool m_IsTargetReached; //has the AI reached its destination?
        protected bool m_IsInMeleeRange; //is the AI able to melee attack?
        //animator related
        protected int m_RootRotationRefCount = 0;
        protected int m_RootPositionRefCount = 0;
        protected bool m_CinematicEnabled = false; //TODO
        public bool cinematicEnabled { get { return m_CinematicEnabled; } set { m_CinematicEnabled = value; } }
        //component cache
        protected Animator m_Animator = null;
        protected NavMeshAgent m_navAgent = null;
        protected Collider m_Collider = null;   //the collider representing the AI character; it should be in a layer where it can only collides with designated objects
        protected Transform m_Transform = null;
        //runtime attribute variables
        [SerializeField] protected int m_Health;
        protected bool m_IsDead = false;
        private bool m_UpdateState = true;

        //----inspector configurable variables----
        [SerializeField] protected AIStateType m_CurrentStateType = AIStateType.Idle;
        //triggers
        [SerializeField] protected SphereCollider m_TargetTrigger = null; //the sphere trigger at the AI's target position
        [SerializeField] protected SphereCollider m_SensorTrigger = null; //the sphere trigger representing the AI's sensory range
        [SerializeField] protected AIDamageTrigger m_RightHandAttackTrigger = null;
        [SerializeField] protected AIDamageTrigger m_LeftHandAttackTrigger = null;
        [SerializeField] protected AIDamageTrigger m_MouthTrigger = null;
        //body parts that are used for ragroll
        [SerializeField] protected List<AIBodyPart> m_BodyParts;
        [SerializeField] [Range(0.0f, 15.0f)] protected float m_StoppingDistance = 1.0f;
        //transform info about body parts
        //used in footstep track
        [SerializeField] private Transform m_RightFootTransform;
        [SerializeField] private Transform m_LeftFootTransform;
        //----public properties----
        public Animator animator { get { return m_Animator; } }
        public NavMeshAgent navAgent { get { return m_navAgent; } }
        public AIBoneControlType curBoneControlType { get { return m_AIBoneControlType; }}

        public AITargetType currentTargetType { get { return m_Target.type; } }
        public bool hasTarget { get { return (m_Target.type != AITargetType.None); } }

        public bool isInMeeleRange { get { return m_IsInMeleeRange; } set { m_IsInMeleeRange = value; } }
        public bool isTargetReached { get { return m_IsTargetReached; } set { m_IsTargetReached = value; } }

        /// <summary>
        /// how much damage this AI can inflict per second 
        /// </summary>
        /// <value>The damage per sec.</value>
        public float damagePerSec { get { return m_Attributes.damagePerSec; } }
        /// <summary>
        /// the maximum health level of the AI
        /// </summary>
        public int totalHealth { get { return m_Attributes.totalHealth; } }
        /// <summary>
        /// the turn speed of the AI when it is not using root rotation
        /// </summary>
        public float turnSpeed { get { return m_Attributes.turnSpeed; } }
        /// <summary>
        /// the current health level of the AI
        /// </summary>
        public int currentHealth { get { return m_Health; } set { m_Health = value; } }
        //debugging
        public SphereCollider targetTrigger { get { return m_TargetTrigger; } }
        /// <summary>
        /// is this AI actively doing something? e.g. not in a ragdoll/ not dead/ is updating its states
        /// </summary>
        public bool isActiveAI
        {
            get
            {
                return (m_AIBoneControlType == AIBoneControlType.Animated
                        && m_UpdateState);
            }
        }
        /// <summary>
        /// the position of the center of the AI sensor trigger in world space
        /// </summary>
        public Vector3 sensorPosition
        {
            get
            {
                if (!m_SensorTrigger) return Vector3.zero;
                else
                {
                    Vector3 point = m_SensorTrigger.transform.position;
                    //correct the point by the collider center offset and the scale of the parent game object
                    point.x += m_SensorTrigger.center.x * m_SensorTrigger.transform.lossyScale.x;
                    point.y += m_SensorTrigger.center.y * m_SensorTrigger.transform.lossyScale.y;
                    point.z += m_SensorTrigger.center.z * m_SensorTrigger.transform.lossyScale.z;
                    return point;
                }
            }
        }
        
        /// <summary>
        /// the radius of the AI sensor
        /// </summary>
        public float sensorRadius
        {
            get
            {
                if (!m_SensorTrigger) return 0.0f;
                else
                {
                    //which one is larger, the x scale or the y scale?
                    float radius = Mathf.Max(m_SensorTrigger.radius * m_SensorTrigger.transform.lossyScale.x, m_SensorTrigger.radius * m_SensorTrigger.transform.lossyScale.y);
                    //which one is larger, the y scale or the z scale?
                    radius = Mathf.Max(m_SensorTrigger.radius * m_SensorTrigger.transform.lossyScale.y, m_SensorTrigger.radius * m_SensorTrigger.transform.lossyScale.z);
                    //return the radius scaled by the largest scale
                    return radius;
                }
            }
        }

        /// <summary>
        /// is the animator controller using root positon?
        /// </summary>
        public bool useRootPosition { get { return m_RootPositionRefCount > 0; } }

        /// <summary>
        /// is the animator controller using root rotation?
        /// </summary>
        public bool useRootRotation { get { return m_RootRotationRefCount > 0; } }

        /// <summary>
        /// is this AI dead?
        /// </summary>
        public bool IsDead { get { return m_IsDead; } }

        //transform info
        public Transform leftFootTransform { get { return m_LeftFootTransform; } }
        public Transform rightFootTransform { get { return m_RightFootTransform; } }

        //delegates
        private event Action OnAIDeath;
        public void RegisterAIDeathEvent(Action action)
        {
            OnAIDeath += action;
        }
        private event Action OnRagdoll;
        public void RegisterRagdollEvent(Action action)
        {
            OnRagdoll += action;
        }

        //----overwrittable methods----
        /// <summary>
        /// this method is coupled to the start method, for additional initialization work
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// this method is called every frame, before the child states are updated
        /// </summary>
        protected virtual void EarlyUpdateStateMachine() { }

        /// <summary>
        /// this method is called every frame, after the child states are updated
        /// </summary>
        protected virtual void UpdateStateMachine() { }

        /// <summary>
        /// this method is called every frame, in the late update
        /// </summary>
        protected virtual void LateUpdateStateMachine() { }

        private void Awake()
        {
            m_Transform = transform;
            m_Animator = GetComponent<Animator>();
            m_Collider = GetComponent<CapsuleCollider>();
            m_navAgent = GetComponent<NavMeshAgent>();

            //set layers
            gameObject.layer = GameSceneManager.Instance.aIEntityLayer;

            if(m_TargetTrigger)
            {
                m_TargetTrigger.gameObject.layer = GameSceneManager.Instance.aIEntityTriggerLayer;
            }
            else
            {
                Debug.LogWarning(gameObject.name + ": " + "AI Target Trigger is missing! This AI will not work properly");
            }

            if (m_SensorTrigger)
            {
                m_SensorTrigger.gameObject.layer = GameSceneManager.Instance.aITriggerLayer;
            }
            else
            {
                Debug.LogWarning(gameObject.name + ": " + "AI Sensor Trigger is missing! This AI will not work properly");
            }
        }

        private void Start()
        {
            if (m_Collider) GameSceneManager.Instance.RegisterAIStateMachineByColliderID(m_Collider.GetInstanceID(), this);
            if (m_SensorTrigger) GameSceneManager.Instance.RegisterAIStateMachineByColliderID(m_SensorTrigger.GetInstanceID(), this);
            //anything else that may be added

            foreach (AIBodyPart bodyPart in m_BodyParts)
            {
                GameSceneManager.Instance.RegisterAIStateMachineByColliderID(bodyPart.bodyPartCollider.GetInstanceID(), this);

                bodyPart.owner = this;
                bodyPart.rigidBody.isKinematic = true;
            }

            //ownership assignments and component initialization
            if (m_SensorTrigger)
            {
                AISensor sensor = m_SensorTrigger.gameObject.GetComponent<AISensor>();
                sensor.owner = this;
            }

            if (m_LeftHandAttackTrigger)
            {
                m_LeftHandAttackTrigger.owner = this;
                m_LeftHandAttackTrigger.animatorDamageParameter = GameSceneManager.Instance.leftHandAttackParameterName;
            }

            if (m_RightHandAttackTrigger)
            {
                m_RightHandAttackTrigger.owner = this;
                m_RightHandAttackTrigger.animatorDamageParameter = GameSceneManager.Instance.rightHandAttackParameterName;
            }

            if (m_MouthTrigger)
            {
                m_MouthTrigger.owner = this;
                m_MouthTrigger.animatorDamageParameter = GameSceneManager.Instance.mouthAttackParameterName;
            }

            if (m_Animator)
            {
                AIStateMachineLink[] scripts = m_Animator.GetBehaviours<AIStateMachineLink>();
                foreach (AIStateMachineLink script in scripts)
                {
                    script.stateMachine = this;
                }
            }

            //populate the dictionary according to the states attached to this game object
            AIState[] states = GetComponents<AIState>();
            foreach(AIState state in states)
            {
                if(state != null && !m_States.ContainsKey(state.GetStateType()))
                {
                    m_States[state.GetStateType()] = state;
                    state.SetStateMachine(this);
                    state.Initialize();
                }
            }

            //set the current state
            if(m_States.ContainsKey(m_CurrentStateType))
            {
                m_CurrentState = m_States[m_CurrentStateType];
                m_CurrentState.OnEnterState();
            }
            else
            {
                m_CurrentState = null;
            }

            //statemachine-specific initialization tasks
            Initialize();
        }

        private void Update()
        {
            EarlyUpdateStateMachine();

            if (m_CurrentState && m_UpdateState)
            {
                AIStateType newType = m_CurrentState.UpdateState();
                if (newType != m_CurrentStateType)
                {
                    AIState newState = null;
                    if (m_States.TryGetValue(newType, out newState))
                    {
                        ChangeState(newState);
                    }
                    else if (m_States.TryGetValue(AIStateType.Idle, out newState))
                    {
                        ChangeState(newState);
                        Debug.LogWarning("AI state machine -- the next state cannot be found; going to the idle state...");
                    }
                }
            }

            UpdateStateMachine();
        }

        private void FixedUpdate()
        {
            visualThreat = null;
            audioThreat = null;

            if (m_Target.type != AITargetType.None)
            {
                m_Target.distance = Vector3.Distance(m_Transform.position, m_Target.lastKnownPosition);
            }

            m_IsTargetReached = false;
        }

        private void LateUpdate()
        {
            LateUpdateStateMachine();
        }

        protected void ChangeState(AIState newState)
        {
            m_CurrentState.OnExitState();
            newState.OnEnterState();
            m_CurrentState = newState;
            m_CurrentStateType = newState.GetStateType();
        }

        /// <summary>
        /// returns the current target of the AI
        /// </summary>
        public AITarget GetCurrentTarget()
        {
            return m_Target;
        }

        /// <summary>
        /// sets the current target for the AI
        /// </summary>
        public void SetTarget(ZombieAggravator zombieAggravator, bool setNavAgentTarget = true)
        {
            if (m_IsDead) return;

            float distanceToTarget = Vector3.Distance(zombieAggravator.transform.position, transform.position);
            m_Target.Set(AITargetType.Aggravator, zombieAggravator.aggravatorCollider, zombieAggravator.transform.position, distanceToTarget);

            //move the trigger to where the target is
            if (m_TargetTrigger != null)
            {
                m_TargetTrigger.radius = m_StoppingDistance;
                m_TargetTrigger.transform.position = m_Target.lastKnownPosition;
                m_TargetTrigger.enabled = true;
            }
            else
            {
                Debug.LogWarning("AI state machine -- no target trigger found!");
            }

            if(setNavAgentTarget)
            {
                m_navAgent.destination = m_Target.lastKnownPosition;
            }
        }

        /// <summary>
        /// sets the current target for the AI
        /// </summary>
        public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPos, float distanceToTarget, bool setNavAgentTarget = true)
        {
            m_Target.Set(targetType, targetCollider, targetPos, distanceToTarget);

            //move the trigger to where the target is
            if (m_TargetTrigger != null)
            {
                m_TargetTrigger.radius = m_StoppingDistance;
                m_TargetTrigger.transform.position = m_Target.lastKnownPosition;
                m_TargetTrigger.enabled = true;
            }
            else
            {
                Debug.LogWarning("AI state machine -- no target trigger found!");
            }

            if (setNavAgentTarget)
            {
                m_navAgent.destination = m_Target.lastKnownPosition;
            }
        }

        /// <summary>
        /// sets the current target for the AI
        /// </summary>
        public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPos, float distanceToTarget, float stoppingDistance, bool setNavAgentTarget = true)
        {
            m_Target.Set(targetType, targetCollider, targetPos, distanceToTarget);

            //move the trigger to where the target is
            if (m_TargetTrigger != null)
            {
                m_TargetTrigger.radius = stoppingDistance;
                m_TargetTrigger.transform.position = m_Target.lastKnownPosition;
                m_TargetTrigger.enabled = true;
            }
            else
            {
                Debug.LogWarning("AI state machine -- no target trigger found!");
            }

            if (setNavAgentTarget)
            {
                m_navAgent.destination = m_Target.lastKnownPosition;
            }
        }

        /// <summary>
        /// sets the current target for the AI
        /// </summary>
        public void SetTarget(AITarget target, bool setNavAgentTarget = true)
        {
            m_Target = target;

            //move the trigger to where the target is
            if (m_TargetTrigger != null)
            {
                m_TargetTrigger.radius = m_StoppingDistance;
                m_TargetTrigger.transform.position = m_Target.lastKnownPosition;
                m_TargetTrigger.enabled = true;
            }
            else
            {
                Debug.LogWarning("AI state machine -- no target trigger found!");
            }

            if (setNavAgentTarget)
            {
                m_navAgent.destination = m_Target.lastKnownPosition;
            }
        }

        /// <summary>
        /// clears the current active target
        /// </summary>
        public void ClearTarget()
        {
            m_Target.Clear();

            if (m_TargetTrigger != null)
            {
                m_TargetTrigger.enabled = false;
            }
            else
            {
                Debug.LogWarning("AI state machine -- no target trigger found!");
            }

            m_IsTargetReached = false;
        }

        /*
        private bool FindNearestNavPoint(Vector3 source, out Vector3 result)
        {
            //find a valid nav mesh position closest to our new root position
            NavMeshHit navMeshHit;
            bool posFound = NavMesh.SamplePosition(source, out navMeshHit, 25.0f, NavMesh.AllAreas);

            if (posFound)
            {
                result = navMeshHit.position;
            }
            else
            {
                result = Vector3.positiveInfinity;
            }

            return posFound;
        }
        */

        /// <summary>
        /// called by unity, when the AI's capsule collider just collided with the target trigger
        /// </summary>
        private void OnTriggerEnter( Collider other )
        {
            //if there is no target trigger or what we collided is not this AI's target trigger
            if (!m_TargetTrigger || other != m_TargetTrigger) return;

            //notify the state object
            if (m_CurrentState) m_CurrentState.OnReachDestination();

            m_IsTargetReached = true;
        }

        /// <summary>
        /// called by unity, when the AI's capsule collider is colliding with the target trigger
        /// </summary>
        private void OnTriggerStay(Collider other)
        {
            if (!m_TargetTrigger || other != m_TargetTrigger) return;

            m_IsTargetReached = true;
        }

        /// <summary>
        /// called by unity, when the AI's capsule collider just collided with the target trigger
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            //if there is no target trigger or what we collided is not this AI's target trigger
            if (!m_TargetTrigger || other != m_TargetTrigger) return;

            //notify the state object
            if (m_CurrentState) m_CurrentState.OnLeaveDestination();

            m_IsTargetReached = false;
        }

        /// <summary>
        /// called by the sensor script, when the sensor (in the AI trigger layer) detects any collision with zombie aggravators; 
        /// this method then forwards the event to the current active state
        /// </summary>
        public void OnTriggerEvent(AITriggerEventType type, Collider other)
        {
            if (m_CurrentState) m_CurrentState.OnTriggerEvent(type, other);
        }

        /// <summary>
        /// a method that invokes the state's corresponding handler function
        /// </summary>
        private void OnAnimatorMove()
        {
            if (m_CurrentState) m_CurrentState.OnAnimatorUpdated();
        }

        /// <summary>
        /// a method that invokes the state's corresponding handler function
        /// </summary>
        private void OnAnimatorIK(int layerIndex)
        {
            if (m_CurrentState) m_CurrentState.OnAnimatorIKUpdated();
        }

        /// <summary>
        /// do you want the nav agent to be in charge of the position or rotation of the game object, or neither?
        /// </summary>
        /// <param name="positionUpdate"></param>
        /// <param name="rotationUpdate"></param>
        public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
        {
            if(m_navAgent)
            {
                m_navAgent.updatePosition = positionUpdate;
                m_navAgent.updateRotation = rotationUpdate;
            }
        }

        /// <summary>
        /// called by the state machine behaviors to enable/disable root motion for that specific clip
        /// </summary>
        /// <param name="rootPosition"></param>
        /// <param name="rootRotation"></param>
        public void AddRootMotionRequest(int rootPosition, int rootRotation)
        {
            m_RootPositionRefCount += rootPosition;
            m_RootRotationRefCount += rootRotation;

            UpdateNavAgentControl();
        }

        private void UpdateNavAgentControl()
        {
            //if we are using root position, let the navagent update the position based on the velocity
            //calculated in the animator
            NavAgentControl(useRootPosition, false);
        }

        public virtual void Die()
        {
            if(!m_IsDead)
            {
                //inform listeners
                if(OnAIDeath != null)
                    OnAIDeath();

                //ragdoll the AI
                m_IsDead = true;
                RagDoll();
            }
        }

        /// <summary>
        /// performs the ragdoll, disables nav agent/ main body collider/ animator
        /// </summary>
        public virtual void RagDoll()
        {
            //inform listeners
            if(OnRagdoll != null)
                OnRagdoll();

            m_AIBoneControlType = AIBoneControlType.Ragdoll;

            if (m_navAgent) m_navAgent.enabled = false;
            //turn off the main collider that is attached to the state machine
            //body part colliders will still have collision
            if (m_Collider) m_Collider.enabled = false;
            if (m_Animator) m_Animator.enabled = false;

            m_IsInMeleeRange = false;

            foreach (AIBodyPart bodyPart in m_BodyParts)
            {
                bodyPart.rigidBody.isKinematic = false;
            }
        }

        public void PauseMachine()
        {
            m_UpdateState = false;
            visualThreat = null;
            audioThreat = null;
            m_IsTargetReached = false;
            m_IsInMeleeRange = false;
        }

        public void ResumeMachine()
        {
            m_UpdateState = true;
        }
    }
}