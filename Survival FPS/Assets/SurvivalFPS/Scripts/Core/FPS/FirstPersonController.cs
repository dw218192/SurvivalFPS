using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SurvivalFPS.AI;

namespace SurvivalFPS.Core.FPS
{
    public class FirstPersonController : MonoBehaviour
    {
        public enum FPSCharacterState { Grounded, AirBorne, Crouching }

        [Serializable]
        public class MovementSettings
        {
            public float GroundedMotionDrag = 10f;
            [Range(1.0f, 1000.0f)] public float SlowDownRate = 100.0f;
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways

            public float CrouchMultiplier = 0.5f; // Speed when crounching
            public float RunMultiplier = 2.0f;   // Speed when sprinting

            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

            private bool m_Running;
            private bool m_Crouching;

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;
                if (input.x > 0 || input.x < 0)
                {
                    //strafe
                    CurrentTargetSpeed = StrafeSpeed;
                }
                if (input.y < 0)
                {
                    //backwards
                    CurrentTargetSpeed = BackwardSpeed;
                }
                if (input.y > 0)
                {
                    //forwards
                    //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                    CurrentTargetSpeed = ForwardSpeed;
                }
                if (Input.GetButton("Run"))
                {
                    CurrentTargetSpeed *= RunMultiplier;
                    m_Running = true;
                }
                else
                {
                    m_Running = false;
                }
                if (Input.GetButton("Crouch"))
                {
                    CurrentTargetSpeed *= CrouchMultiplier;
                    m_Crouching = true;
                }
                else
                {
                    m_Crouching = false;
                }
            }
            public bool Running
            {
                get { return m_Running; }
            }
            public bool Crouching
            {
                get { return m_Crouching; }
            }
        }

        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundForce= 0.5f; // stops the character
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }

        [SerializeField] private Flashlight m_Flashlight;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private Transform m_ArmAndHand;
        [SerializeField] private Transform m_WeaponSocket;
        [SerializeField] private PlayerAnimatorManager m_AnimatorManager;

        [SerializeField] private MovementSettings m_MovementSetting = new MovementSettings();
        [SerializeField] private AdvancedSettings m_AdvancedSetting = new AdvancedSettings();

        //sub-components
        [SerializeField] private FirstPersonCamera m_FPSCamera = new FirstPersonCamera();
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private CurveControlledBob m_WeaponBob = new CurveControlledBob();

        //internals
        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private Vector3 m_GroundContactNormal;
        private bool m_PreviouslyGrounded, m_IsGrounded;

        private Vector2 m_Input;
        private bool m_JumpKeyPressed;
        private bool m_CrouchKeyPressed;
        private float m_CharacterHeight;

        //used by bobs
        private Vector3 m_LocalSpaceCamPos;
        private Vector3 m_LocalSpaceWeaponPos;

        //used by FPS cam
        private Vector3 m_PunchAngle = Vector3.zero; //set by weapon recoil system

        //debug
        [SerializeField] private FPSCharacterState m_State = FPSCharacterState.AirBorne;
        private FPSCharacterState m_PrevState;

        //public properties
        public Vector3 velocity { get { return m_RigidBody.velocity; } }
        //state properties
        public bool isGrounded { get { return m_State == FPSCharacterState.Grounded; } }
        public bool jumping { get { return m_State == FPSCharacterState.AirBorne; } }
        public bool running { get { return m_MovementSetting.Running; } }
        public bool crouching { get { return m_MovementSetting.Crouching; } }
        //other properties
        public Transform weaponSocket { get { return m_WeaponSocket; } }
        public PlayerAnimatorManager playerAnimatorManager { get { return m_AnimatorManager; } }
        public Vector3 punchAngle { get { return m_PunchAngle; } set { m_PunchAngle = value; } }

        private void ChangeState(FPSCharacterState newState)
        {
            m_PrevState = m_State;

            if (m_State != newState)
            {
                m_State = newState;
            }
        }

        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();

            m_CharacterHeight = m_Capsule.height;

            m_RigidBody.useGravity = true;
            m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            //sub-component initialization
            m_FPSCamera.Init(transform, m_Camera.transform);
            m_HeadBob.Init(this);
            m_WeaponBob.Init(this);

            m_LocalSpaceCamPos = m_Camera.transform.localPosition;
            m_LocalSpaceWeaponPos = m_ArmAndHand.transform.localPosition;
            m_PrevState = m_State;
        }


        private void Update()
        {
            RotateView();

            if(Input.GetKeyDown(KeyCode.F))
            {
                m_Flashlight.gameObject.SetActive(!m_Flashlight.gameObject.activeSelf);
            }
        }

        private void LateUpdate()
        {
            m_FPSCamera.ApplyPunchAngle(m_PunchAngle);
            m_PunchAngle = Vector3.zero;
        }

        private void FixedUpdate()
        {
            GroundCheck();
            GetInput();

            if (m_State == FPSCharacterState.Grounded)
            {
                GroundedUpdate();
            }
            else if(m_State == FPSCharacterState.Crouching)
            {
                CrouchingUpdate();
            }
            else if(m_State == FPSCharacterState.AirBorne)
            {
                AirBorneUpdate();
            }

            m_JumpKeyPressed = false;
            AnimatorUpdate();
        }

        private void GroundedUpdate()
        {
            m_RigidBody.drag = m_MovementSetting.GroundedMotionDrag;

            if (!m_IsGrounded)
            {
                ChangeState(FPSCharacterState.AirBorne);
                return;
            }

            if (m_JumpKeyPressed)
            {
                m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                m_RigidBody.AddForce(m_MovementSetting.JumpForce * Vector3.up, ForceMode.Impulse);

                ChangeState(FPSCharacterState.AirBorne);
            }

            if (m_CrouchKeyPressed)
            {
                ChangeState(FPSCharacterState.Crouching);
                ResizeCollider();
                return;
            }

            Move();
            StickToGroundHelper();
            DoBobs();
        }

        private void AirBorneUpdate()
        {
            m_RigidBody.drag = 0f;

            if (!m_PreviouslyGrounded && m_IsGrounded)
            {
                ChangeState(FPSCharacterState.Grounded);
                return;
            }

            if(m_AdvancedSetting.airControl)
            {
                Move();
            }
        }

        private void CrouchingUpdate()
        {
            m_RigidBody.drag = m_MovementSetting.GroundedMotionDrag;

            if (!m_CrouchKeyPressed)
            {
                ChangeState(FPSCharacterState.Grounded);
                ResizeCollider();
                return;
            }

            if (!m_IsGrounded && m_PrevState != FPSCharacterState.Grounded)
            {
                ChangeState(FPSCharacterState.AirBorne);
                ResizeCollider();
                return;
            }

            Move();
            StickToGroundHelper();
            DoBobs();
        }

        private void AnimatorUpdate()
        {
            m_AnimatorManager.SetBool("Running", running);
        }

        private void ResizeCollider()
        {
            if(m_State == FPSCharacterState.AirBorne || m_State == FPSCharacterState.Grounded)
            {
                m_Capsule.height = m_CharacterHeight;
            }
            else if(m_State == FPSCharacterState.Crouching)
            {
                m_Capsule.height = m_CharacterHeight / 2.0f;
            }

            m_Capsule.center = Vector3.zero;
        }

        private void DoBobs()
        {
            Vector3 velocityXZ = m_RigidBody.velocity;
            velocityXZ.y = 0.0f;
            float speed = velocityXZ.magnitude;
            if (speed > 0.01f)
            {
                m_ArmAndHand.transform.localPosition = m_LocalSpaceWeaponPos + m_WeaponBob.GetLocalSpaceVectorOffset(speed);
                m_Camera.transform.localPosition = m_LocalSpaceCamPos + m_HeadBob.GetLocalSpaceVectorOffset(speed);
            }
            else
            {
                m_ArmAndHand.transform.localPosition = m_LocalSpaceWeaponPos;
                m_Camera.transform.localPosition = m_LocalSpaceCamPos;
            }
        }

        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return m_MovementSetting.SlopeCurveModifier.Evaluate(angle);
        }

        private void Move()
        {
            Vector3 desiredMove = Vector3.zero;

            if ((Mathf.Abs(m_Input.x) > float.Epsilon || Mathf.Abs(m_Input.y) > float.Epsilon))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                if (m_State == FPSCharacterState.Grounded || m_State == FPSCharacterState.Crouching)
                {
                    desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
                    desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;
                }
                else
                {
                    Vector3 forwardXZ = transform.forward;
                    forwardXZ.y = 0.0f;
                    forwardXZ.Normalize();
                    desiredMove = forwardXZ * m_Input.y + transform.right * m_Input.x;
                }

                desiredMove.x = desiredMove.x * m_MovementSetting.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * m_MovementSetting.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * m_MovementSetting.CurrentTargetSpeed;

                if (m_RigidBody.velocity.sqrMagnitude <
                    (m_MovementSetting.CurrentTargetSpeed * m_MovementSetting.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            DampenExtraSpeed(desiredMove);

            /*
            Debug.DrawLine(transform.position, transform.position + desiredMove, Color.red);
            Debug.DrawLine(transform.position, transform.position + m_RigidBody.velocity, Color.blue);
            */
        }

        //help the rigidbody to be more controllable
        private void DampenExtraSpeed(Vector3 desiredMove)
        {
            Vector3 vel = m_RigidBody.velocity;
            float coefficient = 1.0f / m_MovementSetting.SlowDownRate;

            if (desiredMove == Vector3.zero)
            {
                vel.x *= coefficient;
                vel.z *= coefficient;
            }
            else
            {
                if (desiredMove.x * vel.x < 0)
                {
                    vel.x *= coefficient;
                }

                if (desiredMove.z * vel.z < 0)
                {
                    vel.z *= coefficient;
                }
            }

            m_RigidBody.velocity = vel;
        }

        //make sure the character sticks to the ground
        private void StickToGroundHelper()
        {
            m_RigidBody.AddForce(m_AdvancedSetting.stickToGroundForce * Vector3.down, ForceMode.Impulse);
        }

        private void GetInput()
        {
            m_Input = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            m_MovementSetting.UpdateDesiredTargetSpeed(m_Input);

            if (Input.GetButtonDown("Jump") && !m_JumpKeyPressed)
            {
                m_JumpKeyPressed = true;
            }

            if (Input.GetButton("Crouch"))
            {
                m_CrouchKeyPressed = true;
            }
            else
            {
                m_CrouchKeyPressed = false;
            }
        }

        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            m_FPSCamera.LookRotation(transform, m_Camera.transform);

            if (m_IsGrounded || m_AdvancedSetting.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - m_AdvancedSetting.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) + m_AdvancedSetting.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
        }
    }
}