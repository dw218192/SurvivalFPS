using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.Audio;

namespace SurvivalFPS.AI
{
    public class AIFootstepPlayer : AIStateMachineLink
    {
        [SerializeField] private AudioCollection m_AudioCollection = null;
        [SerializeField] private int m_BankNum = 0;

        private bool m_HasPlayedPrevFrame = false; //avoid playing the same sound multiple times
        private AudioManager m_AudioManager;

        [SerializeField] [Range(0.1f, 0.4f)] private float m_RayCastDist = 0.1f;
        private Transform m_LeftFoot;
        private Transform m_RightFoot;
        private GameSceneManager m_GameSceneManager;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!m_LeftFoot) m_LeftFoot = m_StateMachine.leftFootTransform;
            if (!m_RightFoot) m_RightFoot = m_StateMachine.rightFootTransform;
            if (!m_GameSceneManager) m_GameSceneManager = GameSceneManager.Instance;
            if (!m_AudioManager) m_AudioManager = AudioManager.Instance;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != 0 && Mathf.Approximately(animator.GetLayerWeight(layerIndex), 0.0f)) return;
            if (m_StateMachine == null) return;

            if(FootRaycast(m_LeftFoot) || FootRaycast(m_RightFoot))
            {
                if(!m_HasPlayedPrevFrame)
                {
                    m_AudioManager.PlayOneShotSound(m_AudioCollection.audioGroup,
                                                    m_AudioCollection[m_BankNum],
                                                    m_StateMachine.transform.position,
                                                    m_AudioCollection.volume,
                                                    m_AudioCollection.spatialBlend,
                                                    m_AudioCollection.priority);
                    
                    m_HasPlayedPrevFrame = true;
                }
            }
            else
            {
                m_HasPlayedPrevFrame = false;
            }
        }

        private bool FootRaycast(Transform footTransform)
        {
            /*
            Debug.DrawLine(footTransform.position, footTransform.position + Vector3.down * m_RayCastDist, Color.green);
            Debug.Break();
            */

            Ray ray = new Ray(footTransform.position, Vector3.down);
            return Physics.Raycast(ray, m_RayCastDist, m_GameSceneManager.geometryLayerMask);
        }
    }
}
