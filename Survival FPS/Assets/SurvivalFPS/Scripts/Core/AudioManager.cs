using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core
{
    public class TrackInfo
    {
        public string Name;
        public AudioMixerGroup Group;
        public IEnumerator TrackFaderRoutine;
    }

    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [SerializeField] AudioMixer m_Mixer = null;
        [SerializeField] Dictionary<string, TrackInfo> m_Tracks = new Dictionary<string, TrackInfo>();

        private AudioSource m_AudioSource;
        private IEnumerator m_PlayInSequenceRoutine;
        private IEnumerator m_PlayWhileRoutine;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            AddAudioSource();

            if (!m_Mixer) return;

            AudioMixerGroup[] groups = m_Mixer.FindMatchingGroups(string.Empty);

            foreach (AudioMixerGroup group in groups)
            {
                TrackInfo trackInfo = new TrackInfo();
                trackInfo.Name = group.name;
                trackInfo.Group = group;
                trackInfo.TrackFaderRoutine = null;
                m_Tracks[group.name] = trackInfo;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public float GetTrackVolume(string name)
        {
            if (!m_Mixer) return float.MinValue;
            TrackInfo trackInfo;

            if (m_Tracks.TryGetValue(name, out trackInfo))
            {
                float volume;
                m_Mixer.GetFloat(name, out volume);
                return volume;
            }

            return float.MinValue;
        }

        public AudioMixerGroup GetAudioGroupByName(string name)
        {
            TrackInfo trackInfo;

            if (m_Tracks.TryGetValue(name, out trackInfo))
            {
                return trackInfo.Group;
            }

            return null;
        }

        public void SetTrackVolume(string name, float volume, float fadeTime = 0.0f)
        {
            if (!m_Mixer) return;
            TrackInfo trackInfo;

            if(m_Tracks.TryGetValue(name, out trackInfo))
            {
                if (trackInfo.TrackFaderRoutine != null) StopCoroutine(trackInfo.TrackFaderRoutine);

                if (fadeTime == 0.0f)
                {
                    m_Mixer.SetFloat(name, volume);
                }
                else
                {
                    trackInfo.TrackFaderRoutine = _setTrackVolumeRoutine(name, volume, fadeTime);
                }
            }
        }

        protected IEnumerator _setTrackVolumeRoutine(string name, float volume, float fadeTime)
        {
            float startVolume = 0.0f;
            float time = 0.0f;
            m_Mixer.GetFloat(name, out startVolume);

            while(time<fadeTime)
            {
                time += Time.unscaledDeltaTime;
                m_Mixer.SetFloat(name, Mathf.Lerp(startVolume, volume, time / fadeTime));
                yield return null;
            }

            m_Mixer.SetFloat(name, volume);
        }

        private void AddAudioSource()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
        }

        public void Play(AudioClip clip, bool loop)
        {
            if (clip)
            {
                m_AudioSource.clip = clip;
                m_AudioSource.loop = loop;
                m_AudioSource.Play();
            }
        }

        public void PlayOneShot (AudioClip clip)
        {
            if(clip)
            {
                m_AudioSource.PlayOneShot(clip);
            }
        }

        public void PlayInSequence (float timeBetween, params AudioClip[] clips)
        {
            if (clips.Length == 0 || timeBetween <= 0.0f) return;

            if(m_PlayInSequenceRoutine == null)
            {
                m_PlayInSequenceRoutine = _PlayInSequence(timeBetween, clips);
                StartCoroutine(m_PlayInSequenceRoutine);
            }
            else
            {
                Debug.LogWarning("there are already sounds being played in sequence");
            }
        }

        /// <summary>
        /// play a random clip from an array of clips
        /// </summary>
        /// <param name="clips">clips to select from</param>
        public void PlayRandom (params AudioClip[] clips)
        {
            if (clips.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, clips.Length);
                PlayOneShot(clips[randomIndex]);
            }
        }

        /// <summary>
        /// play a random clip from an array of clips, while the condition is true
        /// </summary>
        /// <param name="clips">clips to select from</param>
        public void PlayRandom (Func<bool> predicate, params AudioClip[] clips)
        {
            if (clips.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, clips.Length);
                PlayWhile(clips[randomIndex], predicate);
            }
        }

        /// <summary>
        /// play while the supplied predicate is true
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        public void PlayWhile (AudioClip clip, Func<bool> predicate)
        {
            Play(clip, false);

            m_PlayWhileRoutine = _PlayWhileRoutine(predicate);
            StartCoroutine(m_PlayWhileRoutine);
        }

        public void PlayDelayed (AudioClip clip, float time)
        {
            if (clip)
            {
                m_AudioSource.clip = clip;
                m_AudioSource.PlayDelayed(time);
            }
        }

        //private coroutines
        private IEnumerator _PlayInSequence(float timeBetween, AudioClip[] clips)
        {
            for (int i = 0; i < clips.Length; i ++)
            {
                if(clips[i] != null)
                {
                    m_AudioSource.PlayOneShot(clips[i]);
                    yield return new WaitForSeconds(clips[i].length + timeBetween);
                }
            }

            m_PlayInSequenceRoutine = null;
        }

        private IEnumerator _PlayWhileRoutine(Func<bool> predicate)
        {
            while(true)
            {
                if(!predicate() || !m_AudioSource.isPlaying && predicate())
                {
                    m_AudioSource.Stop();
                    m_AudioSource.clip = null;

                    yield break;
                }

                yield return null;
            }

            m_PlayWhileRoutine = null;
        }
    }
}