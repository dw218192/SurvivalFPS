using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.PlayerInteraction
{
    public class InteractiveItem : MonoBehaviour
    {
        //inspector assigend
        [SerializeField] protected InteractiveItemConfig m_ItemData;
        [SerializeField] protected Collider m_Collider;

        protected GameSceneManager m_GameSceneManager;
        public InteractiveItemConfig itemData { get { return m_ItemData; } }
        public bool isInRange { get; private set; }

        private void Awake()
        {
            if (!m_Collider) m_Collider = GetComponent<Collider>();

            m_GameSceneManager = GameSceneManager.Instance;

            if (!m_GameSceneManager)
            {
                Debug.LogWarning("IteractiveItem.Awake() - game scene manager cannot be found");
                return;
            }

            if (m_Collider)
            {
                m_GameSceneManager.RegisterInteractiveItemByColliderID(m_Collider.GetInstanceID(), this);
            }
            else
            {
                Debug.LogWarning("an interactive item does not have a collider!");
            }
        }

        private void Start()
        {
            gameObject.layer = m_GameSceneManager.interactiveLayer;
            m_Collider.isTrigger = true;
        }

        public void Interact(PlayerManager playerManager)
        {
            m_ItemData.Interact(playerManager);
        }

        private void OnTriggerExit(Collider other)
        {
            isInRange = false;
        }

        private void OnTriggerStay(Collider other)
        {
            isInRange = true;
        }
    }
}