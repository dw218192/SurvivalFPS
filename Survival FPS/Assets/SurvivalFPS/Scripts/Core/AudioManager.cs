using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core
{
    //TODO remove surplus sound sources?
    public class AudioManager : MonoBehaviour
    {
        private AudioSource m_AudioSource;
        private IEnumerator m_PlayInSequenceRoutine;

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

        public void PlayOneShot(AudioClip clip)
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
        public void PlayRandom (params AudioClip[] clips)
        {
            if (clips.Length > 0)
            {
                int randomIndex = Random.Range(0, clips.Length);
                PlayOneShot(clips[randomIndex]);
            }
        }
        public void PlayDelayed(AudioClip clip, float time)
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
    }
}