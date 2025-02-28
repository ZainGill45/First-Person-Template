using UnityEngine;

namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera playerCamera;

        [Header("General Settings")]
        [field: SerializeField] private float defaultFOV = 103f;
        [field: SerializeField] private float sensitivity = 0.2856f;
        [field: SerializeField] private float verticalClamp = 90f;

        private const float CONTROLLER_HEAD_OFFSET = 0.25f;
        private const float DEFAULT_ASPECT_RATIO = 16f / 9f;

        private float angleX;
        private float angleY;
        private float verticalDefaultFOV;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            verticalDefaultFOV = Camera.HorizontalToVerticalFieldOfView(defaultFOV, DEFAULT_ASPECT_RATIO);
            playerCamera.fieldOfView = verticalDefaultFOV;
        }

        private void Update()
        {
            angleX += playerInput.mouseX * sensitivity;
            angleY -= playerInput.mouseY * sensitivity;

            angleY = Mathf.Clamp(angleY, -verticalClamp, verticalClamp);

            playerCamera.transform.localEulerAngles = new Vector3(angleY, angleX, 0f);
            characterController.transform.localEulerAngles = new Vector3(0f, angleX, 0f);

            playerCamera.transform.position = characterController.transform.position + Vector3.up * (characterController.height - CONTROLLER_HEAD_OFFSET);
        }
    }
}
