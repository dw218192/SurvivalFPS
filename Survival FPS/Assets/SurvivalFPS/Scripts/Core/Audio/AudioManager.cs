using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

using SurvivalFPS.Utility;
using SurvivalFPS.Core.UI;
using SurvivalFPS.Messaging;

namespace SurvivalFPS.Core.Audio
{
    public partial class AudioManager : SingletonBehaviour<AudioManager>
    {
        //item in the audio pooling system
        //this is used for one shot sounds
        private class AudioPoolItem
        {
            public GameObject GameObject = null;
            public Transform Transform = null;
            public AudioSource AudioSource = null;
            public float Unimportance = float.MaxValue;
            public bool Playing = false;
            public IEnumerator Coroutine = null;
            public ulong ID = 0;
        }

        //information about an audio group in the mixer
        private class AudioGroupInfo
        {
            public string Name;
            public AudioMixerGroup Group;
            public IEnumerator TrackFaderRoutine;
        }

        [SerializeField] AudioMixer m_Mixer = null;
        [SerializeField] int m_MaxSounds = 90;

        //audio group lookup
        Dictionary<string, AudioGroupInfo> m_AudioGroupInfos = new Dictionary<string, AudioGroupInfo>();

        //audio source pool
        //the audio source pool
        private List<AudioPoolItem> m_Pool = new List<AudioPoolItem>();
        //an id-to-source dictionary to keep track of active audio sources in the pool
        private Dictionary<ulong, AudioPoolItem> m_ActivePool = new Dictionary<ulong, AudioPoolItem>();
        //current ID to assign
        private ulong m_IdGiver = 0;
        //position of the audio listener
        private Transform m_ListenerPos = null;

        //layered audio sources and their owners
        private Dictionary<GameObject, LayeredAudioSource> m_LayeredAudioSources = new Dictionary<GameObject, LayeredAudioSource>();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            if (!m_Mixer) return;

            AudioMixerGroup[] groups = m_Mixer.FindMatchingGroups(string.Empty);

            foreach (AudioMixerGroup group in groups)
            {
                AudioGroupInfo groupInfo = new AudioGroupInfo();
                groupInfo.Name = group.name;
                groupInfo.Group = group;
                groupInfo.TrackFaderRoutine = null;
                m_AudioGroupInfos[group.name] = groupInfo;
            }

            // Generate Pool
            for (int i = 0; i < m_MaxSounds; i++)
            {
                // Create GameObject and assigned AudioSource and Parent
                GameObject go = new GameObject("Pool Item");
                AudioSource audioSource = go.AddComponent<AudioSource>();
                go.transform.parent = transform;

                // Create and configure Pool Item
                AudioPoolItem poolItem = new AudioPoolItem();
                poolItem.GameObject = go;
                poolItem.AudioSource = audioSource;
                poolItem.Transform = go.transform;
                poolItem.Playing = false;
                go.SetActive(false);
                m_Pool.Add(poolItem);
            }            
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Messenger.AddPersistentListener(M_EventType.OnGamePaused, OnGamePaused);
            Messenger.AddPersistentListener(M_EventType.OnGameResumed, OnGameResumed);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Messenger.RemovePersistentListener(M_EventType.OnGamePaused, OnGamePaused);
            Messenger.RemovePersistentListener(M_EventType.OnGameResumed, OnGameResumed);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            AudioListener audioListener = FindObjectOfType<AudioListener>();
            if (audioListener) m_ListenerPos = audioListener.transform;

            m_LayeredAudioSources.Clear();
        }

        private void OnGamePaused()
        {
            //pause all pooled sounds
            foreach(AudioPoolItem item in m_ActivePool.Values)
            {
                item.AudioSource.Pause();
            }

            //pause all layered sounds
            foreach (LayeredAudioSource source in m_LayeredAudioSources.Values)
            {
                source.Pause();
            }
        }

        private void OnGameResumed()
        {
            //resume all pooled sounds
            foreach (AudioPoolItem item in m_ActivePool.Values)
            {
                item.AudioSource.UnPause();
            }

            //resume all layered sounds
            foreach (LayeredAudioSource source in m_LayeredAudioSources.Values)
            {
                source.UnPause();
            }
        }

        private void Update()
        {
            foreach (LayeredAudioSource source in m_LayeredAudioSources.Values)
            {
                source.UpdateSource();
            }
        }

#region public functions
        public ILayeredAudioSource RegisterLayeredAudioSource(GameObject requester, int layerNumber, bool allowSameLayerOverride)
        {
            LayeredAudioSource layeredAudioSource = new LayeredAudioSource(requester, layerNumber, allowSameLayerOverride);
            m_LayeredAudioSources[requester] = layeredAudioSource;
            return layeredAudioSource;
        }

        public void UnRegisterLayeredAudioSource(GameObject requester)
        {
            if (m_LayeredAudioSources.ContainsKey(requester))
            {
                m_LayeredAudioSources.Remove(requester);
            }
        }

