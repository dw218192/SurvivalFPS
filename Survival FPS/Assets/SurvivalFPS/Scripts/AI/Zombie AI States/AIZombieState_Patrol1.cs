using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Patrol1 : AIZombieState
    {
        [SerializeField] private AIWaypointNetwork m_Waypoints = null;
        [SerializeField] private bool m_RandomPatrol = false;
        [SerializeField] private int m_CurrentWaypoint = 0;

        [SerializeField] private float m_TurnOnSpotThreshold = 80.0f;
        [SerializeField] private float m_SlerpSpeed = 50.0f;
        [SerializeField] [Range(0,3)] private float m_PatrolSpeed = 0.0f;

        public override AIStateType GetStateType()
        {
            return AIStateType.Patrol;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            if (m_ZombieStateMachine)
            {
                //update the animator
                m_ZombieStateMachine.NavAgentControl(true, false);
                m_ZombieStateMachine.speed = m_PatrolSpeed;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = 0;

                //if random patrol is enabled, choose a waypoint from the waypoint network randomly
                //otherwise, continue to the current waypoint
                if(m_ZombieStateMachine.currentTargetType != AITargetType.Waypoint)
                {
                    m_ZombieStateMachine.ClearTarget();

                    if(m_Waypoints)
                    {
                        if(m_RandomPatrol)
                        {
                            m_CurrentWaypoint = Random.Range(0, m_Waypoints.Waypoints.Count);
                        }

                        if(m_CurrentWaypoint < m_Waypoints.Waypoints.Count)
                        {
                            Transform waypoint = m_Waypoints.Waypoints[m_CurrentWaypoint];
                            if(m_Waypoints != null)
                            {
                                float distance = Vector3.Distance(m_ZombieStateMachine.transform.position, waypoint.position);
                                m_ZombieStateMachine.SetTarget(AITargetType.Waypoint, null, waypoint.position, distance);
                            }
                        }
                    }
                }
            }

            //if we are previously stopped for some reason, resume
            if(m_ZombieStateMachine.navAgent.isStopped) m_ZombieStateMachine.navAgent.isStopped = false;
        }

        public override void OnExitState()
        {
            base.OnExitState();
        }

        public override void OnReachDestination()
        {
            base.OnReachDestination();
            if (m_ZombieStateMachine)
            {
                if(m_ZombieStateMachine.currentTargetType == AITargetType.Waypoint)
                {
                    NextWaypoint();
                }
            }
        }

        public override void OnAnimatorIKUpdated()
        {
            base.OnAnimatorIKUpdated();
            if(m_ZombieStateMachine)
            {
                m_ZombieStateMachine.animator.SetLookAtPosition(m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition);
                m_ZombieStateMachine.animator.SetLookAtWeight(0.55f);
            }
        }

        public override AIStateType UpdateState()
        {
            if (m_ZombieStateMachine)
            {
                if (m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                }

                if (m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }

                if (m_ZombieStateMachine.AudioThreat.type == AITargetType.Audio)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }

                if (m_ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.VisualThreat);

                    //if the food is within range, go after it if we are hunger enough
                    if((1.0f - m_ZombieStateMachine.satisfaction) > (m_ZombieStateMachine.VisualThreat.distance / m_ZombieStateMachine.sensorRadius))
                    {
                        m_StateMachine.SetTarget(m_StateMachine.VisualThreat);
                        return AIStateType.Pursuit;
                    }
                }
            }

            //handle the rotation of the game object
            //large turns are handled by the alerted state
            float angle = Vector3.Angle(m_ZombieStateMachine.transform.forward, (m_ZombieStateMachine.navAgent.steeringTarget - m_ZombieStateMachine.transform.position));
            if(angle > m_TurnOnSpotThreshold)
            {
                return AIStateType.Alerted;
            }

            //manually rotate the zombie using slerp if root rotation is not enabled
            if(!m_ZombieStateMachine.useRootRotation)
            {
                if(m_ZombieStateMachine.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
                {
                    Quaternion newRot = Quaternion.LookRotation(m_ZombieStateMachine.navAgent.desiredVelocity);
                    m_ZombieStateMachine.transform.rotation = Quaternion.RotateTowards(m_ZombieStateMachine.transform.rotation, newRot, Time.deltaTime * m_SlerpSpeed);
                }
            }

            //move onto the next waypoint if there is something wrong with the path or we do not have an active path now
            if(     m_ZombieStateMachine.navAgent.isPathStale 
                || !m_ZombieStateMachine.navAgent.hasPath
                ||  m_ZombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete
              )
            {
                NextWaypoint();
            }

            return AIStateType.Patrol;
        }

        private void NextWaypoint()
        {
            if(m_RandomPatrol && m_Waypoints.Waypoints.Count > 1)
            {
                int oldWatpoint = m_CurrentWaypoint;
                while(m_CurrentWaypoint == oldWatpoint)
                {
                    m_CurrentWaypoint = Random.Range(0, m_Waypoints.Waypoints.Count);
                }
            }
            else
            {
                m_CurrentWaypoint = m_CurrentWaypoint == m_Waypoints.Waypoints.Count - 1 ? 0 : m_CurrentWaypoint + 1;
            }

            if(m_Waypoints.Waypoints[m_CurrentWaypoint])
            {
                Transform newWaypoint = m_Waypoints.Waypoints[m_CurrentWaypoint];
                float distance = Vector3.Distance(m_ZombieStateMachine.transform.position, newWaypoint.position);

                m_ZombieStateMachine.SetTarget(AITargetType.Waypoint, null, newWaypoint.position, distance);
                m_ZombieStateMachine.navAgent.destination = newWaypoint.position;
            }
        }
    }

}
