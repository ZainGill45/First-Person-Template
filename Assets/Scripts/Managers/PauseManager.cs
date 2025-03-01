using UnityEngine;
using DG.Tweening;
using System;

namespace Managers
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CanvasGroup pauseMenu;

        [Header("General Settings")]
        [SerializeField] private Ease fadeEase = Ease.OutCubic;
        [SerializeField] private float fadeDuration = 0.3f;

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

            pauseMenu.DOFade(1f, fadeDuration).SetEase(fadeEase).SetUpdate(true);

            OnGamePaused?.Invoke();
        }

        public void PlayGame()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            gamePaused = false;
            Time.timeScale = 1f;

            pauseMenu.DOFade(0f, fadeDuration).SetEase(fadeEase).SetUpdate(true);

            OnGamePlayed?.Invoke();
        }
    }
}
