using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SurvivalFPS.Core.FPS
{
    [CustomEditor(typeof(PlayerAnimatorManager))]
    public class PlayerAnimatorManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PlayerAnimatorManager playerAnimatorManager = (PlayerAnimatorManager)target;

            if(Application.isPlaying)
            {
                if (playerAnimatorManager.animators.Count > 0)
                {
                    foreach (KeyValuePair<Animator, bool> pair in playerAnimatorManager.animators)
                    {
                        if (pair.Value)
                            EditorGUILayout.ObjectField("enabled:  ", pair.Key, typeof(Animator), false);
                        else
                            EditorGUILayout.ObjectField("disabled: ", pair.Key, typeof(Animator), false);
                    }
                }
            }
        }
    }
}
