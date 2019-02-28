using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace SurvivalFPS.Core.UI
{
    public class LoadingScreen : GameMenu<LoadingScreen>
    {
        [SerializeField] private Image m_LoadingBarImg;
        private float m_LerpSpeed = 0.1f;

        // actual progress value reported by SceneManager AsyncOperation
        private float m_TargetProgressValue;
        // the asynchronous load ends at 0.1f, so you may want to pad the value so the bar fills out correctly
        private const float m_PaddingValue = 0.15f;
        // speed to animate the progress bar

        // used to tell other objects what our slider currently reads
        public float currentProgress 
        { 
            get 
            {
                return m_LoadingBarImg == null ? 0.0f : m_LoadingBarImg.fillAmount;
            }
        }

        public override void Init()
        {
            if (m_LoadingBarImg == null)
            {
                m_LoadingBarImg = gameObject.GetComponentInChildren<Image>();
            }

            m_LoadingBarImg.fillAmount = 0.0f;
        }

        // called by levelloader which provides the progress value
        // update the target progress value
        public void UpdateProgress(float progressValue)
        {
            m_TargetProgressValue = progressValue + m_PaddingValue;
        }

        // because the async progress reported by Unity does not smoothly animate, we can lerp to show some progress bar movement
        private void Update()
        {
            if (m_LoadingBarImg != null)
            {
                if (Mathf.Abs(m_LoadingBarImg.fillAmount - m_TargetProgressValue) > .01f)
                {
                    m_LerpSpeed = m_TargetProgressValue - m_LoadingBarImg.fillAmount;
                    m_LoadingBarImg.fillAmount = Mathf.MoveTowards(m_LoadingBarImg.fillAmount, m_TargetProgressValue, m_LerpSpeed * Time.deltaTime);
                }
            }
        }
    }

}
