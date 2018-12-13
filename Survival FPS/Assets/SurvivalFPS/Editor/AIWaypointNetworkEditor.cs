using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;


[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AIWaypointNetwork network = this.target as AIWaypointNetwork;

        network.DisplayMode = (PathDisplayMode) EditorGUILayout.EnumPopup("Display Mode", network.DisplayMode);

        if(network.DisplayMode == PathDisplayMode.Paths)
        {
            network.UIStart = EditorGUILayout.IntSlider("Start Waypoint Index", network.UIStart, 0, network.Waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("End Waypoint Index", network.UIEnd, 0, network.Waypoints.Count - 1);
        }

        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        AIWaypointNetwork network = this.target as AIWaypointNetwork;

        for (int i = 0; i < network.Waypoints.Count; i ++)
        {
            Transform waypoint = network.Waypoints[i];

            if(waypoint)
            {
                GUIStyle style = GUIStyle.none;
                style.normal.textColor = Color.white;
                Handles.Label(waypoint.position, "Waypoint" + i.ToString(), style);
            }
        }
        
        if(network.DisplayMode == PathDisplayMode.Connections)
        {
            Vector3[] linePoints = new Vector3[network.Waypoints.Count + 1];

            for (int i = 0; i < network.Waypoints.Count + 1; i++)
            {
                Transform waypoint;

                if (i < network.Waypoints.Count)
                {
                    waypoint = network.Waypoints[i];
                }
                else
                {
                    waypoint = network.Waypoints[0];
                }

                if (waypoint)
                {
                    GUIStyle style = GUIStyle.none;
                    style.normal.textColor = Color.white;
                    Handles.Label(waypoint.position, "Waypoint" + i.ToString(), style);

                    linePoints[i] = waypoint.position;
                }
                else
                {
                    linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
                }
            }

            Handles.DrawPolyLine(linePoints);
        }

        else if (network.DisplayMode == PathDisplayMode.Paths)
        {
            NavMeshPath path = new NavMeshPath();

            if (network.Waypoints[network.UIStart] && network.Waypoints[network.UIEnd])
            {
                Vector3 from = network.Waypoints[network.UIStart].position;
                Vector3 to = network.Waypoints[network.UIEnd].position;
                NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
                Handles.color = Color.yellow;
                Handles.DrawPolyLine(path.corners);
            }
            else
            {
                Debug.LogWarning("both start and end must not be null!");
            }
        }
    }
}