        /// <summary>
        /// Play a layered sound. If the requester has no layered audio source, it will be added.
        /// </summary>
        public void PlayLayered(GameObject requester, AudioCollection audioCollection, int collectionBankNum, byte layerIndex, bool looping = true)
        {
            LayeredAudioSource layeredAudioSource;

            if (!m_LayeredAudioSources.TryGetValue(requester, out layeredAudioSource))
            {
                layeredAudioSource = new LayeredAudioSource(requester);
                m_LayeredAudioSources[requester] = layeredAudioSource;
            }

            layeredAudioSource.Play(audioCollection, collectionBankNum, layerIndex, looping);
        }

        /// <summary>
        /// Stops the audio layer.
        /// </summary>
        /// <param name="requester">Requester.</param>
        /// <param name="layer">Layer.</param>
        public void StopPlayLayered(GameObject requester, byte layer)
        {
            LayeredAudioSource layeredAudioSource;

            if (!m_LayeredAudioSources.TryGetValue(requester, out layeredAudioSource))
            {
                return;
            }

            layeredAudioSource.Stop(layer);
        }

        public float GetGroupVolume(string groupName)
        {
            if (!m_Mixer) return float.MinValue;
            AudioGroupInfo groupInfo;

            if (m_AudioGroupInfos.TryGetValue(groupName, out groupInfo))
            {
                float volume;
                m_Mixer.GetFloat(groupName, out volume);
                return volume;
            }

            return float.MinValue;
        }

        public AudioMixerGroup GetAudioGroupByName(string name)
        {
            AudioGroupInfo trackInfo;

            if (m_AudioGroupInfos.TryGetValue(name, out trackInfo))
            {
                return trackInfo.Group;
            }

            return null;
        }

        public void SetGroupVolume(string groupName, float volume, float fadeTime = 0.0f)
        {
            if (!m_Mixer) return;
            AudioGroupInfo trackInfo;

            if (m_AudioGroupInfos.TryGetValue(groupName, out trackInfo))
            {
                if (trackInfo.TrackFaderRoutine != null) StopCoroutine(trackInfo.TrackFaderRoutine);

                if (Mathf.Approximately(fadeTime, 0.0f))
                {
                    m_Mixer.SetFloat(groupName, volume);
                }
                else
                {
                    trackInfo.TrackFaderRoutine = _setGroupVolumeRoutine(groupName, volume, fadeTime);
                    StartCoroutine(trackInfo.TrackFaderRoutine);
                }
            }
        }

        /// <summary>
        /// Play a clip one shot. 
        /// Increase priority to override low priority sounds.
        /// </summary>
        /// <returns>The one shot sound.</returns>
        /// <param name="groupName">Audio mixer group it should belong to.</param>
        /// <param name="clip">Clip to play.</param>
        /// <param name="position">The position of this sound.</param>
        /// <param name="volume">The volume of this sound.</param>
        /// <param name="spatialBlend">Spatial blend.</param>
        /// <param name="priority">The priority of this sound. </param>
        public ulong PlayOneShotSound(string groupName, AudioClip clip, Vector3 position, float volume, float spatialBlend, byte priority = 255)
        {
            // Do nothing if track does not exist, clip is null or volume is zero
            if (!m_AudioGroupInfos.ContainsKey(groupName) || clip == null || volume.Equals(0.0f)) return 0;

            float unimportance = (m_ListenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);

            int leastImportantIndex = -1;
            float leastImportanceValue = float.MinValue;

            // Find an available audio source to use
            for (int i = 0; i < m_Pool.Count; i++)
            {
                AudioPoolItem poolItem = m_Pool[i];

                // Is this source available
                if (!poolItem.Playing)
                    return ConfigurePoolObject(i, groupName, clip, position, volume, spatialBlend, unimportance);


                // We have a pool item that is less important than the one we are going to play
                if (poolItem.Unimportance > leastImportanceValue)
                {
                    // Record the least important sound we have found so far
                    // as a candidate to relace with our new sound request
                    leastImportanceValue = poolItem.Unimportance;
                    leastImportantIndex = i;
                }
            }

            // If we get here all sounds are being used but we know the least important sound currently being
            // played so if it is less important than our sound request then use replace it
            if (leastImportanceValue > unimportance)
                return ConfigurePoolObject(leastImportantIndex, groupName, clip, position, volume, spatialBlend, unimportance);

            // Could not be played (no sound in the pool available)
            return 0;
        }

        /// <summary>
        /// Play a clip one shot after the specified time in seconds
        /// </summary>
        /// <returns>The one shot sound.</returns>
        /// <param name="groupName">Audio mixer group it should belong to.</param>
        /// <param name="clip">Clip to play.</param>
        /// <param name="position">The position of this sound.</param>
        /// <param name="volume">The volume of this sound.</param>
        /// <param name="spatialBlend">Spatial blend.</param>
        /// <param name="priority">The priority of this sound.</param>
        public void PlayOneShotSoundDelayed(string groupName, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, byte priority = 255)
        {
            StartCoroutine(_playOneShotSoundDelayedRoutine(groupName, clip, position, volume, spatialBlend, duration, priority));
        }

