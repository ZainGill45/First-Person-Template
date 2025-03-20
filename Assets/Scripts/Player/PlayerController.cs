using UnityEngine;
using Utilities;
using Managers;
using System;
using KCC;

namespace Player
{
    [Serializable]
    public struct LayeredParameter
    {
        public float parameterValue;
        public bool active;
    }

    public class PlayerController : MonoBehaviour, ICharacterController
    {
        #region Variables
        [field: Header("Dependencies")]
        [field: SerializeField] public KinematicCharacterMotor collision { get; private set; }
        [field: SerializeField] public Camera playerCamera { get; private set; }

        [field: Header("Camera Settings")]
        [field: SerializeField, Range(0f, -90f)] public float minVerticalClamp { get; private set; } = -90f;
        [field: SerializeField, Range(0f, 90f)] public float maxVerticalClamp { get; private set; } = 90f;
        [field: SerializeField, Range(50f, 150f)] public float defaultFOV { get; private set; } = 103f;
        [field: SerializeField, Range(0.01f, 1f)] public float sensitivity { get; private set; } = 0.2f;

        [field: Header("General Settings")]
        [field: SerializeField] public float defaultSpeed { get; private set; } = 7f;

        [field: Header("Movement Smoothing")]
        [field: SerializeField] public float acceleration { get; private set; } = 9f;
        [field: SerializeField] public float deceleration { get; private set; } = 9f;
        [field: SerializeField] public float airAcceleration { get; private set; } = 5f;
        [field: SerializeField] public float airDeceleration { get; private set; } = 0.1f;

        [field: Header("Miscellaneous Settings")]
        [field: SerializeField] public float defaultLerpModifier { get; private set; } = 5f;
        [field: SerializeField] public float gravity { get; private set; } = 30f;

        public delegate void DefaultSpeedChangedDelegated();
        public event DefaultSpeedChangedDelegated OnDefaultSpeedChanged;
        public delegate void DefaultFOVChangedDelegated();
        public event DefaultFOVChangedDelegated OnDefaultFOVChanged;
        public delegate void LeftGroundDelegated();
        public event LeftGroundDelegated OnLeftGround;
        public delegate void LandedDelegated();
        public event LandedDelegated OnLanded;

        private RaycastHit headHit;

        public LayeredParameter[] desiredSpeedRegistry { get; private set; }
        public LayeredParameter[] desiredFOVRegistry { get; private set; }

        public Vector3 inputVector { get; private set; }
        public Vector3 smoothedVector { get; private set; }
        public Vector3 planerImpulseVector { get; private set; }

        public float defaultVerticalFOV { get; private set; }
        public float impulseFOVLerpModifier { get; set; }
        public float speedLerpModifier { get; set; }
        public float fovLerpModifier { get; set; }

        private const float GROUNDED_RECOVERY_MOD = 5f;
        private const float AIR_RECOVERY_MOD = 1f;

        private const int DEFAULT_SPEED_LAYER = 0;
        private const int DEFAULT_FOV_LAYER = 0;

        private float currentSpeed;
        private float impulseFOV;
        private float yVelocity;
        private float angleX;
        private float angleY;
        #endregion

