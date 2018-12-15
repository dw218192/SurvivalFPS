using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{
    public NavMeshAgent Agent;
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

	// Use this for initialization
	void Start ()
    {
        Agent = GetComponent<NavMeshAgent>();
        AnimationController = GetComponent<Animator>();
        SetNextDestination(false);
        if (Agent) m_OriginalMaxSpeed = Agent.speed;

    }
	
	// Update is called once per frame
	void Update ()
    {
        HasPath = Agent.hasPath;
        PathPending = Agent.pathPending;
        PathStale = Agent.isPathStale;
        PathStatus = Agent.pathStatus;
        IsOnOffMeshLink = Agent.isOnOffMeshLink;
        /*
        if (!HasPath && !PathPending || PathStatus == NavMeshPathStatus.PathInvalid)
            SetNextDestination(true);
        */

        /*
        if (IsOnOffMeshLink && !IsLinkBeingHandled)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }
        */

        if ((Agent.remainingDistance <= Agent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }

        else if (Agent.isPathStale)
        {
            SetNextDestination(false);
        }

        int turnOnSpot = 0;

        Vector3 cross = Vector3.Cross(Agent.velocity.normalized, Agent.desiredVelocity.normalized); //sin value of the angle between two vectors
        float turnAmount = cross.y < 0 ? -cross.magnitude * 2.32f : cross.magnitude * 2.32f;
        
        Debug.DrawLine(Agent.transform.position, Agent.transform.position + Agent.velocity, Color.red);
        Debug.DrawLine(Agent.transform.position, Agent.transform.position + Agent.desiredVelocity, Color.blue);

        if(Vector3.Angle(transform.forward, Agent.desiredVelocity) > 100.0f)
        {
            Agent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(turnAmount);
            AnimationController.SetInteger("TurnOnSpot", turnOnSpot);
        }
        else
        {
            Agent.speed = m_OriginalMaxSpeed;
            turnOnSpot = 0;
            AnimationController.SetInteger("TurnOnSpot", turnOnSpot);
            AnimationController.SetFloat("Vertical", Agent.desiredVelocity.magnitude, 1.0f, Time.deltaTime);
            AnimationController.SetFloat("Horizontal", turnAmount, 0.5f, Time.deltaTime);
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
            Agent.destination = nextWaypointTransform.position;
        }
    }

    /*
    IEnumerator Jump(float duration)
    {
        if (!IsLinkBeingHandled) IsLinkBeingHandled = true;
        OffMeshLinkData offMeshLinkData = Agent.currentOffMeshLinkData;
        Vector3 startPos = Agent.transform.position;
        Vector3 endPos = offMeshLinkData.endPos + Agent.baseOffset * Vector3.up;
        float time = 0.0f;

        while(time <= duration)
        {
            float t = time / duration;
            
            Agent.transform.position = Vector3.Lerp(startPos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;
            yield return null;
        }

        Agent.CompleteOffMeshLink();
        IsLinkBeingHandled = false;
    }
    */
}
