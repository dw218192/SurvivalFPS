using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentRootMotion : MonoBehaviour {

    private NavMeshAgent m_Agent;
    public AIWaypointNetwork Network;
    public int CurrentIndex = 0;
    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public bool IsOnOffMeshLink = false;
    public bool IsLinkBeingHandled = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve;
    public Animator AnimationController;
    private float m_OriginalMaxSpeed = 0.0f;
    private float m_SmoothAngle = 0.0f;
    public bool MixMode = false;

    // Use this for initialization
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        AnimationController = GetComponent<Animator>();
        SetNextDestination(false);
        m_Agent.updateRotation = false;

        if (m_Agent) m_OriginalMaxSpeed = m_Agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        HasPath = m_Agent.hasPath;
        PathPending = m_Agent.pathPending;
        PathStale = m_Agent.isPathStale;
        PathStatus = m_Agent.pathStatus;
        IsOnOffMeshLink = m_Agent.isOnOffMeshLink;

        Vector3 localDesiredVel = transform.InverseTransformVector(m_Agent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVel.x, localDesiredVel.z) * Mathf.Rad2Deg;

        m_SmoothAngle = Mathf.MoveTowardsAngle(m_SmoothAngle, angle, 80.0f * Time.deltaTime);
        //float speed = Vector3.Dot(Vector3.forward, localDesiredVel);
        float speed = localDesiredVel.z;

        AnimationController.SetFloat("Angle", m_SmoothAngle);
        AnimationController.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        if(m_Agent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            if(!MixMode || (MixMode && Mathf.Abs(angle) < 80.0f && AnimationController.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")))
            {
                Quaternion lookRotation = Quaternion.LookRotation(m_Agent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 50.0f * Time.deltaTime);
            }
        }

        if ((m_Agent.remainingDistance <= m_Agent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }

        else if (m_Agent.isPathStale)
        {
            SetNextDestination(false);
        }
    }

    private void SetNextDestination(bool increment)
    {
        if (Network == null) return;
        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        int nextWaypoint = (CurrentIndex + incStep >= Network.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        nextWaypointTransform = Network.Waypoints[nextWaypoint];

        if (nextWaypointTransform != null)
        {
            CurrentIndex = nextWaypoint;
            m_Agent.destination = nextWaypointTransform.position;
        }
    }

    public void OnAnimatorMove()
    {
        if (MixMode && !AnimationController.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
            transform.rotation = AnimationController.rootRotation;

        m_Agent.velocity = AnimationController.deltaPosition / Time.deltaTime;
    }
}
