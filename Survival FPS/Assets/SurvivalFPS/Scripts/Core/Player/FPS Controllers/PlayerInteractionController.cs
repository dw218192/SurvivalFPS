using System;
using UnityEngine;

using SurvivalFPS.Core.PlayerInteraction;

namespace SurvivalFPS.Core.FPS
{
    [DisallowMultipleComponent]
    public class PlayerInteractionController : PlayerController
    {
        private enum ControllerState { Inactive, Scanning, InteractionInput, Interaction }

        //inspector assigned
        [SerializeField] private float m_ScanRange;

        //private internal variables
        private Camera m_PlayerCamera;
        private GameSceneManager m_GameSceneManager;
        private PlayerManager m_PlayerManager;

        private RaycastHit m_HitInfo;
        private Vector3 m_RayScreenPoint;

        private InteractiveItem m_CurrentItem, m_ItemLastFrame;

        private float m_InputTimer = 0.0f;
        private float m_LastInputTime = -1.0f;

        private string m_UseButtonName = "Use";
        private ControllerState m_ControllerState = ControllerState.Scanning;

        //events
        public event Action<InteractiveItemConfig> interactiveBeingLookedAt;
        public event Action<float> interactiveProgressChanged;

        private void ChangeState(ControllerState newstate)
        {
            m_ControllerState = newstate;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_PlayerManager = GetComponent<PlayerManager>();
            m_PlayerCamera = m_PlayerManager.playerCamera;

            if(m_PlayerCamera)
            {
                m_RayScreenPoint = new Vector3((m_PlayerCamera.pixelWidth - 1) / 2, (m_PlayerCamera.pixelHeight - 1) / 2, 0.0f);
            }
            else
            {
                Debug.LogWarning("PlayerInteractionController.Start()- player camera is null");
            }

            m_GameSceneManager = GameSceneManager.Instance;
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_ControllerState == ControllerState.Inactive)
            {
                return;
            }

            m_CurrentItem = RaycastForItems();

            if (m_ControllerState == ControllerState.Scanning)
            {
                ScanningUpdate();
            }
            else if (m_ControllerState == ControllerState.InteractionInput)
            {
                InteractionInputUpdate();
            }
            else if (m_ControllerState == ControllerState.Interaction)
            {
                InteractionUpdate();
            }

            m_ItemLastFrame = m_CurrentItem;
        }

        private void ScanningUpdate()
        {
            //in range and is looking at this item
            if (m_CurrentItem)
            {
                if(m_CurrentItem.isInRange)
                {
                    //Debug.Log("looked at");
                    interactiveBeingLookedAt(m_CurrentItem.itemData);
                }
                //too far away from this item
                else
                {
                    interactiveBeingLookedAt(null);
                }
            }
            //just looked away from this item
            else if (m_ItemLastFrame && m_ItemLastFrame.isInRange && !m_CurrentItem)
            {
                //Debug.Log("looked away");
                interactiveBeingLookedAt(null);
            }

            bool usePressed = Input.GetButtonDown(m_UseButtonName);

            //a leading edge of use button is detected and the player is in the range
            //of any interactable item
            if (usePressed && m_CurrentItem && m_CurrentItem.isInRange)
            {
                m_LastInputTime = Time.time;
                ChangeState(ControllerState.InteractionInput);
            }
        }

        private void InteractionInputUpdate()
        {
            m_InputTimer += Time.deltaTime;

            InteractiveItemConfig itemInfo = m_CurrentItem.itemData;

            do
            {
                //if we have reached the desired input time, begin interaction
                if (m_InputTimer > itemInfo.inputTime)
                {
                    OnInteractionSuccess();
                    ChangeState(ControllerState.Interaction);
                    break;
                }

                //the scanned item has been changed (the player is not facing the item)
                //the interaction will fail
                if (m_ItemLastFrame != m_CurrentItem)
                {
                    OnInteractionFail();
                    ChangeState(ControllerState.Scanning);
                    break;
                }

                //if the player is no longer not in the range
                //the interaction will fail
                if (!m_CurrentItem.isInRange)
                {
                    OnInteractionFail();
                    ChangeState(ControllerState.Scanning);
                    break;
                }

                //if the item has a max input interval of 0, which means the player
                //needs to hold the use button for a certain duration to interact with
                //the item
                if (itemInfo.inputInterval <= float.Epsilon)
                {
                    //if the player lifts up the button, the interaction will fail
                    if (!Input.GetButton(m_UseButtonName))
                    {
                        OnInteractionFail();
                        break;
                    }

                    //inform progress change listeners
                    interactiveProgressChanged(m_InputTimer / itemInfo.inputTime);

                    return;
                }

                //TODO: otherwise, it's a tapping interaction
                else
                {
                    if (Input.GetButtonDown(m_UseButtonName))
                    {
                        m_LastInputTime = Time.time;
                    }

                    if (Time.time - m_LastInputTime > itemInfo.inputInterval)
                    {
                        OnInteractionFail();
                        break;
                    }
                }
            }
            while (false);

            //cleanup
            m_InputTimer = 0.0f;
        }

        private void InteractionUpdate()
        {
            if (m_CurrentItem.itemData.isInteracting)
            {
                return;
            }

            ChangeState(ControllerState.Scanning);
        }

        private InteractiveItem RaycastForItems()
        {
            if(m_PlayerCamera && m_GameSceneManager)
            {
                Ray ray = m_PlayerCamera.ScreenPointToRay(m_RayScreenPoint);

                if(Physics.Raycast(ray, out m_HitInfo, m_ScanRange, m_GameSceneManager.interactiveLayerMask))
                {
                    //Debug.Log(m_HitInfo.collider.name);
                    int instanceID = m_HitInfo.collider.GetInstanceID();
                    return m_GameSceneManager.GetInteractiveItemByColliderID(instanceID);
                }
            }

            return null;
        }

        private void OnInteractionFail()
        {
            interactiveProgressChanged(-1.0f);
        }

        private void OnInteractionSuccess()
        {
            interactiveProgressChanged(1.0f);

            m_CurrentItem.Interact(m_PlayerManager);
        }

#region controller implementation

        public override void ResumeControl()
        {
            ChangeState(ControllerState.Scanning);
        }

        public override void StopControl()
        {
            ChangeState(ControllerState.Inactive);
        }

        #endregion
    }
}