        #region Initilization
        private void Awake()
        {
            collision.CharacterController = this;

            desiredSpeedRegistry = new LayeredParameter[3];
            desiredFOVRegistry = new LayeredParameter[3];
        }
        private void Start()
        {
            defaultVerticalFOV = Camera.HorizontalToVerticalFieldOfView(defaultFOV, 16f / 9f);

            playerCamera.fieldOfView = defaultVerticalFOV;
            speedLerpModifier = defaultLerpModifier;
            fovLerpModifier = defaultLerpModifier;
            currentSpeed = defaultSpeed;

            LayeredParameter defaultDesiredSpeed = new LayeredParameter
            {
                parameterValue = defaultSpeed,
                active = true
            };
            desiredSpeedRegistry[DEFAULT_SPEED_LAYER] = defaultDesiredSpeed;

            LayeredParameter defaultDesiredFOV = new LayeredParameter
            {
                parameterValue = defaultVerticalFOV,
                active = true
            };
            desiredFOVRegistry[DEFAULT_FOV_LAYER] = defaultDesiredFOV;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        private void OnEnable()
        {
            OnLanded += OnLandedResponse;
        }
        private void OnDisable()
        {
            OnLanded -= OnLandedResponse;
        }
        #endregion

        private void Update()
        {
            if (PauseManager.instance.gamePaused) 
                return;

            #region Reset Planer Impulse Vector
            planerImpulseVector = collision.GroundingStatus.IsStableOnGround
                ? inputVector.magnitude > 0f
                    ? Vector3.MoveTowards(planerImpulseVector, Vector3.zero, Time.deltaTime * collision.Velocity.magnitude * GROUNDED_RECOVERY_MOD + 0.01f)
                    : Vector3.MoveTowards(planerImpulseVector, Vector3.zero, Time.deltaTime * collision.Velocity.magnitude * GROUNDED_RECOVERY_MOD * 0.5f + 0.01f)
                : inputVector.magnitude > 0f
                    ? Vector3.MoveTowards(planerImpulseVector, Vector3.zero, Time.deltaTime * collision.Velocity.magnitude * AIR_RECOVERY_MOD + 0.01f)
                    : Vector3.MoveTowards(planerImpulseVector, Vector3.zero, Time.deltaTime * collision.Velocity.magnitude * AIR_RECOVERY_MOD * 0.5f + 0.01f);
            #endregion

            #region Rotate Controller
            angleX += Input.GetAxisRaw("Mouse X") * sensitivity;
            angleY -= Input.GetAxisRaw("Mouse Y") * sensitivity;
            angleY = Mathf.Clamp(angleY, minVerticalClamp, maxVerticalClamp);

            transform.localEulerAngles = new Vector3(0, angleX, 0);
            playerCamera.transform.localEulerAngles = new Vector3(angleY, angleX, 0);
            playerCamera.transform.position = transform.position + Vector3.up * (collision.Capsule.height - 0.25f);
            #endregion

            inputVector = (transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal")).normalized;

            #region Apply Gravity
            if (!collision.GroundingStatus.IsStableOnGround)
                yVelocity -= gravity * Time.deltaTime;
            #endregion

            #region Smooth Movement Vector
            smoothedVector = collision.GroundingStatus.IsStableOnGround
                ? inputVector.magnitude > 0f
                    ? Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * acceleration)
                    : Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * deceleration)
                : inputVector.magnitude > 0f
                    ? Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * airAcceleration)
                    : Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * airDeceleration);
            #endregion