        /// <summary>
        /// Stops the sound immediately.
        /// </summary>
        /// <param name="id">Sound identifier.</param>
        public void StopSound(ulong id)
        {
            AudioPoolItem activeSound;

            // If this if exists in our active pool
            if (m_ActivePool.TryGetValue(id, out activeSound))
            {
                activeSound.AudioSource.Stop();
                activeSound.AudioSource.clip = null;
                activeSound.GameObject.SetActive(false);
                m_ActivePool.Remove(id);

                // Make it available again
                activeSound.Playing = false;
            }
        }

        /// <summary>
        /// Stops the sound delayed.
        /// </summary>
        /// <param name="id">Sound identifier.</param>
        /// <param name="duration">Duration.</param>
        public void StopSoundDelayed(ulong id, float duration)
        {
            StartCoroutine(_stopSoundDelayedRoutine(id, duration));
        }
#endregion

#region private internal helpers
        /// <summary>
        /// Internal function to occupy a pool item. 
        /// </summary>
        /// <returns>The ID of this pool object.</returns>
        /// <param name="poolIndex">Index of the pool item in the pooling list. Note that this is not its ID.</param>
        /// <param name="groupName">Audio Mixer group name the audio source belongs to.</param>
        /// <param name="clip">Clip to play.</param>
        /// <param name="position">Position.</param>
        /// <param name="volume">Volume.</param>
        /// <param name="spatialBlend">Spatial blend.</param>
        /// <param name="unimportance">Unimportance.</param>
        private ulong ConfigurePoolObject(int poolIndex, string groupName, AudioClip clip, Vector3 position, float volume, float spatialBlend, float unimportance)
        {
            // If poolIndex is out of range abort request
            if (poolIndex < 0 || poolIndex >= m_Pool.Count) return 0;

            // Get the pool item
            AudioPoolItem poolItem = m_Pool[poolIndex];

            // Generate new ID so we can stop it later if we want to
            m_IdGiver++;

            // Configure the audio source's position and colume
            AudioSource source = poolItem.AudioSource;
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatialBlend;

            // Assign to requested audio group/track
            source.outputAudioMixerGroup = m_AudioGroupInfos[groupName].Group;

            // Position source at requested position
            source.transform.position = position;

            // Enable GameObject and record that it is now playing
            poolItem.Playing = true;
            poolItem.Unimportance = unimportance;
            poolItem.ID = m_IdGiver;
            poolItem.GameObject.SetActive(true);
            source.Play();

            //use a coroutine to make it available again after the clip is finished
            poolItem.Coroutine = _stopSoundDelayedRoutine(m_IdGiver, source.clip.length);
            StartCoroutine(poolItem.Coroutine);

            // Add this sound to our active pool with its unique id
            m_ActivePool[m_IdGiver] = poolItem;

            // Return the id to the caller
            return m_IdGiver;
        }

        /// <summary>
        /// Internal function to set a group volume gradually
        /// </summary>
        /// <returns>The group volume routine.</returns>
        /// <param name="groupName">Name.</param>
        /// <param name="volume">Volume.</param>
        /// <param name="fadeTime">Fade time.</param>
        private IEnumerator _setGroupVolumeRoutine(string groupName, float volume, float fadeTime)
        {
            float startVolume = 0.0f;
            float time = 0.0f;
            m_Mixer.GetFloat(groupName, out startVolume);

            while (time < fadeTime)
            {
                time += Time.unscaledDeltaTime;
                m_Mixer.SetFloat(groupName, Mathf.Lerp(startVolume, volume, time / fadeTime));
                yield return null;
            }

            m_Mixer.SetFloat(groupName, volume);
        }

        /// <summary>
        /// Internal function used by PlayOneShotDelayed
        /// </summary>
        private IEnumerator _playOneShotSoundDelayedRoutine(string groupName, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, byte priority = 255)
        {
            yield return new WaitForSeconds(duration);
            PlayOneShotSound(groupName, clip, position, volume, spatialBlend, priority);
        }

        /// <summary>
        /// Internal function to stop the audio source and makes its pool item available in the pool after the specified time
        /// </summary>
        /// <returns>The sound delayed.</returns>
        /// <param name="id">ID of the audio source in the pool.</param>
        /// <param name="duration">num of seconds to wait before stopping.</param>
        private IEnumerator _stopSoundDelayedRoutine(ulong id, float duration)
        {
            yield return new WaitForSeconds(duration);
            AudioPoolItem activeSound;

            // If this if exists in our active pool
            if (m_ActivePool.TryGetValue(id, out activeSound))
            {
                activeSound.AudioSource.Stop();
                activeSound.AudioSource.clip = null;
                activeSound.GameObject.SetActive(false);
                m_ActivePool.Remove(id);

                // Make it available again
                activeSound.Playing = false;
            }
        }
#endregion
    }
}