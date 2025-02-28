using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private PlayerInput playerInput;

        [Header("General Movement")]
        [SerializeField] private float walkSpeed = 7f;

        private Vector3 inputVector;
        private Vector3 smoothedVector;
        private Vector3 planerImpulseVector;

        private const float GRAVITY = -30f;
        private const float ACCELERATION = 10f;
        private const float DECELERATION = 9f;
        private const float AIR_RECOVERY_MOD = 1f;
        private const float AIR_ACCELERATION = 5f;
        private const float AIR_DECELERATION = 0.2f;
        private const float GROUNDED_Y_VELOCITY = -0.1f;
        private const float GROUNDED_RECOVERY_MOD = 5f;
        private const float DEFAULT_LERP_MODIFIER = 5f;
        private const float DEFAULT_GROUND_CONSTRAINT_TIME = 0.1f;

        private float yVelocity;
        private float speedBoost;
        private float desiredSpeed;
        private float currentSpeed;
        private float speedLerpModifier;
        private float groundConstraintTime;
        private float groundConstraintTimer;
        private float speedBoostLerpModifier;

        private void Start()
        {
            desiredSpeed = walkSpeed;
            currentSpeed = walkSpeed;
            speedLerpModifier = DEFAULT_LERP_MODIFIER;
            speedBoostLerpModifier = DEFAULT_LERP_MODIFIER;
            groundConstraintTime = DEFAULT_GROUND_CONSTRAINT_TIME;
            groundConstraintTimer = DEFAULT_GROUND_CONSTRAINT_TIME;
        }

        private void Update()
        {
            if (PauseManager.instance.gamePaused)
                return;

            InitializeController();

            inputVector = (transform.forward * playerInput.moveY + transform.right * playerInput.moveX).normalized;

            ApplyGravity();
            SmoothMovement();
            InterpolateParameters();

            controller.Move((smoothedVector * currentSpeed + Vector3.up * yVelocity + planerImpulseVector) * Time.deltaTime);
        }

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
        }
        private void ApplyGravity()
        {
            if (groundConstraintTimer <= groundConstraintTime)
            {
                groundConstraintTimer += Time.deltaTime;
                return;
            }

            yVelocity = !controller.isGrounded ? yVelocity += GRAVITY * Time.deltaTime : yVelocity = GROUNDED_Y_VELOCITY;
        }
        private void SmoothMovement()
        {
            if (controller.isGrounded)
            {
                if (inputVector.magnitude > 0f)
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * ACCELERATION);
                }
                else
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * DECELERATION);
                }
            }
            else
            {
                if (inputVector.magnitude > 0f)
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * AIR_ACCELERATION);
                }
                else
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * AIR_DECELERATION);
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
        public void GiveSpeedBoost(float boostAmount, float lerpModifier = DEFAULT_LERP_MODIFIER)
        {
            speedBoost = boostAmount;
            speedBoostLerpModifier = lerpModifier;
        }
        public void PauseGroundConstraints(float time = DEFAULT_GROUND_CONSTRAINT_TIME)
        {
            groundConstraintTime = time;
            groundConstraintTimer = 0f;
        }
        public float GetBaseControllerGravity()
        {
            return GRAVITY;
        }
        public bool IsGrounded()
        {
            return controller.isGrounded;
        }
        #endregion
    }
}
