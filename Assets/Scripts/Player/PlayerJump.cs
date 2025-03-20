using UnityEngine;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private PlayerController controller;

        [field: Header("General Settings")]
        [field: SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [field: SerializeField] private float jumpHeight = 1.5f;

        private float jumpForce;

        private void Start()
        {
            jumpForce = Mathf.Sqrt(jumpHeight * -2f * -controller.gravity);
        }

        private void Update()
        {
            if (Input.GetKeyDown(jumpKey) && controller.collision.GroundingStatus.IsStableOnGround)
                controller.AddImpulseVector(Vector3.up * jumpForce, true);
        }
    }
}
