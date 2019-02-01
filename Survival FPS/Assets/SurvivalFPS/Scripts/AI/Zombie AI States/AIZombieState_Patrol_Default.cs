using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SurvivalFPS.AI
{
    public class AIZombieState_Patrol_Default : AIZombieState
    {
        [SerializeField] [Range(0, 3)] private float m_PatrolSpeed = 0.0f;
        [SerializeField] [Range(20.0f, 180.0f)] private float m_TurnSpeed = 50.0f;

        [SerializeField] private AIWaypointNetwork m_Waypoints = null;
        [SerializeField] private bool m_RandomPatrol = false;
        [SerializeField] private int m_CurrentWaypoint = 0;

        [SerializeField] private float m_TurnOnSpotThreshold = 80.0f;

        public override AIStateType GetStateType()
        {
            return AIStateType.Patrol;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            if (m_ZombieStateMachine && !m_ZombieStateMachine.IsDead)
            {
                //update the animator
                //m_ZombieStateMachine.NavAgentControl(true, false);
                m_ZombieStateMachine.speed = m_PatrolSpeed;
                m_ZombieStateMachine.seeking = 0;
                m_ZombieStateMachine.feeding = false;
                m_ZombieStateMachine.attackType = 0;

                //if random patrol is enabled, choose a waypoint from the waypoint network randomly
                //otherwise, continue to the current waypoint
                if (m_ZombieStateMachine.currentTargetType != AITargetType.Waypoint)
                {
                    m_ZombieStateMachine.ClearTarget();

                    if (m_Waypoints)
                    {
                        if (m_RandomPatrol)
                        {
                            m_CurrentWaypoint = Random.Range(0, m_Waypoints.Waypoints.Count);
                        }

                        if (m_CurrentWaypoint < m_Waypoints.Waypoints.Count)
                        {
                            Transform waypoint = m_Waypoints.Waypoints[m_CurrentWaypoint];

                            float distance = Vector3.Distance(m_ZombieStateMachine.transform.position, waypoint.position);
                            m_ZombieStateMachine.SetTarget(AITargetType.Waypoint, null, waypoint.position, distance);
                        }
                    }
                }


                //if we are previously stopped for some reason, resume
                if (m_ZombieStateMachine.navAgent) 
                    m_ZombieStateMachine.navAgent.isStopped = false;
            }

        }

        public override void OnExitState()
        {
            base.OnExitState();
        }

        public override void OnReachDestination()
        {
            base.OnReachDestination();
            if (m_ZombieStateMachine && !m_ZombieStateMachine.IsDead)
            {
                if (m_ZombieStateMachine.currentTargetType == AITargetType.Waypoint)
                {
                    NextWaypoint();
                }
            }
        }

        //head movement
        /*
        public override void OnAnimatorIKUpdated()
        {
            base.OnAnimatorIKUpdated();
            if(m_ZombieStateMachine)
            {
                m_ZombieStateMachine.animator.SetLookAtPosition(m_ZombieStateMachine.GetCurrentTarget().lastKnownPosition + );
                m_ZombieStateMachine.animator.SetLookAtWeight(0.55f);
            }
        }
        */

        public override AIStateType UpdateState()
        {
            if (!m_ZombieStateMachine)
            {
                return AIStateType.Patrol;
            }

            if (m_ZombieStateMachine.visualThreat)
            {
                m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.visualThreat);
                //TODO: flashlight behaviour
                /*
                if (m_ZombieStateMachine.visualThreat.GetType() == typeof(FlashLightAggravator))
                {
                    return AIStateType.Alerted;
                }
                */

                //TODO: Food
                /*
                if (m_ZombieStateMachine.visualThreat.GetType() == typeof(ZombieFood))
                {
                }
                */

                return AIStateType.Pursuit;
            }

            if (m_ZombieStateMachine.audioThreat)
            {
                //if it is not already investigated
                if (!m_ZombieStateMachine.IsTargetRecentlyInvestigated(m_ZombieStateMachine.audioThreat))
                {
                    m_ZombieStateMachine.SetTarget(m_ZombieStateMachine.audioThreat, false);
                    return AIStateType.Alerted;
                }
            }


            //-----handle the rotation of the game object-----

            //large turns are handled by the alerted state
            float angle = Vector3.Angle(m_ZombieStateMachine.transform.forward, (m_ZombieStateMachine.navAgent.steeringTarget - m_ZombieStateMachine.transform.position));
            if (angle > m_TurnOnSpotThreshold)
            {
                return AIStateType.Alerted;
            }

            //if the path is still pending
            if (m_ZombieStateMachine.navAgent.pathPending)
            {
                m_ZombieStateMachine.speed = 0;
                return AIStateType.Patrol;
            }
            else
            {
                m_ZombieStateMachine.speed = m_PatrolSpeed;
            }

            //move onto the next waypoint if there is something wrong with the path 
            if (m_ZombieStateMachine.navAgent.isPathStale || m_ZombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                NextWaypoint();
            }

            return AIStateType.Patrol;
        }

        private void NextWaypoint()
        {
            if (m_RandomPatrol && m_Waypoints.Waypoints.Count > 1)
            {
                int oldWatpoint = m_CurrentWaypoint;
                while (m_CurrentWaypoint == oldWatpoint)
                {
                    m_CurrentWaypoint = Random.Range(0, m_Waypoints.Waypoints.Count);
                }
            }
            else
            {
                m_CurrentWaypoint = (m_CurrentWaypoint == m_Waypoints.Waypoints.Count - 1) ? 0 : m_CurrentWaypoint + 1;
            }

            if (m_Waypoints.Waypoints[m_CurrentWaypoint])
            {
                Transform newWaypoint = m_Waypoints.Waypoints[m_CurrentWaypoint];
                float distance = Vector3.Distance(m_ZombieStateMachine.transform.position, newWaypoint.position);

                m_ZombieStateMachine.SetTarget(AITargetType.Waypoint, null, newWaypoint.position, distance);
            }
        }
    }
}
