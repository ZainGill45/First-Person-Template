using UnityEngine;
using System;

namespace Managers
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CanvasGroup pauseMenu;

        public static PauseManager instance { get; private set; }

        public static event Action OnGamePaused;
        public static event Action OnGamePlayed;

        public bool gamePaused { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            PlayGame();
        }

        private void Update()
        {
            if (InputManager.pauseDown)
            {
                gamePaused = !gamePaused;

                if (gamePaused)
                {
                    PauseGame();
                }
                else
                {
                    PlayGame();
                }
            }
        }

        public void PauseGame()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            gamePaused = true;

            Time.timeScale = 0f;
            pauseMenu.alpha = 1f;

            OnGamePaused?.Invoke();
        }

        public void PlayGame()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            gamePaused = false;

            Time.timeScale = 1f;
            pauseMenu.alpha = 0f;

            OnGamePlayed?.Invoke();
        }
    }
}
