using UnityEngine;

namespace Player
{
    public class PlayerSprint : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Sprint Settings")]
        [SerializeField] private float sprintModifier = 2f;
        [SerializeField] private float sprintEnterModifier = 2f;
        [SerializeField] private float sprintExitModifier = 3f;

        public bool sprinting { get; private set; }

        private void Update()
        {
            if (playerMovement.input.sprintHold && !sprinting)
            {
                playerMovement.SetDesiredSpeed(playerMovement.walkSpeed * sprintModifier, sprintEnterModifier);
                sprinting = true;
            }
            else if (!playerMovement.input.sprintHold && sprinting)
            {
                playerMovement.SetDesiredSpeed(playerMovement.walkSpeed, sprintExitModifier);
                sprinting = false;
            }
        }
    }
}
