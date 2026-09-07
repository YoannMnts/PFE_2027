using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TraversalPro.Samples.Playground
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] Transform menuParent;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] List<MenuItem> items = new();
        [SerializeField] string playerActionMap = "Player";
        [SerializeField] string uiActionMap = "UI";
        [SerializeField] Volume globalVolume;
        [SerializeField] float inputCooldown = .5f;
        [SerializeField, Min(.00001f)] float pauseTimeScale = .00001f;
        [SerializeField] InputActionReference pauseAction;
        [SerializeField] InputActionReference closeMenuAction;
        [SerializeField] InputActionReference cancelAction;
        [SerializeField] InputActionReference navigateAction;
        [SerializeField] InputActionReference submitAction;
        CursorLockMode priorCursorLockMode;
        bool wasCursorVisible;
        bool isPaused;
        int currentItemIndex;
        float cooldown;

        // PlayerInput playerInput => isFirstPersonActive ? firstPersonPlayer : thirdPersonPlayer;

        void Awake()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Hovered += UpdateHover;
            }
        }

        void Start()
        {
            if (pauseAction) pauseAction.action.performed += TogglePause;
            if (closeMenuAction) closeMenuAction.action.performed += ClosePauseMenu;
            if (cancelAction) cancelAction.action.performed += ClosePauseMenu;
            if (navigateAction) navigateAction.action.performed += Navigate;
            if (submitAction) submitAction.action.performed += Submit;
            if (!playerInput) playerInput = FindAnyObjectByType<PlayerInput>();
            if (playerInput && playerInput.isActiveAndEnabled) playerInput.SwitchCurrentActionMap(playerActionMap);
        }

        void OnDestroy()
        {
            if (pauseAction) pauseAction.action.performed -= TogglePause;
            if (closeMenuAction) closeMenuAction.action.performed -= ClosePauseMenu;
            if (cancelAction) cancelAction.action.performed -= ClosePauseMenu;
            if (navigateAction) navigateAction.action.performed -= Navigate;
            if (submitAction) submitAction.action.performed -= Submit;
        }

        void Update()
        {
            cooldown -= Time.unscaledDeltaTime;
        }

        public void Pause()
        {
            isPaused = true;
            menuParent.gameObject.SetActive(true);
            priorCursorLockMode = Cursor.lockState;
            wasCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = pauseTimeScale;
            if (playerInput && playerInput.isActiveAndEnabled) playerInput.SwitchCurrentActionMap(uiActionMap);
            if (globalVolume && globalVolume.profile.TryGet(out DepthOfField depthOfField))
            {
                depthOfField.active = true;
            }
            currentItemIndex = 0;
            UpdateHover(items[currentItemIndex]);
            foreach (MenuItem item in items)
            {
                item.CompleteAnimation();
            }
            cooldown = inputCooldown;
        }

        public void Continue()
        {
            isPaused = false;
            menuParent.gameObject.SetActive(false);
            Cursor.lockState = priorCursorLockMode;
            Cursor.visible = wasCursorVisible;
            Time.timeScale = 1;
            if (playerInput && playerInput.isActiveAndEnabled) playerInput.SwitchCurrentActionMap(playerActionMap);
            if (globalVolume && globalVolume.profile.TryGet(out DepthOfField depthOfField))
            {
                depthOfField.active = false;
            }
        }

        public void Restart()
        {
            Continue();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void TogglePause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (isPaused)
            {
                Continue();
            }
            else
            {
                Pause();
            }
        }

        public void ClosePauseMenu(InputAction.CallbackContext context)
        {
            if (!isPaused) return;
            if (!context.performed) return;
            Continue();
        }
        
        public void Submit(InputAction.CallbackContext context)
        {
            if (!isPaused) return;
            if (!context.performed) return;
            items[currentItemIndex].Click();
        }
        
        public void Navigate(InputAction.CallbackContext context)
        {
            if (!isPaused) return;
            if (cooldown > 0) return;
            Vector2 value = context.ReadValue<Vector2>();
            currentItemIndex = (int)Mathf.Round((currentItemIndex - value.y) % items.Count + items.Count) % items.Count;
            UpdateHover(items[currentItemIndex]);
        }

        void UpdateHover(MenuItem hovered)
        {
            hovered.IsHovered = true;
            currentItemIndex = items.IndexOf(hovered);
            foreach (MenuItem item in items)
            {
                if (item != hovered) item.IsHovered = false;
            }
        }
    }
}