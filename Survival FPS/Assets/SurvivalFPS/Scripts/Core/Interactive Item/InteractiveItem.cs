using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Core.PlayerInteraction
{
    public abstract class InteractiveItem : MonoBehaviour
    {
        [SerializeField] protected string m_ItemName;

        //inspector assigend
        [SerializeField] protected InteractiveItemConfig m_InputConfig;
        [SerializeField] protected Collider m_Collider;

        protected GameSceneManager m_GameSceneManager;

        public string itemName { get { return m_ItemName; } }
        public InteractiveItemConfig inputConfig { get { return m_InputConfig; } }
        public bool isInRange { get; private set; }
        public bool isInteracting { get; protected set; }

        protected virtual void Initialize() {}
        protected virtual void OnBeginInteract(PlayerManager playerManager) {}
        protected virtual void OnEndInteraction() {}

        protected void Start()
        {
            if (!m_Collider) m_Collider = GetComponent<Collider>();

            m_GameSceneManager = GameSceneManager.Instance;

            if (!m_GameSceneManager)
            {
                Debug.LogWarning("IteractiveItem: game scene manager cannot be found");
                return;
            }

            if (m_Collider)
            {
                m_GameSceneManager.RegisterInteractiveItemByColliderID(m_Collider.GetInstanceID(), this);
            }
            else
            {
                Debug.LogWarning("IteractiveItem: an interactive item does not have a collider!");
            }

            gameObject.layer = GameApplication.LayerData.interactiveLayer;
            m_Collider.isTrigger = true;

            Initialize();
        }

        public void Interact(PlayerManager playerManager)
        {
            isInteracting = true;

            OnBeginInteract(playerManager);
        }

        public void EndInteraction()
        {
            isInteracting = false;

            OnEndInteraction();
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