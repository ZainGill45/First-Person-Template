using UnityEngine;
using Player;
using System;

namespace Managers
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private PlayerInput playerInput;

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
            if (playerInput.pauseDown)
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
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;

            OnGamePaused?.Invoke();
        }

        public void PlayGame()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            gamePaused = false;
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;

            OnGamePlayed?.Invoke();
        }
    }
}
