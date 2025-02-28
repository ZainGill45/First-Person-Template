using UnityEngine;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerInput playerInput;

        [Header("General Settings")]
        [SerializeField] private float jumpHeight = 1f;

        private float jumpForce;

        private void Start()
        {
            jumpForce = Mathf.Sqrt(jumpHeight * -2f * playerMovement.GetBaseControllerGravity());
        }

        private void Update()
        {
            if (playerInput.jumpDown && playerMovement.IsGrounded())
            {
                playerMovement.PauseGroundConstraints();
                playerMovement.AddImpulseVector(Vector3.up * jumpForce, true);
            }
        }
    }
}
