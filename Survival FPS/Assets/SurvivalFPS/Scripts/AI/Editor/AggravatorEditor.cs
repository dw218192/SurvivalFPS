using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SurvivalFPS.AI
{
    [CustomEditor(typeof(AggravatorData))]
    public class AggravatorEditor : Editor
    {
        private SerializedProperty m_OverridingListProperty;

        private void OnEnable()
        {
            m_OverridingListProperty = serializedObject.FindProperty("m_OverridingList");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("this aggravator will override the following ones");
            EditorGUILayout.LabelField("i.e. the zombie will prioritize this aggravator over the ones listed below: ");
            DrawDefaultInspector();
        }
    }
}