using UnityEngine;
using Utilities;
using Managers;
using KCC;

namespace Player
{
    public class PlayerController : MonoBehaviour, ICharacterController
    {
        #region Variables
        [field: Header("Dependencies")]
        [field: SerializeField] private Mesh debugMesh;
        [field: SerializeField] private KinematicCharacterMotor motor;
        [field: SerializeField] private Camera cam;

        [field: Header("Camera Settings")]
        [field: SerializeField, Range(50f, 150f)] private float defaultFOV = 103f;
        [field: SerializeField, Range(0.01f, 1f)] private float sensitivity = 0.2f;
        [field: SerializeField, Range(0f, -90f)] private float minVerticalClamp = -90f;
        [field: SerializeField, Range(0f, 90f)] private float maxVerticalClamp = 90f;

        [field: Header("General Settings")]
        [field: SerializeField] private float defaultSpeed = 8f;
        [field: SerializeField] private float jumpForce = 10f;
        [field: SerializeField] private float gravity = 30f;

        [field: Header("Debug Settings")]
        [field: SerializeField] private Color debugColor = Color.red;
        [field: SerializeField] private float headHitOffset = 0.04f;

        public delegate void OnLeftGroundDelegate();
        public event OnLeftGroundDelegate OnLeftGround;
        public delegate void OnLandedDelegate();
        public event OnLandedDelegate OnLanded;

        private Vector3 inputVector;
        private Vector3 smoothedVector;
        private Vector3 planerImpulseVector;

        private Vector3 controllerRotation;
        private Vector3 movementDirection;

        private float angleX;
        private float angleY;
        private float yVelocity;
        private float desiredFOV;
        private float currentSpeed;
        private float desiredSpeed;
        private float defaultVerticalFOV;
        #endregion

        #region Initilization
        private void Start()
        {
            defaultVerticalFOV = Camera.HorizontalToVerticalFieldOfView(defaultFOV, ZUtils.DEFAULT_ASPECT_RATIO);

            cam.fieldOfView = defaultVerticalFOV;
            motor.CharacterController = this;
            desiredFOV = defaultVerticalFOV;
            desiredSpeed = defaultSpeed;
            currentSpeed = defaultSpeed;
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

            #region Evaluate Controller Rotation
            angleX += Input.GetAxisRaw("Mouse X") * sensitivity;
            angleY -= Input.GetAxisRaw("Mouse Y") * sensitivity;

            angleY = Mathf.Clamp(angleY, minVerticalClamp, maxVerticalClamp);

            cam.transform.eulerAngles = new Vector3(angleY, angleX, 0);
            controllerRotation = new Vector3(0, angleX, 0);

            cam.transform.position = motor.transform.position + motor.CharacterUp * (motor.Capsule.height - 0.26f);
            #endregion

            inputVector = (motor.CharacterForward * Input.GetAxisRaw("Vertical") + motor.CharacterRight * Input.GetAxisRaw("Horizontal")).normalized;

            #region Apply Gravity
            if (motor.GroundingStatus.IsStableOnGround)
            {
                inputVector = motor.GetDirectionTangentToSurface(inputVector, motor.GroundingStatus.GroundNormal);
            } else
            {
                yVelocity -= gravity * Time.deltaTime;
            }
            #endregion

            #region Evaluate Jumping
            if (Input.GetKeyDown(KeyCode.Space) && motor.GroundingStatus.IsStableOnGround)
            {
                motor.ForceUnground();
                yVelocity += jumpForce;
            }
            #endregion

            #region Smooth Movement
            if (motor.GroundingStatus.IsStableOnGround)
            {
                smoothedVector = inputVector.magnitude > 0f
                    ? Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 9f)
                    : Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 8f);
            }
            else
            {
                smoothedVector = inputVector.magnitude > 0f
                    ? Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 5f)
                    : Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 0.1f);
            }
            #endregion

            movementDirection = smoothedVector + planerImpulseVector;
        }

        #region KCC Callbacks
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
        {
            currentRotation = Quaternion.Euler(controllerRotation);
        }
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = (movementDirection * defaultSpeed) + motor.CharacterUp * yVelocity;
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
            bool headHit = Physics.SphereCast(transform.position, motor.Capsule.radius - headHitOffset, motor.transform.up * (motor.Capsule.height - (motor.Capsule.radius - headHitOffset)), out RaycastHit hit);

            if (!motor.GroundingStatus.IsStableOnGround && motor.Velocity.y > 0 && hitNormal.y < 0 && headHit)
                yVelocity = 0f;
        }
        public void PostGroundingUpdate(float deltaTime) 
        {
            if (motor.GroundingStatus.IsStableOnGround && !motor.LastGroundingStatus.IsStableOnGround)
                OnLanded?.Invoke();
            else if (!motor.GroundingStatus.IsStableOnGround && motor.LastGroundingStatus.IsStableOnGround)
                OnLeftGround?.Invoke();
        }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) 
        { 
        }
        #endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = debugColor;

            Gizmos.DrawWireMesh(debugMesh, 0, motor.transform.position + Vector3.up * motor.Capsule.center.y, motor.transform.rotation, new Vector3(1f, 1f * motor.Capsule.height * 0.5f, 1f));
            Gizmos.DrawRay(cam.transform.position, cam.transform.forward);
            Gizmos.DrawWireSphere(transform.position + motor.transform.up * (motor.Capsule.height - (motor.Capsule.radius - headHitOffset)), motor.Capsule.radius - headHitOffset);
        }

        #region Event Responses
        private void OnLandedResponse()
        {
            yVelocity = 0f;
        }
        #endregion
    }
}
