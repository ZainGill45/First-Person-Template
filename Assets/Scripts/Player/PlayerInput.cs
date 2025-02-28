using UnityEngine;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Key Mappings")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        public float moveX { get; private set; }
        public float mouseX { get; private set; }
        public float moveY { get; private set; }
        public float mouseY { get; private set; }

        public bool jumpDown { get; private set; }
        public bool sprintHold { get; private set; }

        private bool inputLocked;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                inputLocked = !inputLocked;

            if (inputLocked)
            {
                moveX = 0;
                mouseX = 0;
                moveY = 0;
                mouseY = 0;

                jumpDown = false;
                sprintHold = false;

                return;
            }

            moveX = Input.GetAxis("Horizontal");
            mouseX = Input.GetAxis("Mouse X");
            moveY = Input.GetAxis("Vertical");
            mouseY = Input.GetAxis("Mouse Y");

            jumpDown = Input.GetKeyDown(jumpKey);
            sprintHold = Input.GetKey(sprintKey);
        }
    }
}
