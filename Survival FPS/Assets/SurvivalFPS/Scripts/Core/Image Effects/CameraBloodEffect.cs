using UnityEngine;
using System.Collections;

namespace SurvivalFPS.Core
{
    [ExecuteInEditMode()]
    public class CameraBloodEffect : MonoBehaviour
    {
        [SerializeField] private float m_BloodAmount = 0.0f;
        [SerializeField] private float m_MinBloodAmount = 0.0f;
        [SerializeField] private float m_Distortion = 1.0f;
        [SerializeField] private bool m_AutoFade = true;
        [SerializeField] private float m_FadeSpeed = 0.05f;

        [SerializeField] private Shader m_Shader = null;
        [SerializeField] private Texture2D m_BloodTexture = null;
        [SerializeField] private Texture2D m_BloodNormalMap = null;

        private Material m_Material = null;

        public float bloodAmount { get { return m_BloodAmount; } set { m_BloodAmount = value; } }
        public float minBloodAmount { get { return m_MinBloodAmount; } set { m_MinBloodAmount = value; } }
        public bool autoFade { get { return m_AutoFade; } set { m_AutoFade = value; } }
        public float fadeSpeed { get { return m_FadeSpeed; } set { m_FadeSpeed = value; } }

        private void Update()
        {
            if(m_AutoFade)
            {
                m_BloodAmount -= m_FadeSpeed * Time.deltaTime;
                m_BloodAmount = Mathf.Max(m_BloodAmount, m_MinBloodAmount);
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (m_Shader == null) return;
            if (m_Material == null)
            {
                m_Material = new Material(m_Shader);
            }

            if (m_Material == null) return;

            //send data into shader
            if(m_BloodTexture)
                m_Material.SetTexture("_BloodTex", m_BloodTexture);

            if(m_BloodNormalMap)
                m_Material.SetTexture("_BloodBump", m_BloodTexture);

            m_Material.SetFloat("_Distortion", m_Distortion);
            m_Material.SetFloat("_BloodAmount", m_BloodAmount);


            Graphics.Blit(src, dest, m_Material);
        }
    }
}
