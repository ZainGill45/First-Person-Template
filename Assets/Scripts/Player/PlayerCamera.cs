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
        [SerializeField, Range(0f, 90f)] private float maxVerticalClamp = 90f;
        [SerializeField, Range(0f, -90f)] private float minVerticalClamp = -90f;
        [SerializeField, Range(60f, 130f)] private float defaultFOV = 103f;
        [SerializeField, Range(0.01f, 1f)] private float sensitivity = 0.2856f;

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

            angleY = Mathf.Clamp(angleY, minVerticalClamp, maxVerticalClamp);

            playerCamera.transform.localEulerAngles = new Vector3(angleY, angleX, 0f);
            characterController.transform.localEulerAngles = new Vector3(0f, angleX, 0f);

            playerCamera.transform.position = characterController.transform.position + Vector3.up * (characterController.height - CONTROLLER_HEAD_OFFSET);
        }
    }
}
