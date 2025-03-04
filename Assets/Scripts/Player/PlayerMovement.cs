using UnityEngine;
using Utilities;
using Managers;
using System;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Variables
        [field: Header("Dependencies")]
        [field: SerializeField] private Camera playerCamera;
        [field: SerializeField] private CharacterController controller;

        [field: Header("Camera Settings")]
        [field: SerializeField, Range(0f, 1f)] private float headOffset = 0.25f;
        [field: SerializeField, Range(0f, 90f)] private float maxVerticalClamp = 90f;
        [field: SerializeField, Range(0f, -90f)] private float minVerticalClamp = -90f;
        [field: SerializeField, Range(60f, 130f)] private float defaultFOV = 103f;
        [field: SerializeField, Range(0.01f, 1f)] private float sensitivity = 0.2f;

        [field: Header("Movement Settings")]
        [field: SerializeField] private float walkSpeed = 7f;
        [field: SerializeField] private float jumpHeight = 1f;
        [field: SerializeField] private float acceleration = 10f;
        [field: SerializeField] private float deceleration = 9f;
        [field: SerializeField] private float airAcceleration = 5f;
        [field: SerializeField] private float airDeceleration = 0.2f;

        [field: Header("Sprint Settings")]
        [field: SerializeField, Range(60f, 130f)] private float sprintFOV = 106f;
        [field: SerializeField] private float sprintModifier = 2f;
        [field: SerializeField] private float sprintEnterModifier = 2f;
        [field: SerializeField] private float sprintExitModifier = 3f;

        [field: Header("Miscellaneous Settings")]
        [field: SerializeField] private float defaultLerpModifier = 5f;
        [field: SerializeField] private float camPositionalSmoothing = 64f;
        [field: SerializeField] private float gravity = -30f;

        private Vector3 inputVector;
        private Vector3 smoothedVector;
        private Vector3 planerImpulseVector;

        private const float AIR_RECOVERY_MOD = 1f;
        private const float GROUNDED_RECOVERY_MOD = 5f;

        private float angleX;
        private float angleY;
        private float jumpForce;
        private float yVelocity;
        private float speedBoost;
        private float desiredFOV;
        private float desiredSpeed;
        private float currentSpeed;
        private float fovLerpModifier;
        private float speedLerpModifier;
        private float sprintVerticalFOV;
        private float defaultVerticalFOV;
        private float speedBoostLerpModifier;

        private bool sprinting;
        private bool ceilingHit;
        #endregion

        #region Initilization
        private void Awake()
        {
            if (LayerMask.NameToLayer("Player") == -1)
                Debug.LogError("Player layer not found in LayerMask. Please create a layer named \"Player\" and assign it to the Player GameObject.");
        }
        private void Start()
        {
            defaultVerticalFOV = Camera.HorizontalToVerticalFieldOfView(defaultFOV, ZUtils.DEFAULT_AR);
            sprintVerticalFOV = Camera.HorizontalToVerticalFieldOfView(sprintFOV, ZUtils.DEFAULT_AR);

            jumpForce = Mathf.Sqrt(jumpHeight * -2f * gravity);

            desiredSpeed = walkSpeed;
            currentSpeed = walkSpeed;
            desiredFOV = defaultVerticalFOV;
            fovLerpModifier = defaultLerpModifier;
            speedLerpModifier = defaultLerpModifier;
            playerCamera.fieldOfView = defaultVerticalFOV;
            speedBoostLerpModifier = defaultLerpModifier;
        }
        #endregion

        private void Update()
        {
            InitializeController();
            RotateController();

            inputVector = (transform.forward * InputManager.moveY + transform.right * InputManager.moveX).normalized;

            EvaluateJumping();
            EvaluateSprinting();

            controller.Move((smoothedVector * currentSpeed + Vector3.up * yVelocity + planerImpulseVector) * Time.deltaTime);
        }

        #region Private Functions
        private void InitializeController()
        {
            #region Reset Planer Impulse Vector
            if (controller.isGrounded)
            {
                if (inputVector.magnitude > 0f)
                {
                    planerImpulseVector = Vector3.MoveTowards(planerImpulseVector, Vector3.zero,
                        Time.deltaTime * controller.velocity.magnitude * GROUNDED_RECOVERY_MOD + 0.01f);
                }
                else
                {
                    planerImpulseVector = Vector3.MoveTowards(planerImpulseVector, Vector3.zero,
                        Time.deltaTime * controller.velocity.magnitude * GROUNDED_RECOVERY_MOD * 0.5f + 0.01f);
                }
            }
            else
            {
                if (inputVector.magnitude > 0f)
                {
                    planerImpulseVector = Vector3.MoveTowards(planerImpulseVector, Vector3.zero,
                        Time.deltaTime * controller.velocity.magnitude * AIR_RECOVERY_MOD + 0.01f);
                }
                else
                {
                    planerImpulseVector = Vector3.MoveTowards(planerImpulseVector, Vector3.zero,
                        Time.deltaTime * controller.velocity.magnitude * AIR_RECOVERY_MOD * 0.5f + 0.01f);
                }
            }
            #endregion

            #region Smooth Controller Movement
            if (controller.isGrounded)
            {
                if (inputVector.magnitude > 0f)
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * acceleration);
                }
                else
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * deceleration);
                }
            }
            else
            {
                if (inputVector.magnitude > 0f)
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * airAcceleration);
                }
                else
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * airDeceleration);
                }
            }
            #endregion

            #region Check Head Collision
            if ((controller.collisionFlags & CollisionFlags.Above) != 0 && !ceilingHit)
            {
                ceilingHit = true;
                yVelocity = 0f;
            }
            #endregion

            #region Apply Gravity
            if (controller.isGrounded)
                yVelocity = -0.1f;

            if (!controller.isGrounded)
                yVelocity += gravity * Time.deltaTime;
            #endregion

            #region Interpolate Parameters
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, desiredFOV, Time.deltaTime * fovLerpModifier);

            speedBoost = Mathf.Lerp(speedBoost, 0f, Time.deltaTime * speedBoostLerpModifier);
            currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, Time.deltaTime * speedLerpModifier);

            currentSpeed += speedBoost;
            #endregion
        }
        private void RotateController()
        {
            angleX += InputManager.mouseX * sensitivity;
            angleY -= InputManager.mouseY * sensitivity;

            angleY = Mathf.Clamp(angleY, minVerticalClamp, maxVerticalClamp);

            playerCamera.transform.localEulerAngles = new Vector3(angleY, angleX, 0f);
            controller.transform.localEulerAngles = new Vector3(0f, angleX, 0f);

            if (ZUtils.Approx(playerCamera.transform.position, controller.transform.position + Vector3.up * (controller.height - headOffset), ZUtils.SMALL_THRESHOLD))
            {
                playerCamera.transform.position = controller.transform.position + Vector3.up * (controller.height - headOffset);
                return;
            }

            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position,
                                                           controller.transform.position + Vector3.up * (controller.height - headOffset),
                                                           Time.deltaTime * camPositionalSmoothing);
        }
        private void EvaluateJumping()
        {
            if (InputManager.jumpDown && controller.isGrounded)
                AddImpulseVector(Vector3.up * jumpForce, true);
        }
        private void EvaluateSprinting()
        {
            if (InputManager.sprintHold && !sprinting)
            {
                desiredSpeed = walkSpeed * sprintModifier;
                speedLerpModifier = sprintEnterModifier;

                desiredFOV = sprintVerticalFOV;
                fovLerpModifier = sprintEnterModifier;

                sprinting = true;
            }
            else if (!InputManager.sprintHold && sprinting)
            {
                desiredSpeed = walkSpeed;
                speedLerpModifier = sprintExitModifier;

                desiredFOV = defaultVerticalFOV;
                fovLerpModifier = sprintExitModifier;

                sprinting = false;
            }
        }
        #endregion

        #region Public Functions
        public void AddImpulseVector(Vector3 forceVector, bool overrideVelocity = false)
        {
            if (overrideVelocity)
            {
                planerImpulseVector = new Vector3(0f, planerImpulseVector.y, 0f);
                yVelocity = 0f;
            }

            planerImpulseVector += Vector3.right * forceVector.x + Vector3.forward * forceVector.z;
            yVelocity += forceVector.y;
        }
        #endregion
    }
}
