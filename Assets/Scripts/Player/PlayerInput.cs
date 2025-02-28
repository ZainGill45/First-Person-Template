using UnityEngine;
using Managers;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Key Mappings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        public float moveX { get; private set; }
        public float mouseX { get; private set; }
        public float moveY { get; private set; }
        public float mouseY { get; private set; }

        public bool pauseDown { get; private set; }
        public bool jumpDown { get; private set; }
        public bool sprintHold { get; private set; }

        private void OnEnable()
        {
            PauseManager.OnGamePaused += OnGamePausedReponse;
        }
        private void OnDisable()
        {
            PauseManager.OnGamePaused -= OnGamePausedReponse;
        }

        private void Update()
        {
            pauseDown = Input.GetKeyDown(pauseKey);

            if (PauseManager.instance.gamePaused)
                return;

            moveX = Input.GetAxisRaw("Horizontal");
            mouseX = Input.GetAxisRaw("Mouse X");
            moveY = Input.GetAxisRaw("Vertical");
            mouseY = Input.GetAxisRaw("Mouse Y");

            jumpDown = Input.GetKeyDown(jumpKey);
            sprintHold = Input.GetKey(sprintKey);
        }

        private void OnGamePausedReponse()
        {
            moveX = 0f;
            mouseX = 0f;
            moveY = 0f;
            mouseY = 0f;

            jumpDown = false;
            sprintHold = false;
        }
    }
}
