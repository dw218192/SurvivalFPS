using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Core.Audio;

namespace SurvivalFPS.AI
{
    public class LayeredAudioPlayer : AIStateMachineLink
    {
        [SerializeField] private AudioCollection m_Collection;
        [SerializeField] private bool m_Looping = false;
        [SerializeField] private int m_BankNum = 0;
        [SerializeField] private byte m_Layer;
        [SerializeField] private bool m_StartOverOnEnter = true;
        [SerializeField] private bool m_StopOnExit = true;

        private static AudioManager.ILayeredAudioSource m_LayeredAudioSource;
        private AudioManager m_AudioManager;
        private bool m_Played = false;
        private float m_PrevLayerWeight = 0.0f;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //first time entering this state
            if (m_StateMachine && !m_AudioManager)
                m_StateMachine.RegisterRagdollEvent(OnRagdoll);

            if(!m_AudioManager)
                m_AudioManager = AudioManager.Instance;
            
            if (m_LayeredAudioSource == null)
                m_LayeredAudioSource = m_AudioManager.RegisterLayeredAudioSource(m_StateMachine.gameObject, 10, true);

            float layerWeight = animator.GetLayerWeight(layerIndex);

            // Get the layer weight and only play for weighted layer
            if (layerIndex != 0 && Mathf.Approximately(layerWeight, 0.0f)) return;

            //play the sound if
            //it's the first time entering this state
            //or we are told to start over on entering
            if (m_Collection != null && (!m_Played || m_StartOverOnEnter))
            {
                m_LayeredAudioSource.Play(m_Collection, m_BankNum, m_Layer, m_Looping);
                m_Played = true;
            }

            // Store layer weight to detect changes mid animation
            m_PrevLayerWeight = layerWeight;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StateMachine) return;

            // Get the current layer weight
            float layerWeight = animator.GetLayerWeight(layerIndex);

            // If its changes we might need to start or stop the audio layer assigned to it
            if (!Mathf.Approximately(layerWeight, m_PrevLayerWeight) && m_Collection != null)
            {
                if (layerWeight > 0.0f && !m_Played)
                {
                    m_LayeredAudioSource.Play(m_Collection, m_BankNum, m_Layer, m_Looping);
                    m_Played = true;
                }
                else
                {
                    m_LayeredAudioSource.Stop(m_Layer);
                }
            }

            m_PrevLayerWeight = layerWeight;
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_StopOnExit)
            {
                m_LayeredAudioSource.Stop(m_Layer);
            }
        }

        private void OnRagdoll()
        {
            m_LayeredAudioSource.Stop(m_Layer);
        }
    }
}
