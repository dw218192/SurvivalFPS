using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SurvivalFPS.AI
{
    /// <summary>
    /// state machine used by zombie AI characters
    /// </summary>
    public class AIZombieStateMachine : AIStateMachine
    {
        // Inspector Assigned
        [SerializeField] [Range(10.0f, 360.0f)] private float m_Fov = 50.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Sight = 0.5f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Hearing = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Aggression = 0.5f;
        [SerializeField] [Range(0, 100)] private int m_Health = 100;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Intelligence = 0.5f;
        [SerializeField] [Range(0.0f, 1.0f)] private float m_Satisfaction = 1.0f;
        [SerializeField] private float m_SpeedDampTime = 1.0f;
        //[SerializeField] private List<Aggravator> m_AvailableAggravators = new List<Aggravator>();

        // Private
        private int m_Seeking = 0;
        private bool m_Feeding = false;
        private bool m_Crawling = false;
        private int m_AttackType = 0;
        private float m_Speed = 0.0f;

        // Hashes
        private int m_SpeedHash = Animator.StringToHash("Speed");
        private int m_SeekingHash = Animator.StringToHash("Seeking");
        private int m_FeedingHash = Animator.StringToHash("Feeding");
        private int m_AttackHash = Animator.StringToHash("Attack");


        // Public Properties
        /// <summary>
        /// [between 0 and 360] an angle that defines the cone of a zombie's vision in its spherical sensor
        /// </summary>
        public float fov { get { return m_Fov; } }
        /// <summary>
        /// [between 0 and 1] the percentage of the zombie's sensory cone that marks how far the zombie can hear
        /// </summary>
        public float hearing { get { return m_Hearing; } }
        /// <summary>
        /// [between 0 and 1] the percentage of the zombie's sensory cone that marks how far the zombie can see
        /// </summary>
        public float sight { get { return m_Sight; } }
        /// <summary>
        /// the crawling parameter in the animator
        /// </summary>
        public bool crawling { get { return m_Crawling; } }
        /// <summary>
        /// [between 0 and 1] a zombie's tracking ability
        /// </summary>
        public float intelligence { get { return m_Intelligence; } }
        /// <summary>
        /// [between 0 and 1] a zombie's hunger meter
        /// </summary>
        public float satisfaction { get { return m_Satisfaction; } set { m_Satisfaction = value; } }
        /// <summary>
        /// [between 0 and 1] how agile the zombie is
        /// </summary>
        public float aggression { get { return m_Aggression; } set { m_Aggression = value; } }
        /// <summary>
        /// the current health level of the zombie
        /// </summary>
        public int health { get { return m_Health; } set { m_Health = value; } }
        /// <summary>
        /// the attackType parameter in the animator
        /// </summary>
        public int attackType { get { return m_AttackType; } set { m_AttackType = value; } }
        /// <summary>
        /// the feeding parameter in the animator
        /// </summary>
        public bool feeding { get { return m_Feeding; } set { m_Feeding = value; } }
        /// <summary>
        /// the seeking parameter in the animator, which is essentially a turn on spot (1 - right, 0 - stationary, -1 - left)
        /// </summary>
        public int seeking { get { return m_Seeking; } set { m_Seeking = value; } }
        /// <summary>
        /// the speed parameter in the animator
        /// </summary>
        public float speed { get { return m_Speed; } set { m_Speed = value; } }

        /// <summary>
        /// Refresh the animator with up-to-date values for its parameters
        /// </summary>
        protected override void UpdateStateMachine()
        {
            if (m_Animator != null)
            {
                m_Animator.SetFloat(m_SpeedHash, m_Speed, m_SpeedDampTime, Time.deltaTime);
                m_Animator.SetBool(m_FeedingHash, m_Feeding);
                m_Animator.SetInteger(m_SeekingHash, m_Seeking);
                m_Animator.SetInteger(m_AttackHash, m_AttackType);
            }
        }
    }
}
