﻿using UnityEngine;
using UnityEngine.UI;
using Core.UI;
using Core.Managers;

namespace Game.UI
{
    /// <summary>
    /// Pause popup UI for game pause menu.
    /// Provides resume, settings, and exit options.
    /// </summary>
    public class PausePopup : PopupBase
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        /// <summary>
        /// Setup button listeners
        /// </summary>
        private void SetupButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }
        }

        /// <summary>
        /// Handle resume button click
        /// </summary>
        private void OnResumeClicked()
        {
            AudioManager.Instance.PlaySFX("click"); // Play click SFX if available
            Hide();
            UIManager.Instance.HidePopup<PausePopup>();
        }

        /// <summary>
        /// Handle settings button click
        /// </summary>
        private void OnSettingsClicked()
        {
            AudioManager.Instance.PlaySFX("click");
            Debug.Log("[PausePopup] Settings clicked");
            // TODO: Show settings popup
        }

        /// <summary>
        /// Handle exit button click
        /// </summary>
        private void OnExitClicked()
        {
            AudioManager.Instance.PlaySFX("click");
            Debug.Log("[PausePopup] Exit clicked");
            UIManager.Instance.HideAllPopups();
            // TODO: Load main menu or close game
        }

        protected override void OnPopupShown()
        {
            Debug.Log("[PausePopup] Pause menu shown");
            // Pause game logic here if needed
        }

        protected override void OnPopupHidden()
        {
            Debug.Log("[PausePopup] Pause menu hidden");
            // Resume game logic here if needed
        }

        public override void OnDespawned()
        {
            // Clean up button listeners when returning to pool
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);

            base.OnDespawned();
        }
    }
}
