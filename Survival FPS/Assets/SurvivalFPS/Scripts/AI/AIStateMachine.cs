using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SurvivalFPS.Utility;

namespace SurvivalFPS.AI
{
    public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
    public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
    public enum AITriggerEventType { Enter, Stay, Exit }

    public class AITarget
    {
        private AITargetType m_Type;
        private Collider m_Collider;
        private Vector3 m_LastKnownPosition;
        private float m_Distance;
        private float m_Time;

        public AITargetType type { get { return m_Type; } }
        public Collider collider { get { return m_Collider; } }
        public Vector3 lastKnownPosition { get { return m_LastKnownPosition; } }
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
    public abstract class AIStateMachine : MonoBehaviour
    {
        //public fields set by child states each frame
        //they represent the potential targets for the AI
        [HideInInspector] public AITarget VisualThreat = new AITarget();
        [HideInInspector] public AITarget AudioThreat = new AITarget();

        //protected
        protected Dictionary<AIStateType, AIState> m_States = new Dictionary<AIStateType, AIState>();
        protected AITarget m_Target = new AITarget(); //actual target of the AI
        protected AIState m_CurrentState = null;
        protected int m_RootRotationRefCount = 0;
        protected int m_RootPositionRefCount = 0;

        [SerializeField] protected AIStateType m_CurrentStateType = AIStateType.Idle; 
        [SerializeField] protected SphereCollider m_TargetTrigger = null; //the sphere trigger at the AI's target position
        [SerializeField] protected SphereCollider m_SensorTrigger = null; //the sphere trigger representing the AI's sensory range

        [SerializeField] [Range(0.0f, 15.0f)] protected float m_StoppingDistance = 1.0f;

        //component cache
        protected Animator m_Animator = null;
        protected NavMeshAgent m_navAgent = null;
        protected Collider m_Collider = null;   //the collider representing the AI character; it should be in a layer where it can only collides with designated objects
        protected Transform m_Transform = null;

        //public properties
        public Animator animator { get { return m_Animator; } }
        public NavMeshAgent navAgent { get { return m_navAgent; } }
        public AITargetType currentTargetType { get { return m_Target.type; } }

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

        //overwrittable methods
        /// <summary>
        /// this method is coupled to the start method, for additional initialization work
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// this method is called every frame, after the child states are updated
        /// </summary>
        protected virtual void UpdateStateMachine() { }

        private void Awake()
        {
            m_Transform = transform;
            m_Animator = GetComponent<Animator>();
            m_Collider = GetComponent<CapsuleCollider>();
            m_navAgent = GetComponent<NavMeshAgent>();

            //register colliders to the scene manager
            if(GameSceneManager.Instance)
            {
                if (m_Collider) GameSceneManager.Instance.RegisterAIStateMachine(m_Collider.GetInstanceID(), this);
                if (m_SensorTrigger) GameSceneManager.Instance.RegisterAIStateMachine(m_SensorTrigger.GetInstanceID(), this);
                //anything else that may be added
            }
        }

        private void Start()
        {
            //ownership assignments
            if (m_SensorTrigger)
            {
                AISensor sensor = m_SensorTrigger.gameObject.GetComponent<AISensor>();
                sensor.owner = this;
            }

            if (m_Animator)
            {
                AIStateMachineLink[] scripts = m_Animator.GetBehaviours<AIStateMachineLink>();
                foreach (AIStateMachineLink script in scripts)
                {
                    script.stateMachine = this;
                }
            }

            //populate the dictionary according to the states attached
            AIState[] states = GetComponents<AIState>();
            foreach(AIState state in states)
            {
                if(state != null && !m_States.ContainsKey(state.GetStateType()))
                {
                    m_States[state.GetStateType()] = state;
                    state.SetStateMachine(this);
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
            if (!m_CurrentState) return;

            AIStateType newType = m_CurrentState.UpdateState();
            if(newType != m_CurrentStateType)
            {
                AIState newState = null;
                if(m_States.TryGetValue(newType, out newState))
                {
                    ChangeState(newState);
                }
                else if(m_States.TryGetValue(AIStateType.Idle, out newState))
                {
                    ChangeState(newState);
                    Debug.LogWarning("the next state cannot be found; going to the idle state...");
                }
            }

            UpdateStateMachine();
        }

        protected virtual void FixedUpdate()
        {
            //clear all the visual and audio threats in the last frame
            VisualThreat.Clear();
            AudioThreat.Clear();

            if (m_Target.type != AITargetType.None)
            {
                m_Target.distance = Vector3.Distance(m_Transform.position, m_Target.lastKnownPosition);
            }
        }

        private void ChangeState(AIState newState)
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
        public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPos, float distanceToTarget)
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
                Debug.LogWarning("no sphere collider found!");
            }
        }

        /// <summary>
        /// sets the current target for the AI
        /// </summary>
        public void SetTarget(AITargetType targetType, Collider targetCollider, Vector3 targetPos, float distanceToTarget, float stoppingDistance)
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
                Debug.LogWarning("no sphere collider found!");
            }
        }

        /// <summary>
        /// sets the current target for the AI
        /// </summary>
        public void SetTarget(AITarget target)
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
                Debug.LogWarning("no sphere collider found!");
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
                Debug.LogWarning("no sphere collider found!");
            }
        }

        /// <summary>
        /// called by unity, when the AI's capsule collider just collided with any collider that is on the AI entity trigger layer
        /// </summary>
        private void OnTriggerEnter( Collider other )
        {
            //if there is no target trigger or what we collided is not this AI's target trigger
            if (!m_TargetTrigger || other != m_TargetTrigger) return;

            //notify the state object
            if (m_CurrentState) m_CurrentState.OnReachDestination();
        }

        /// <summary>
        /// called by unity, when the AI's capsule collider just left the collision range with any collider that is on the AI entity trigger layer
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            //if there is no target trigger or what we collided is not this AI's target trigger
            if (!m_TargetTrigger || other != m_TargetTrigger) return;

            //notify the state object
            if (m_CurrentState) m_CurrentState.OnLeaveDestination();
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
        }
    }
}