using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.FPS
{
    /// <summary>
    /// a class that manages all the component animators on a game object
    /// these animators are supposed to run on the same animator controller
    /// and operate on models all sharing the same rig
    /// </summary>
    public class PlayerAnimatorManager : MonoBehaviour
    {
        //a map of an animator to its activity
        private Dictionary<Animator, bool> m_Animators = new Dictionary<Animator, bool>();
        private RuntimeAnimatorController m_RuntimeAnimatorController;

        public RuntimeAnimatorController runtimeAnimatorController
        {
            set
            {
                m_RuntimeAnimatorController = value;

                if (m_Animators.Count > 0)
                {
                    foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                    {
                        if (pair.Value && pair.Key)
                        {
                            pair.Key.runtimeAnimatorController = value;
                        }
                    }
                }
            }
        }

        public Dictionary<Animator, bool> animators { get { return m_Animators; } }

        public void AddAnimator(Animator animator)
        {
            if(!m_Animators.ContainsKey(animator))
            {
                m_Animators.Add(animator, true);
            }
        }
        /// <summary>
        /// allow the animator's parameters to be changed by the set parameter functions
        /// </summary>
        public void EnableAnimator(Animator animator)
        {
            if (m_Animators.ContainsKey(animator))
            {
                m_Animators[animator] = true;
            }
        }
        /// <summary>
        /// do not allow the animator's parameters to be changed by the set parameter functions
        /// </summary>
        public void DisableAnimator(Animator animator)
        {
            if (m_Animators.ContainsKey(animator))
            {
                m_Animators[animator] = false;
            }
        }

        public void SetFloat(int id, float value)
        {
            if(m_Animators.Count > 0)
            {
                foreach(KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if(pair.Value && pair.Key)
                    {
                        pair.Key.SetFloat(id, value);
                    }
                }
            }
        }
        public void SetFloat(string name, float value)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetFloat(name, value);
                    }
                }
            }
        }
        public void SetFloat(int id, float value, float dampTime, float deltaTime)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetFloat(id, value, dampTime, deltaTime);
                    }
                }
            }
        }
        public void SetFloat(string name, float value, float dampTime, float deltaTime)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetFloat(name, value, dampTime, deltaTime);
                    }
                }
            }
        }
        public void SetBool(string name, bool value)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetBool(name, value);
                    }
                }
            }
        }
        public void SetBool(int id, bool value)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetBool(id, value);
                    }
                }
            }
        }
        public void SetTrigger(string name)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetTrigger(name);
                    }
                }
            }
        }
        public void SetTrigger(int id)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.SetTrigger(id);
                    }
                }
            }
        }
        public void Play(string stateName, int layer, float normalizedTime = 0.0f)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.Play(stateName, -1, normalizedTime);
                    }
                }
            }
        }
        public void Play(int stateNameHash, int layer, float normalizedTime = 0.0f)
        {
            if (m_Animators.Count > 0)
            {
                foreach (KeyValuePair<Animator, bool> pair in m_Animators)
                {
                    if (pair.Value && pair.Key)
                    {
                        pair.Key.Play(stateNameHash, -1, normalizedTime);
                    }
                }
            }
        }
    }

}