using UnityEngine;
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
        #region Editor Exposed Variables
        [field: Header("Dependencies")]
        [field: SerializeField] public CharacterController controller { get; private set; }
        [field: SerializeField] public PlayerInput input { get; private set; }

        [field: Header("General Movement")]
        [field: SerializeField] public float walkSpeed { get; private set; } = 7f;
        [field: SerializeField] public float gravity { get; private set; } = -30f;
        #endregion

        public PlayerYState playerYState { get; private set; }

        private Vector3 inputVector;
        private Vector3 smoothedVector;
        private Vector3 planerImpulseVector;

        private const float ACCELERATION = 10f;
        private const float DECELERATION = 9f;
        private const float AIR_RECOVERY_MOD = 1f;
        private const float AIR_ACCELERATION = 5f;
        private const float AIR_DECELERATION = 0.2f;
        private const float GROUNDED_Y_VELOCITY = -0.05f;
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
        #endregion

        #region MonoBehaviour Functions
        #region Initilization
        private void Awake()
        {
            if (LayerMask.NameToLayer("Player") == -1)
                Debug.LogError("Player layer not found in LayerMask. Please create a layer named \"Player\" and assign it to the Player GameObject.");
        }
        private void Start()
        {
            desiredSpeed = walkSpeed;
            currentSpeed = walkSpeed;
            yVelocity = GROUNDED_Y_VELOCITY;
            playerYState = PlayerYState.Grounded;
            speedLerpModifier = DEFAULT_LERP_MODIFIER;
            speedBoostLerpModifier = DEFAULT_LERP_MODIFIER;
            groundConstraintTime = DEFAULT_GROUND_CONSTRAINT_TIME;
            groundConstraintTimer = DEFAULT_GROUND_CONSTRAINT_TIME;
        }
        #endregion

        private void Update()
        {
            if (PauseManager.instance.gamePaused)
                return;

            InitializeController();

            inputVector = (transform.forward * input.moveY + transform.right * input.moveX).normalized;

            ApplyGravity();
            SmoothMovement();
            InterpolateParameters();

            controller.Move((smoothedVector * currentSpeed + Vector3.up * yVelocity + planerImpulseVector) * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (playerYState == PlayerYState.Ascending)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * (controller.height - controller.radius + 0.01f), controller.radius);
            }
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
        #endregion
    }
}
