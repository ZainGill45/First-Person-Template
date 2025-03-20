using UnityEngine;

namespace Player
{
    public class PlayerSprint : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private PlayerController controller;

        [field: Header("General Settings")]
        [field: SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [field: SerializeField, Range(50f, 150f)] private float sprintFOV = 108f;
        [field: SerializeField] private float sprintMultiplier = 2f;
        [field: SerializeField] private float sprintEnterModifier = 4f;
        [field: SerializeField] private float sprintExitModifier = 2f;

        private const int SPRINT_SPEED_LAYER = 1;
        private const int SPRINT_FOV_LAYER = 1;

        private float sprintVerticalFOV;

        private bool sprinting;

        #region Initilization
        private void Start()
        {
            sprintVerticalFOV = Camera.HorizontalToVerticalFieldOfView(sprintFOV, 16f / 9f);

            controller.RegisterNewDesiredSpeed(SPRINT_SPEED_LAYER, controller.defaultSpeed * sprintMultiplier, false);
            controller.RegisterNewDesiredFOV(SPRINT_FOV_LAYER, sprintVerticalFOV, false);
        }
        private void OnEnable()
        {
            controller.OnDefaultSpeedChanged += OnDefaultSpeedChangedResponse;
        }
        private void OnDisable()
        {
            controller.OnDefaultSpeedChanged -= OnDefaultSpeedChangedResponse;
        }
        #endregion

        private void Update()
        {
            if (Input.GetKey(sprintKey) && Input.GetAxisRaw("Vertical") > 0f)
            {
                if (sprinting) return;

                controller.SetDesiredSpeedActive(SPRINT_SPEED_LAYER, true);
                controller.SetDesiredFOVActive(SPRINT_FOV_LAYER, true);

                controller.speedLerpModifier = sprintEnterModifier;
                controller.fovLerpModifier = sprintEnterModifier;

                sprinting = true;
            }

            if (!Input.GetKey(sprintKey) || Input.GetAxisRaw("Vertical") <= 0f)
            {
                if (!sprinting) return;

                controller.SetDesiredSpeedActive(SPRINT_SPEED_LAYER, false);
                controller.SetDesiredFOVActive(SPRINT_FOV_LAYER, false);

                controller.speedLerpModifier = sprintExitModifier;
                controller.fovLerpModifier = sprintExitModifier;

                sprinting = false;
            }
        }

        #region Event Responses
        private void OnDefaultSpeedChangedResponse()
        {
            controller.SetDesiredSpeedParameter(SPRINT_SPEED_LAYER, controller.defaultSpeed * sprintMultiplier);
        }
        #endregion
    }
}