using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.Audio
{
    public partial class AudioManager
    {
        //layered audio source interface
        public interface ILayeredAudioSource
        {
            bool Play(AudioCollection collection, int bank, int layerIndex, bool looping = true);
            void Stop(int layerIndex);
            void StopDelayed(int layerIndex, float duration);
            void Mute(int layerIndex);
            void UnMute(int layerIndex);
            void MuteAll();
            void UnMuteAll();
            bool IsPlaying(int layerIndex);
        }

        private class LayeredAudioSource : ILayeredAudioSource
        {
            //an info class to store the info about a layer
            private class AudioLayer
            {
                public AudioLayer()
                {
                    Clear();
                }

                public void Clear()
                {
                    Clip = null;
                    Collection = null;
                    ClipBank = 0;
                    Looping = false;
                    Time = 0.0f;
                    Duration = 0.0f;
                    Muted = false;
                }

                public AudioClip Clip;
                public AudioCollection Collection;
                public int ClipBank = 0;
                public bool Looping = true; //loop the collection?
                public float Time = 0.0f; //time to track the current progress of the clip
                public float Duration = 0.0f; //duration of this layer
                public bool Muted = false; //is it muted?
            }

            //private vars
            private AudioSource m_AudioSource;
            private List<AudioLayer> m_AudioLayers = new List<AudioLayer>();
            private int m_ActiveLayer = -1;
            private bool m_Refresh = false;
            private bool m_AllowSameLayerOverride = false;

            public bool IsPlaying(int layerIndex)
            {
                if (m_AudioLayers.Count <= layerIndex) return false;
                return m_AudioLayers[layerIndex].Collection != null;
            }

            /// <summary>
            /// Creates a layered audio source
            /// </summary>
            /// <param name="owner">The owner of this layered audio source.</param>
            /// <param name="layerNum">The number of layers it has.</param>
            public LayeredAudioSource(GameObject owner, int layerNum = 5, bool allowSameLayerOverride = true)
            {
                if (owner != null && layerNum > 0)
                {
                    AudioSource audioSource = owner.GetComponent<AudioSource>();
                    m_AudioSource = audioSource ? audioSource : owner.AddComponent<AudioSource>();
                    m_AllowSameLayerOverride = allowSameLayerOverride;

                    for (int i = 0; i < layerNum; i++)
                    {
                        m_AudioLayers.Add(new AudioLayer());
                    }
                }
            }

            public bool Play(AudioCollection collection, int bank, int layerIndex, bool looping = true)
            {
                if (layerIndex >= m_AudioLayers.Count)
                {
                    Debug.LogWarning(typeof(LayeredAudioSource) + ": Play - layerIndex out of bound");
                    return false;
                }

                AudioLayer audioLayer = m_AudioLayers[layerIndex];

                //if it's the same audiolayer, do nothing if same layer overriding is disabled
                if (audioLayer.Collection == collection &&
                    audioLayer.Looping == looping &&
                    audioLayer.ClipBank == bank &&
                    !m_AllowSameLayerOverride)
                {
                    Debug.LogWarning(typeof(LayeredAudioSource) + ": Play - audio layer already exists");
                    return false;
                }

                audioLayer.Collection = collection;
                audioLayer.ClipBank = bank;
                audioLayer.Looping = looping;

                //time/duration/clip are handled in update
                audioLayer.Time = 0.0f;
                audioLayer.Duration = 0.0f;
                audioLayer.Muted = false;
                audioLayer.Clip = null;

                m_Refresh = true;

                return true;
            }

            public void Stop(int layerIndex)
            {
                if (layerIndex > m_AudioLayers.Count) return;
                AudioLayer audioLayer = m_AudioLayers[layerIndex];

                if (audioLayer != null)
                {
                    audioLayer.Looping = false;

                    //set time to duration (i.e. force it to finish playing) 
                    //so that in the next update
                    //time will be greater than duration
                    audioLayer.Time = audioLayer.Duration;
                }
            }

            public void StopDelayed(int layerIndex, float duration)
            {
                AudioManager.Instance.StartCoroutine(_stopDelayedRoutine(layerIndex, duration));
            }

            private IEnumerator _stopDelayedRoutine(int layerIndex, float duration)
            {
                yield return new WaitForSecondsRealtime(duration);
                Stop(layerIndex);
            }

            /// <summary>
            /// Mute a layer. Note that muting a layer does not pause it and will
            /// also mute the layers below it until this is finished
            /// </summary>
            /// <param name="layerIndex">Layer index.</param>
            public void Mute(int layerIndex)
            {
                if (layerIndex > m_AudioLayers.Count) return;
                AudioLayer audioLayer = m_AudioLayers[layerIndex];

                if (audioLayer != null)
                {
                    audioLayer.Muted = true;
                }
            }

            public void UnMute(int layerIndex)
            {
                if (layerIndex > m_AudioLayers.Count) return;
                AudioLayer audioLayer = m_AudioLayers[layerIndex];

                if (audioLayer != null)
                {
                    audioLayer.Muted = false;
                }
            }

            public void MuteAll()
            {
                for (int i = 0; i < m_AudioLayers.Count; i++)
                {
                    m_AudioLayers[i].Muted = true;
                }
            }

            public void UnMuteAll()
            {
                for (int i = 0; i < m_AudioLayers.Count; i++)
                {
                    m_AudioLayers[i].Muted = false;
                }
            }

            /*
             * cases to consider:
             * 1. new sound issued, its layer lower than the current active highest layer sound
             *      set clip/time/duration; refresh audio source
             * 2. new sound issued, its layer higher than the current active highest layer sound
             *      set clip/time/duration
             * 3. non-looping highest layer sound finished
             *      deactivate that sound
             * 4. looping highest layer sound finished
             *      set clip/time/duration; refresh audio source
             */
            public void UpdateSource()
            {
                //each frame we find the highest active layer
                //this is used to find the highest layer index with a clip assigned
                int newActiveLayer = -1;

                //iterate topdown
                for (int i = m_AudioLayers.Count - 1; i >= 0; i--)
                {
                    AudioLayer audioLayer = m_AudioLayers[i];

                    //if it's an inactive layer, continue
                    if (audioLayer.Collection == null) continue;

                    //else increment the progress of the clip
                    audioLayer.Time += Time.deltaTime;

                    //if we've just made a new play request (both time and duration is 0)
                    //or a clip has finished or terminated
                    if (audioLayer.Time > audioLayer.Duration)
                    {
                        //if it's a looping layer
                        //or it's the first time we've set up this player (clip is null)
                        //then we need to assign a new clip to this layer from the collection
                        if (audioLayer.Looping || audioLayer.Clip == null)
                        {
                            //fetch a new clip
                            AudioClip clip = audioLayer.Collection[audioLayer.ClipBank];

                            //if it's the same clip as before, wrap around in case of large deltaTime
                            if (clip == audioLayer.Clip)
                            {
                                audioLayer.Time = audioLayer.Time % audioLayer.Clip.length;
                            }
                            else
                            {
                                audioLayer.Time = 0.0f;
                            }

                            //update the duration and set the clip
                            audioLayer.Duration = clip.length;
                            audioLayer.Clip = clip;

                            //set this layer to be the new active layer 
                            if (newActiveLayer < i)
                            {
                                //this is new active layer index
                                newActiveLayer = i;
                            }

                            //if it needs to loop and the layer may not have not changed
                            if (audioLayer.Looping)
                            {
                                m_Refresh = true;
                            }
                        }
                        //it is not a looping layer, simply deactivate it
                        else
                        {
                            audioLayer.Clear();
                        }
                    }
                    //the layer is in the middle of playing a clip
                    else
                    {
                        //if this is the highest layer found
                        //set it to be the new highest layer
                        if (newActiveLayer < i)
                        {
                            newActiveLayer = i;
                        }
                    }
                }

                /*
                    if the highest active has changed since the last frame
                    or we need to refresh the audio source for a looping sound
                */
                if (newActiveLayer != m_ActiveLayer || m_Refresh)
                {
                    //if there is nothing to play at all
                    if (newActiveLayer == -1)
                    {
                        m_AudioSource.Stop();
                        m_AudioSource.clip = null;
                    }
                    //if there is something to play
                    else
                    {
                        AudioLayer audioLayer = m_AudioLayers[newActiveLayer];

                        //make the audio source to play the sound on the highest layer
                        m_AudioSource.clip = audioLayer.Clip;
                        m_AudioSource.volume = audioLayer.Muted ? 0.0f : audioLayer.Collection.volume;
                        m_AudioSource.spatialBlend = audioLayer.Collection.spatialBlend;
                        m_AudioSource.time = audioLayer.Time;
                        m_AudioSource.loop = false; //we handle the looping, not the audio source component
                        m_AudioSource.outputAudioMixerGroup = AudioManager.Instance.GetAudioGroupByName(audioLayer.Collection.audioGroup);
                        m_AudioSource.Play();
                    }

                    //update the active layer
                    m_ActiveLayer = newActiveLayer;
                }
                //if the highest active layer is not changed since the last frame
                else
                {
                    //mute it if required
                    if (m_ActiveLayer != -1 && m_AudioSource)
                    {
                        AudioLayer audioLayer = m_AudioLayers[m_ActiveLayer];
                        if (audioLayer.Muted) m_AudioSource.volume = 0.0f;
                        else m_AudioSource.volume = audioLayer.Collection.volume;
                    }
                }
            }
        }
    }
}

