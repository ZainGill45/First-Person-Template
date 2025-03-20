using System.Collections;
using UnityEngine;

namespace Player 
{
    public class PlayerCrouch : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private PlayerController controller;

        [field: Header("General Settings")]
        [field: SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [field: SerializeField, Range(50f, 150f)] private float crouchFOV = 98f;
        [field: SerializeField] private float crouchHeight = 1.45f;
        [field: SerializeField] private float crouchMultiplier = 0.7f;
        [field: SerializeField] private float crouchEnterModifier = 4f;
        [field: SerializeField] private float crouchExitModifier = 3f;

        private const int CROUCH_SPEED_LAYER = 2;
        private const int CROUCH_FOV_LAYER = 2;

        private float initialControllerYOffset;
        private float initialControllerHeight;
        private float crouchVerticalFOV;

        private bool crouching;

        #region Initilization
        private void Start()
        {
            crouchVerticalFOV = Camera.HorizontalToVerticalFieldOfView(crouchFOV, 16f / 9f);

            controller.RegisterNewDesiredSpeed(CROUCH_SPEED_LAYER, controller.defaultSpeed * crouchMultiplier, false);
            controller.RegisterNewDesiredFOV(CROUCH_FOV_LAYER, crouchVerticalFOV, false);

            initialControllerYOffset = controller.collision.Capsule.center.y;
            initialControllerHeight = controller.collision.Capsule.height;
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
            if (!controller.collision.GroundingStatus.IsStableOnGround)
                return;

            if (Input.GetKey(crouchKey))
            {
                if (crouching) return;

                StopCoroutine(nameof(ExitCrouch));
                StopCoroutine(nameof(EnterCrouch));

                StartCoroutine(nameof(EnterCrouch));

                crouching = true;
            }

            if (!Input.GetKey(crouchKey))
            {
                if (!crouching) return;

                StopCoroutine(nameof(EnterCrouch));
                StopCoroutine(nameof(ExitCrouch));

                StartCoroutine(nameof(ExitCrouch));

                crouching = false;
            }
        }

        private IEnumerator EnterCrouch()
        {
            controller.SetDesiredSpeedActive(CROUCH_SPEED_LAYER, true);
            controller.SetDesiredFOVActive(CROUCH_FOV_LAYER, true);

            controller.speedLerpModifier = crouchEnterModifier;
            controller.fovLerpModifier = crouchEnterModifier;

            while (controller.collision.Capsule.height != crouchHeight)
            {
                float controllerHeight = Mathf.Lerp(controller.collision.Capsule.height, crouchHeight, Time.deltaTime * crouchEnterModifier);
                float controllerYOffset = initialControllerYOffset - (initialControllerHeight - controllerHeight) * 0.5f;

                controller.collision.SetCapsuleDimensions(controller.collision.Capsule.radius, controllerHeight, controllerYOffset);

                if (Mathf.Approximately(controller.collision.Capsule.height, crouchHeight))
                    controller.collision.SetCapsuleDimensions(controller.collision.Capsule.radius, crouchHeight, crouchHeight * 0.5f);

                yield return null;
            }
        }
        private IEnumerator ExitCrouch()
        {
            while (controller.collision.Capsule.height != initialControllerHeight)
            {
                if (!Physics.Raycast(transform.position + transform.up * controller.collision.Capsule.height, transform.up, 0.05f))
                {
                    controller.SetDesiredSpeedActive(CROUCH_SPEED_LAYER, false);
                    controller.SetDesiredFOVActive(CROUCH_FOV_LAYER, false);

                    controller.speedLerpModifier = crouchExitModifier;
                    controller.fovLerpModifier = crouchExitModifier;

                    float controllerHeight = Mathf.Lerp(controller.collision.Capsule.height, initialControllerHeight, Time.deltaTime * crouchExitModifier);
                    float controllerYOffset = initialControllerYOffset - (initialControllerHeight - controllerHeight) * 0.5f;

                    controller.collision.SetCapsuleDimensions(controller.collision.Capsule.radius, controllerHeight, controllerYOffset);

                    if (Mathf.Approximately(controller.collision.Capsule.height, initialControllerHeight))
                        controller.collision.SetCapsuleDimensions(controller.collision.Capsule.radius, initialControllerHeight, initialControllerYOffset);
                }
                else
                {
                    Debug.DrawRay(transform.position + transform.up * controller.collision.Capsule.height, transform.up * 0.05f, Color.red);

                    controller.SetDesiredSpeedActive(CROUCH_SPEED_LAYER, true);
                    controller.SetDesiredFOVActive(CROUCH_FOV_LAYER, true);

                    controller.speedLerpModifier = crouchEnterModifier;
                    controller.fovLerpModifier = crouchEnterModifier;
                }

                yield return null;
            }
        }

        #region Event Responses
        private void OnDefaultSpeedChangedResponse()
        {
            controller.SetDesiredSpeedParameter(CROUCH_SPEED_LAYER, controller.defaultSpeed * crouchMultiplier);
        }
        #endregion
    }
}