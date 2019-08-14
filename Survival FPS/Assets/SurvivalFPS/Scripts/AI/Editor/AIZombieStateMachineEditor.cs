using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using SurvivalFPS.AI;

namespace SurvivalFPS.ScriptEditor
{
    [CustomEditor(typeof(AIZombieStateMachine))]
    public class AIZombieStateMachineEditor : Editor
    {
        private AIZombieStateMachine zombie;

        private void OnEnable()
        {
            zombie = (AIZombieStateMachine)target;
        }

        private void OnSceneGUI()
        {
            Color color = Handles.color;
            color = Color.grey;
            color.a = 0.3f;

            Handles.color = color;
            Vector3 from = Quaternion.AngleAxis(- zombie.fov / 2, zombie.transform.up) * zombie.transform.forward;
            //zombie sight
            Handles.DrawSolidArc(zombie.transform.position, zombie.transform.up, from, zombie.fov, zombie.sight * zombie.sensorRadius);
            //zombie hearing
            Handles.DrawSolidDisc(zombie.transform.position, zombie.transform.up, zombie.hearing * zombie.sensorRadius);


            if(Application.isPlaying)
            {
                color = Color.red;
                Handles.color = color;
                SphereCollider targetTrigger = zombie.targetTrigger;
                Handles.DrawWireDisc(targetTrigger.transform.position, zombie.transform.up, targetTrigger.radius);

                if (zombie.navAgent.desiredVelocity.sqrMagnitude > 0.0f)
                {
                    Handles.DrawLine(zombie.transform.position, zombie.transform.position + zombie.navAgent.desiredVelocity);
                }

                Handles.DrawSolidDisc(zombie.navAgent.steeringTarget, zombie.transform.up, 0.1f);
                Handles.Label(zombie.navAgent.steeringTarget, "Steering Target");

                
                color = Color.blue;
                Handles.color = color;
                if(zombie.navAgent.velocity.sqrMagnitude > 0.0f)
                {
                    Handles.DrawLine(zombie.transform.position, zombie.transform.position + zombie.navAgent.velocity);
                    Handles.Label(zombie.transform.position + zombie.navAgent.velocity, "current velocity");
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Zombie Memory Queue (oldest to lastest): ", EditorStyles.boldLabel);
                if(zombie.memoryQueue.Count > 0)
                {
                    //memory queue
                    foreach (ZombieAggravator aggravator in zombie.memoryQueue)
                    {
                        EditorGUILayout.ObjectField(aggravator.gameObject, typeof(Object), true);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No recent memories");
                }


                EditorGUILayout.LabelField("Current Potential Threats: ", EditorStyles.boldLabel);
                if(zombie.visualThreat)
                {
                    EditorGUILayout.ObjectField("Visual Threat: ", zombie.visualThreat.gameObject, typeof(Object), true);
                }
                else
                {
                    EditorGUILayout.LabelField("Visual Threat: None");
                }
                if (zombie.audioThreat)
                {
                    EditorGUILayout.ObjectField("Audio Threat: ", zombie.audioThreat.gameObject, typeof(Object), true);
                }
                else
                {
                    EditorGUILayout.LabelField("Audio Threat: None");
                }


                EditorGUILayout.LabelField("Current Target Status: ", EditorStyles.boldLabel);
                if (zombie.GetCurrentTarget().type == AITargetType.Aggravator)
                {
                    EditorGUILayout.ObjectField("Current Target: ", zombie.GetCurrentTarget().collider.gameObject, typeof(Object), true);
                }
                else if (zombie.GetCurrentTarget().type == AITargetType.None)
                {
                    EditorGUILayout.LabelField("Current Target: No target at this time");
                }
                else if (zombie.GetCurrentTarget().type == AITargetType.Waypoint)
                {
                    EditorGUILayout.LabelField("Current Target: Wayooint");
                }

                if (zombie.GetCurrentTarget() != null)
                {
                    EditorGUILayout.LabelField("Is Target Reached: " + zombie.isTargetReached);
                }


                EditorGUILayout.LabelField("Nav Agent Status: ", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Path Status: " + zombie.navAgent.pathStatus.ToString());
                EditorGUILayout.LabelField("Is Path Stale: " + zombie.navAgent.isPathStale);
                EditorGUILayout.LabelField("Has Path: " + zombie.navAgent.hasPath);
                EditorGUILayout.LabelField("Path Pending: " + zombie.navAgent.pathPending);

                EditorGUILayout.LabelField("Animator Status: ", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Use Root Rotation: " + zombie.useRootRotation);
                EditorGUILayout.LabelField("Use Root Position: " + zombie.useRootPosition);


            }
        }
    }
}
