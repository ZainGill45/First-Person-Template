using UnityEngine;
using KCC;

namespace Player
{
    public struct CharacterInput
    {
        public Vector3 rotation;

        public float moveX;
        public float moveY;

        public bool jumpDown;
        public bool sprintHold;
    }

    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private KinematicCharacterMotor characterMotor;
        [field: SerializeField] public Transform cameraTarget { get; private set; }

        [field: Header("General Settings")]
        [field: SerializeField] private float defaultSpeed = 8f;
        [field: SerializeField] private float jumpForce = 10f;
        [field: SerializeField] private float gravity = -30f;

        private Vector3 inputVector;
        private Vector3 smoothedVector;
        private Vector3 planerImpulseVector;

        private Vector3 requestedRotation;
        private Vector3 requestedMovement;

        private bool requestedJump;

        public void Initialize()
        {
            characterMotor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput characterInput)
        {
            inputVector = (characterMotor.CharacterForward * characterInput.moveY + characterMotor.CharacterRight * characterInput.moveX).normalized;

            if (characterMotor.GroundingStatus.IsStableOnGround)
            {
                if (inputVector.magnitude > 0f)
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 9f);
                } else
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 8f);
                }
            } else
            {
                if (inputVector.magnitude > 0f)
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 5f);
                }
                else
                {
                    smoothedVector = Vector3.Lerp(smoothedVector, inputVector, Time.deltaTime * 0.1f);
                }
            }

            requestedMovement = smoothedVector + planerImpulseVector;

            requestedJump = requestedJump || characterInput.jumpDown;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
        {
            currentRotation = Quaternion.Euler(new Vector3(0, requestedRotation.y, 0));
        }
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
        {
            if (characterMotor.GroundingStatus.IsStableOnGround)
            {
                currentVelocity = requestedMovement * defaultSpeed;
            } else
            {
                currentVelocity += characterMotor.CharacterUp * gravity * deltaTime;
            }

            if (requestedJump)
            {
                requestedJump = false;

                characterMotor.ForceUnground();

                float currentVerticalSpeed = Vector3.Dot(currentVelocity, characterMotor.CharacterUp);
                float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpForce);

                currentVelocity += characterMotor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
        }
        public void AfterCharacterUpdate(float deltaTime) { }
        public void BeforeCharacterUpdate(float deltaTime) { }
        public bool IsColliderValidForCollisions(Collider coll) { return true; }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void PostGroundingUpdate(float deltaTime) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        
    }
}
