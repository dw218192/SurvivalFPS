using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.AI;

namespace SurvivalFPS.Core.Audio
{
    public enum AnimatorAudioChannel { AudioChannel1, AudioChannel2, AudioChannel3, AudioChannel4 };

    public class AudioCollectionPlayer : AIStateMachineLink
    {
        [SerializeField] private AnimatorAudioChannel m_AnimatorAudioChannel = AnimatorAudioChannel.AudioChannel1;
        [SerializeField] private AudioCollection m_AudioCollection = null;

        //private vars
        private int m_PreviousChannel; //avoid playing the same sound multiple times
        private AudioManager m_AudioManager;
        private int m_ChannelAnimParamHash = -1; //the channel param name hash in the animator

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_AudioManager = AudioManager.Instance;
            m_PreviousChannel = 0;

            if(m_ChannelAnimParamHash == -1)
            {
                m_ChannelAnimParamHash = Animator.StringToHash(m_AnimatorAudioChannel.ToString());
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != 0 && Mathf.Approximately(animator.GetLayerWeight(layerIndex), 0.0f)) return;
            if (m_StateMachine == null) return;

            int channel = Mathf.FloorToInt(animator.GetFloat(m_ChannelAnimParamHash));

            //use the leading edge
            if(m_PreviousChannel != channel && channel > 0)
            {
                int bank = Mathf.Max(0, Mathf.Min(channel - 1, m_AudioCollection.bankCount - 1));
                m_AudioManager.PlayOneShotSound(m_AudioCollection.audioGroup,
                                                m_AudioCollection[bank],
                                                m_StateMachine.transform.position,
                                                m_AudioCollection.volume,
                                                m_AudioCollection.spatialBlend,
                                                m_AudioCollection.priority);
            }

            m_PreviousChannel = channel;
        }
    }
}