using System;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;

using SurvivalFPS.Core.PlayerInteraction;

namespace SurvivalFPS.Core.FPS
{
    public enum InteractionEventType
    {
        OnPlayerInRange, //TODO
        OnPlayerOutOfRange, //TODO
        OnPlayerLookAtOutOfRange,
        OnPlayerLookAwayOutOfRange,
        OnPlayerLookAtInRange,
        OnPlayerLookAwayInRange,
        OnPlayerBeginInput,
        OnInteractionFail,
        OnInteractionSuccess,
        OnInteractionLimitReached //TODO
    }

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

        public event Action<InteractiveItem, InteractionEventType> interactionEvent;
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
                    //Debug.Log("looking at and in range");
                    interactionEvent(m_CurrentItem, 
                                     InteractionEventType.OnPlayerLookAtInRange);
                }
                else
                {
                    //Debug.Log("looking at but out of range");
                    interactionEvent(m_CurrentItem, 
                                     InteractionEventType.OnPlayerLookAtOutOfRange);
                }
            }
            //just looked away from this item
            else if (m_ItemLastFrame && m_CurrentItem != m_ItemLastFrame)
            {
                if(m_ItemLastFrame.isInRange)
                {
                    //Debug.Log("looked away and in range");
                    interactionEvent(m_ItemLastFrame,
                                     InteractionEventType.OnPlayerLookAwayInRange);
                }
                else
                {
                    //Debug.Log("looked away and out of range");
                    interactionEvent(m_ItemLastFrame,
                                     InteractionEventType.OnPlayerLookAwayOutOfRange);
                }

            }

            bool usePressed = Input.GetButtonDown(m_UseButtonName);

            //a leading edge of use button is detected and the player is in the range
            //of any interactable item
            if (usePressed && m_CurrentItem && m_CurrentItem.isInRange)
            {
                m_LastInputTime = Time.time;
                interactionEvent(m_CurrentItem, InteractionEventType.OnPlayerBeginInput);
                ChangeState(ControllerState.InteractionInput);
            }
        }

        private void InteractionInputUpdate()
        {
            m_InputTimer += Time.deltaTime;

            InteractiveItemConfig itemInfo = null;

            if(m_CurrentItem)
            {
                itemInfo = m_CurrentItem.inputConfig;
            }

            do
            {
                //the scanned item has been changed (the player is not facing the item)
                //the interaction will fail
                if (m_ItemLastFrame != m_CurrentItem)
                {
                    OnInteractionFail();
                    ChangeState(ControllerState.Scanning);
                    break;
                }

                //if we have reached the desired input time, begin interaction
                if (m_InputTimer > itemInfo.inputTime)
                {
                    OnInteractionSuccess();
                    ChangeState(ControllerState.Interaction);
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

                //if the player lifts up the button, the interaction will fail
                if (!Input.GetButton(m_UseButtonName))
                {
                    OnInteractionFail();
                    ChangeState(ControllerState.Scanning);
                    break;
                }

                //inform progress change listeners
                interactiveProgressChanged(m_InputTimer / itemInfo.inputTime);

                return;
            }
            while (false);

            //cleanup
            m_InputTimer = 0.0f;
        }

        private void InteractionUpdate()
        {
            if (m_CurrentItem && m_CurrentItem.isInteracting)
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

            interactionEvent(m_CurrentItem, InteractionEventType.OnInteractionFail);
        }

        private void OnInteractionSuccess()
        {
            interactiveProgressChanged(1.0f);

            interactionEvent(m_CurrentItem, InteractionEventType.OnInteractionSuccess);

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