            #region Interpolate Parameters
            impulseFOV = Mathf.Lerp(impulseFOV, 0f, Time.deltaTime * impulseFOVLerpModifier);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, ComputeDesiredFOV() + impulseFOV, Time.deltaTime * fovLerpModifier);

            currentSpeed = Mathf.Lerp(currentSpeed, ComputeDesiredSpeed(), Time.deltaTime * speedLerpModifier);
            #endregion
        }

        private float ComputeDesiredSpeed()
        {
            float computed = defaultSpeed;

            int maxLayer = int.MinValue;

            for (int i = 0; i < desiredSpeedRegistry.Length; i++)
            {
                LayeredParameter param = desiredSpeedRegistry[i];

                if (param.active && i > maxLayer)
                {
                    maxLayer = i;
                    computed = param.parameterValue;
                }
            }

            return computed;
        }
        private float ComputeDesiredFOV()
        {
            float computed = defaultVerticalFOV;

            int maxLayer = int.MinValue;

            for (int i = 0; i < desiredFOVRegistry.Length; i++)
            {
                LayeredParameter param = desiredFOVRegistry[i];
                if (param.active && i > maxLayer)
                {
                    maxLayer = i;
                    computed = param.parameterValue;
                }
            }

            return computed;
        }

        #region KCC Callbacks
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
        {
            currentRotation = Quaternion.Euler(new Vector3(0, angleX, 0));
        }
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = smoothedVector * currentSpeed + planerImpulseVector + collision.CharacterUp * yVelocity;
        }
        public void AfterCharacterUpdate(float deltaTime)
        {
        }
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }
        public bool IsColliderValidForCollisions(Collider coll) 
        { 
            return true; 
        }
        public void OnDiscreteCollisionDetected(Collider hitCollider) 
        {
        }
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) 
        { 
        }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            if (!collision.GroundingStatus.IsStableOnGround && collision.Velocity.y > 0 && hitNormal.y < 0)
            {
                if (Physics.SphereCast(transform.position + transform.up * (collision.Capsule.height - collision.Capsule.radius), collision.Capsule.radius, collision.transform.up, out headHit))
                    yVelocity = 0f;
            }
        }
        public void PostGroundingUpdate(float deltaTime) 
        {
            if (collision.GroundingStatus.IsStableOnGround && !collision.LastGroundingStatus.IsStableOnGround)
                OnLanded?.Invoke();
            else if (!collision.GroundingStatus.IsStableOnGround && collision.LastGroundingStatus.IsStableOnGround)
                OnLeftGround?.Invoke();
        }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) 
        { 
        }
        #endregion

        #region Public Functions
        public void AddImpulseVector(Vector3 forceVector, bool overrideVelocity = false)
        {
            if (forceVector.y != 0f)
                collision.ForceUnground();

            if (overrideVelocity)
            {
                planerImpulseVector = new Vector3(0f, planerImpulseVector.y, 0f);
                yVelocity = 0f;
            }
            planerImpulseVector += Vector3.right * forceVector.x + Vector3.forward * forceVector.z;
            yVelocity += forceVector.y;
        }
        public void RegisterNewDesiredSpeed(int layer, float targetSpeed, bool activeState)
        {
            LayeredParameter newDesiredSpeed = new LayeredParameter
            {
                parameterValue = targetSpeed,
                active = activeState
            };

            desiredSpeedRegistry[layer] = newDesiredSpeed;
        }
        public void RegisterNewDesiredFOV(int layer, float verticalFOV, bool activeState)
        {
            LayeredParameter newDesiredFOV = new LayeredParameter
            {
                parameterValue = verticalFOV,
                active = activeState
            };

            desiredFOVRegistry[layer] = newDesiredFOV;
        }
        public void SetDesiredSpeedActive(int layer, bool newActiveState)
        {
            desiredSpeedRegistry[layer].active = newActiveState;
        }
        public void SetDesiredFOVActive(int layer, bool newActiveState)
        {
            desiredFOVRegistry[layer].active = newActiveState;
        }
        public void SetDesiredSpeedParameter(int layer, float newDesiredSpeed)
        {
            desiredSpeedRegistry[layer].parameterValue = newDesiredSpeed;
        }
        public void SetDesiredFOVParameter(int layer, float newVerticalFOV)
        {
            desiredFOVRegistry[layer].parameterValue = newVerticalFOV;
        }
        public void AddSpeedBoost(float boost, float lerpModifier)
        {
            currentSpeed += boost;
            speedLerpModifier = lerpModifier;
        }
        public void AddImpulseFOV(float impulse, float lerpModifier)
        {
            impulseFOV += impulse;
            impulseFOVLerpModifier = lerpModifier;
        }
        public void SetDefaultFOV(float newDefaultVerticalFOV)
        {
            defaultVerticalFOV = newDefaultVerticalFOV;
            desiredFOVRegistry[DEFAULT_FOV_LAYER].parameterValue = newDefaultVerticalFOV;

            OnDefaultFOVChanged?.Invoke();
        }
        public void SetDefaultSpeed(float newDefaultSpeed)
        {
            defaultSpeed = newDefaultSpeed;
            desiredSpeedRegistry[DEFAULT_SPEED_LAYER].parameterValue = newDefaultSpeed;

            OnDefaultSpeedChanged?.Invoke();
        }
        #endregion

        #region Event Responses
        private void OnLandedResponse()
        {
            yVelocity = 0f;
        }
        #endregion
    }
}
