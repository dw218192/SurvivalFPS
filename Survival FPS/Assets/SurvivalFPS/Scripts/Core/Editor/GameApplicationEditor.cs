using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SurvivalFPS.ScriptEditor
{
    [CustomEditor(typeof(GameApplication))]
    public class GameApplicationEditor : Editor
    {
        private bool m_ToggleProjectLayer = false;
        public override void OnInspectorGUI()
        {
            m_ToggleProjectLayer = GUILayout.Toggle(m_ToggleProjectLayer, "show loaded project layer");

            if (m_ToggleProjectLayer)
            {
                EditorGUILayout.LabelField("loaded project layer: ");

                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = typeof(GameApplication.LayerData).GetFields(bindingFlags);

                foreach (FieldInfo field in fields)
                {
                    EditorGUILayout.LabelField(((GameApplication.LayerData)field.GetValue(null)).ToString());
                }
            }

            DrawDefaultInspector();
        }
    }
}
