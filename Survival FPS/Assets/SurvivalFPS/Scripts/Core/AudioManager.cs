using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core
{
    public class AudioManager : MonoBehaviour
    {
        private AudioSource m_AudioSource;
        private IEnumerator m_PlayInSequenceRoutine;
        private IEnumerator m_PlayWhileRoutine;

        void Awake()
        {
            AddAudioSource();
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

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
                m_AudioSource.PlayOneShot(clips[i]);
                yield return new WaitForSeconds(clips[i].length + timeBetween);
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