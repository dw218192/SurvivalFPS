using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

using SurvivalFPS.Core.FPS;
using SurvivalFPS.Core.Audio;

namespace SurvivalFPS.Core.Weapon
{
    /// <summary>
    /// This anim state machine behavior is responsible for playing weapon audio files
    /// depending on the anim state the weapon is in. 
    /// Note that the audiocollection is organized in the following way: 
    /// collection[0][..] -- bringup sound(s)  
    /// collection[1][..] -- fire sound(s)  
    /// collection[2][..] -- reload sound(s)  
    /// </summary>
    public class WeaponAudioPlayer : PlayerStateMachineLink
    {
        [SerializeField] private AnimatorAudioChannel m_Channel = AnimatorAudioChannel.AudioChannel1;

        private WeaponConfig m_CurrentWeapon;
        private AudioCollection m_AudioCollection;
        private AudioManager m_AudioManager;
        private ulong m_AudioSourceID;
        private GameSceneManager m_GameSceneManager;

        //track the clip index in the audio bank
        private int m_CurSoundIndex;

        //the animator parameter that should be set by an animation curve
        private int m_ChannelAnimParamHash = -1;
        private int m_PreviousChannel;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(m_playerWeaponController)
            {
                m_playerWeaponController.RegisterWeaponChangeEvent(OnPlayerWeaponChanged);
                m_CurrentWeapon = m_playerWeaponController.currentWeapon;

                if(m_CurrentWeapon)
                    m_AudioCollection = m_CurrentWeapon.audioCollection;
            }

            if(!m_AudioManager)
            {
                m_AudioManager = AudioManager.Instance;
            }

            if(!m_GameSceneManager)
            {
                m_GameSceneManager = GameSceneManager.Instance;
            }

            if(m_ChannelAnimParamHash == -1)
            {
                m_ChannelAnimParamHash = Animator.StringToHash(m_Channel.ToString());
            }

            m_CurSoundIndex = 0;
            m_PreviousChannel = 0;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (layerIndex != 0 && Mathf.Approximately(animator.GetLayerWeight(layerIndex), 0.0f)) return;
            if (m_playerWeaponController == null) return;

            int channel = Mathf.FloorToInt(animator.GetFloat(m_ChannelAnimParamHash));

            //use the leading edge of the channel param
            if (m_PreviousChannel != channel && channel > 0)
            {
                AudioClip clip = null;

                //play different audio files according to what animation is being played
                if(stateInfo.shortNameHash == m_GameSceneManager.bringUpStateNameHash)
                {
                    PlayClipHelper(out clip, 0, m_CurSoundIndex);
                }
                else if(stateInfo.shortNameHash == m_GameSceneManager.fireStateNameHash)
                {
                    PlayClipHelper(out clip, 1, m_CurSoundIndex);
                }
                else if (stateInfo.shortNameHash == m_GameSceneManager.reloadStateNameHash)
                {
                    PlayClipHelper(out clip, 2, m_CurSoundIndex);
                }

                //increment the index in the clip bank if there're more clips
                if (clip) m_CurSoundIndex++;
            }

            m_PreviousChannel = channel;
        }

        private void PlayClipHelper(out AudioClip clip, int bankIndex, int clipIndex)
        {
            clip = m_AudioCollection[bankIndex, clipIndex];

            if (clip)
            {
                m_AudioSourceID = m_AudioManager.PlayOneShotSound(
                    m_AudioCollection.audioGroup,
                    clip,
                    m_playerWeaponController.transform.position,
                    m_AudioCollection.volume,
                    m_AudioCollection.spatialBlend,
                    m_AudioCollection.priority);
            }
        }

        private void OnPlayerWeaponChanged(WeaponConfig currentWeapon)
        {
            //TODO stop the sound
        }
    }
}
