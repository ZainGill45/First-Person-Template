using UnityEngine;
using Utilities;

namespace Player
{
    public struct CameraInput
    {
        public float mouseX;
        public float mouseY;
    }

    public class PlayerCamera : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private Camera mainCamera;

        [field: Header("General Settings")]
        [field: SerializeField, Range(50f, 150f)] private float defaultFOV = 103f;
        [field: SerializeField, Range(0.01f, 1f)] private float sensitivity = 0.2f;
        [field: SerializeField, Range(0f, -90f)] private float minVerticalClamp = -90f;
        [field: SerializeField, Range(0f, 90f)] private float maxVerticalClamp = 90f;

        private Vector3 eularAngles;

        private float defaultVerticalFOV;

        public void Initialize(Transform cameraTarget)
        {
            defaultVerticalFOV = Camera.HorizontalToVerticalFieldOfView(defaultFOV, ZUtils.DEFAULT_ASPECT_RATIO);
            mainCamera.fieldOfView = defaultVerticalFOV;

            transform.position = cameraTarget.position;
            transform.eulerAngles = eularAngles = cameraTarget.eulerAngles;
        }

        public void UpdateRotation(CameraInput cameraInput)
        {
            eularAngles += new Vector3(-cameraInput.mouseY * sensitivity, cameraInput.mouseX * sensitivity, 0);

            eularAngles.x = Mathf.Clamp(eularAngles.x, minVerticalClamp, maxVerticalClamp);

            transform.eulerAngles = eularAngles;
        }

        public void UpdatePosition(Transform cameraTarget)
        {
            transform.position = cameraTarget.position;
        }
    }
}
