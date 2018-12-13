using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
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

	// Use this for initialization
	void Start ()
    {
        Agent = GetComponent<NavMeshAgent>();
        SetNextDestination(false);
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

        if (IsOnOffMeshLink && !IsLinkBeingHandled)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }

        if ((Mathf.Approximately(Agent.remainingDistance, Agent.stoppingDistance) && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid)
            SetNextDestination(true);

        else if (Agent.isPathStale)
            SetNextDestination(false);
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
}
