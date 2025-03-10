using UnityEngine;
using KCC;

namespace Player
{
    public struct CharacterInput
    {
        public Quaternion rotation;

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

        private Quaternion requestedRotation;
        private Vector3 requestedMovement;

        private bool requestedJump;

        public void Initialize()
        {
            characterMotor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput characterInput)
        {
            requestedRotation = characterInput.rotation;
            requestedMovement = new Vector3(characterInput.moveX, 0f, characterInput.moveY);

            requestedMovement = Vector3.ClampMagnitude(requestedMovement, 1f);

            requestedMovement = characterInput.rotation * requestedMovement;

            requestedJump = requestedJump || characterInput.jumpDown;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
        {
            Vector3 forward = Vector3.ProjectOnPlane
            (
                requestedRotation * Vector3.forward, 
                characterMotor.CharacterUp
            );

            if (forward != Vector3.zero )
                currentRotation = Quaternion.LookRotation(forward, characterMotor.CharacterUp);
        }
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
        {
            if (characterMotor.GroundingStatus.IsStableOnGround)
            {
                Vector3 groundedMovement = characterMotor.GetDirectionTangentToSurface
                (
                    direction: requestedMovement,
                    surfaceNormal: characterMotor.GroundingStatus.GroundNormal
                ) * requestedMovement.magnitude;

                currentVelocity = groundedMovement * defaultSpeed;
            } else
            {
                currentVelocity += characterMotor.CharacterUp * gravity * deltaTime;
            }

            if (requestedJump)
            {
                requestedJump = false;

                characterMotor.ForceUnground();
                currentVelocity += characterMotor.CharacterUp * jumpForce;
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
