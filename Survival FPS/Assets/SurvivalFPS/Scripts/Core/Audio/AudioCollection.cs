using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SurvivalFPS.Core.Audio
{
    [CreateAssetMenu(menuName = "SurvivalFPS/Audio/AudioCollection")]
    public class AudioCollection : ScriptableObject
    {
        [Serializable]
        private class ClipBank
        {
            public string ClipBankName;
            public List<AudioClip> Clips = new List<AudioClip>();
            [HideInInspector] public AudioClip LastClipPlayed;
            public bool AllowConsecutiveSameClip = false;
        }

        [SerializeField] private string m_AudioGroup = string.Empty;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Volume = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_SpatialBlend = 1.0f;
        [SerializeField] [Range(0, 255)] private byte m_Priority = 128;
        [SerializeField] List<ClipBank> m_AudioClipBanks = new List<ClipBank>();

        //true if the user allows the collection to choose the same clip as the last one within the same bank

        public string audioGroup { get { return m_AudioGroup; } }
        public float volume { get { return m_Volume; } }
        public float spatialBlend { get { return m_SpatialBlend; } }
        public byte priority { get { return m_Priority; } }
        public int bankCount { get { return m_AudioClipBanks.Count; }}

        //random selection
        public AudioClip this[int i]
        {
            get
            {
                if (m_AudioClipBanks == null || m_AudioClipBanks.Count <= i) 
                    return null;
                if (m_AudioClipBanks[i].Clips.Count == 0) 
                    return null;

                ClipBank clipBank = m_AudioClipBanks[i];
                List<AudioClip> clipList = clipBank.Clips;

                AudioClip clip;
                if (!clipBank.AllowConsecutiveSameClip && clipBank.Clips.Count > 1)
                {
                    do
                    {
                        clip = clipList[UnityEngine.Random.Range(0, clipList.Count)];
                    }
                    while (m_AudioClipBanks[i].LastClipPlayed == clip);

                    m_AudioClipBanks[i].LastClipPlayed = clip;
                }
                else
                {
                    clip = clipList[UnityEngine.Random.Range(0, clipList.Count)];
                }

                return clip;
            }
        }

        //retrieve a specific clip
        public AudioClip this[int i, int j]
        {
            get
            {
                if (m_AudioClipBanks == null || m_AudioClipBanks.Count <= i)
                    return null;
                if (m_AudioClipBanks[i].Clips.Count == 0 || m_AudioClipBanks[i].Clips.Count <= j)
                    return null;

                return m_AudioClipBanks[i].Clips[j];
            }
        }
    }
}