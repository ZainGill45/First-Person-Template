using UnityEngine;
using Utilities;
using Managers;

namespace Player
{
    public enum PlayerYState
    {
        Ascending,
        Descending,
        Grounded
    }

    public class PlayerMovement : MonoBehaviour
    {
        #region Variables
        [field: Header("Dependencies")]
        [field: SerializeField] public Camera playerCamera { get; private set; }
        [field: SerializeField] public CharacterController controller { get; private set; }

        [field: Header("Camera Settings")]
        [field: SerializeField, Range(0f, 1f)] public float headOffset { get; private set; } = 0.25f;
        [field: SerializeField, Range(0f, 90f)] public float maxVerticalClamp { get; private set; } = 90f;
        [field: SerializeField, Range(0f, -90f)] public float minVerticalClamp { get; private set; } = -90f;
        [field: SerializeField, Range(60f, 130f)] public float defaultFOV { get; private set; } = 103f;
        [field: SerializeField, Range(0.01f, 1f)] public float sensitivity { get; private set; } = 0.2856f;

        [field: Header("Movement Settings")]
        [field: SerializeField] public float walkSpeed { get; private set; } = 7f;
        [field: SerializeField] public float jumpHeight { get; private set; } = 1f;
        [field: SerializeField] public float acceleration { get; private set; } = 10f;
        [field: SerializeField] public float deceleration { get; private set; } = 9f;
        [field: SerializeField] public float airAcceleration { get; private set; } = 5f;
        [field: SerializeField] public float airDeceleration { get; private set; } = 0.2f;

        [field: Header("Sprint Settings")]
        [field: SerializeField] public float sprintModifier { get; private set; } = 2f;
        [field: SerializeField] public float sprintEnterModifier{ get; private set; } = 2f;
        [field: SerializeField] public float sprintExitModifier{ get; private set; } = 3f;

        [field: Header("Miscellaneous Settings")]
        [field: SerializeField] public float defaultLerpModifier { get; private set; } = 5f;
        [field: SerializeField] public float camPositionalSmoothing { get; private set; } = 64f;
        [field: SerializeField] public float gravity { get; private set; } = -30f;

        public PlayerYState playerYState { get; private set; }

        public bool sprinting { get; private set; }

        private Vector3 inputVector;
        private Vector3 smoothedVector;
        private Vector3 planerImpulseVector;

        private const float AIR_RECOVERY_MOD = 1f;
        private const float GROUNDED_RECOVERY_MOD = 5f;
        private const float GROUNDED_Y_VELOCITY = -0.05f;
        private const float DEFAULT_GROUND_CONSTRAINT_TIME = 0.1f;

        private float angleX;
        private float angleY;
        private float jumpForce;
        private float yVelocity;
        private float speedBoost;
        private float desiredSpeed;
        private float currentSpeed;
        private float speedLerpModifier;
        private float verticalDefaultFOV;
        private float groundConstraintTime;
        private float groundConstraintTimer;
        private float speedBoostLerpModifier;
        #endregion

        #region Initilization
        private void Awake()
        {
            if (LayerMask.NameToLayer("Player") == -1)
                Debug.LogError("Player layer not found in LayerMask. Please create a layer named \"Player\" and assign it to the Player GameObject.");
        }
        private void Start()
        {
            verticalDefaultFOV = Camera.HorizontalToVerticalFieldOfView(defaultFOV, ZUtils.DEFAULT_AR);
            jumpForce = Mathf.Sqrt(jumpHeight * -2f * gravity);

            desiredSpeed = walkSpeed;
            currentSpeed = walkSpeed;
            yVelocity = GROUNDED_Y_VELOCITY;
            playerYState = PlayerYState.Grounded;
            speedLerpModifier = defaultLerpModifier;
            playerCamera.fieldOfView = verticalDefaultFOV;
            speedBoostLerpModifier = defaultLerpModifier;
            groundConstraintTime = DEFAULT_GROUND_CONSTRAINT_TIME;
            groundConstraintTimer = DEFAULT_GROUND_CONSTRAINT_TIME;
        }
        #endregion

        #region MonoBehaviour Functions
        private void Update()
        {
            InitializeController();
            RotateController();

            inputVector = (transform.forward * InputManager.moveY + transform.right * InputManager.moveX).normalized;

            ApplyGravity();
            EvaluateJumping();
            EvaluateSprinting();
            SmoothMovement();
            InterpolateParameters();

            controller.Move((smoothedVector * currentSpeed + Vector3.up * yVelocity + planerImpulseVector) * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * (controller.height - controller.radius + 0.01f), controller.radius);
        }
        #endregion

        #region Private Functions
        private void InitializeController()
        {
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

            if (controller.isGrounded)
            {
                playerYState = PlayerYState.Grounded;
            } else if (yVelocity > GROUNDED_Y_VELOCITY)
            {
                playerYState = PlayerYState.Ascending;
            } else if (yVelocity < -Mathf.Abs(GROUNDED_Y_VELOCITY))
            {
                playerYState = PlayerYState.Descending;
            }
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
        private void ApplyGravity()
        {
            if (playerYState == PlayerYState.Ascending && 
                Physics.CheckSphere(transform.position + Vector3.up * (controller.height - controller.radius + 0.01f), controller.radius, LayerMask.NameToLayer("Player")))
            {
                    yVelocity = 0f;
            }

            if (groundConstraintTimer <= groundConstraintTime)
            {
                groundConstraintTimer += Time.deltaTime;
                return;
            }

            yVelocity = playerYState != PlayerYState.Grounded ? yVelocity += gravity * Time.deltaTime : yVelocity = GROUNDED_Y_VELOCITY;
        }
        private void EvaluateJumping()
        {
            if (InputManager.jumpDown && playerYState == PlayerYState.Grounded)
            {
                AddImpulseVector(Vector3.up * jumpForce, true);

                groundConstraintTime = 0.1f;
                groundConstraintTimer = 0f;
            }
        }
        private void EvaluateSprinting()
        {
            if (InputManager.sprintHold && !sprinting)
            {
                desiredSpeed = walkSpeed * sprintModifier;
                speedLerpModifier = sprintEnterModifier;

                sprinting = true;
            }
            else if (!InputManager.sprintHold && sprinting)
            {
                desiredSpeed = walkSpeed;
                speedLerpModifier = sprintExitModifier;

                sprinting = false;
            }
        }
        private void SmoothMovement()
        {
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
        }
        private void InterpolateParameters()
        {
            speedBoost = Mathf.Lerp(speedBoost, 0f, Time.deltaTime * speedBoostLerpModifier);
            currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, Time.deltaTime * speedLerpModifier);

            currentSpeed += speedBoost;
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
