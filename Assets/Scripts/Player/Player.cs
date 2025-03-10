using UnityEngine;
using Managers;

namespace Player
{
    public class Player : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private PlayerCamera playerCamera;

        private void Start()
        {
            playerCharacter.Initialize();
            playerCamera.Initialize(playerCharacter.cameraTarget);
        }

        private void Update()
        {
            if (PauseManager.instance.gamePaused) 
                return;

            CameraInput mouseInput = new() 
            { 
                mouseX = Input.GetAxisRaw("Mouse X"), 
                mouseY = Input.GetAxisRaw("Mouse Y") 
            };

            playerCamera.UpdateRotation(mouseInput);

            CharacterInput characterInput = new()
            {
                rotation = playerCamera.transform.localEulerAngles,
                moveX = Input.GetAxisRaw("Horizontal"),
                moveY = Input.GetAxisRaw("Vertical"),
                jumpDown = Input.GetKeyDown(KeyCode.Space),
                sprintHold = Input.GetKey(KeyCode.LeftShift)
            };

            playerCharacter.UpdateInput(characterInput);
        }

        private void LateUpdate()
        {
            playerCamera.UpdatePosition(playerCharacter.cameraTarget);
        }
    }
}
