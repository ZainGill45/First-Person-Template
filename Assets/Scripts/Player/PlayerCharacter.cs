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

        private Quaternion requestedRotation;

        public void Initialize()
        {
            characterMotor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput characterInput)
        {
            requestedRotation = characterInput.rotation;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
        {
            Vector3 forward = Vector3.ProjectOnPlane(requestedRotation * Vector3.forward, characterMotor.CharacterUp);
            currentRotation = Quaternion.LookRotation(forward, characterMotor.CharacterUp);
        }
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
        { 
        
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
