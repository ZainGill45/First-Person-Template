using UnityEngine;
using Managers;

namespace Player
{
    public class PlayerJump : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerMovement player;

        [Header("General Settings")]
        [SerializeField] private float jumpHeight = 1f;

        private float jumpForce;

        private void Start()
        {
            jumpForce = Mathf.Sqrt(jumpHeight * -2f * player.gravity);
        }

        private void Update()
        {
            if (PauseManager.instance.gamePaused)
                return;

            if (player.input.jumpDown && player.playerYState == PlayerYState.Grounded)
            {
                player.PauseGroundConstraints();
                player.AddImpulseVector(Vector3.up * jumpForce, true);
            }
        }
    }
}
