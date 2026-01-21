using Core.Managers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple Lose Popup - Show/Hide trực tiếp, không animation.
/// </summary>
public class LosePopup : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private CanvasGroup backgroundCanvasGroup;

    [Header("Popup Content")]
    [SerializeField] private RectTransform popupContent;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        SetupHiddenState();
    }

    private void OnEnable()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    private void OnDisable()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }
    }

    private void SetupHiddenState()
    {
        if (backgroundCanvasGroup != null)
        {
            backgroundCanvasGroup.alpha = 0f;
        }

        if (popupContent != null)
        {
            popupContent.localScale = Vector3.zero;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (backgroundCanvasGroup != null)
        {
            backgroundCanvasGroup.alpha = 1f;
        }

        if (popupContent != null)
        {
            popupContent.localScale = Vector3.one;
        }
    }

    public void Hide()
    {
        SetupHiddenState();
        gameObject.SetActive(false);
    }

    private void OnRestartClicked()
    {
        // Ẩn popup TRƯỚC
        Hide();

        // Resume time TRƯỚC khi làm gì khác
        Time.timeScale = 1f;

        // Clear save
        if (SavePosition.Instance != null)
        {
            SavePosition.Instance.DeleteSavedGameState();
        }

        // Reset board
        if (CellManager.Instance != null)
        {
            CellManager.Instance.ResetBoard();
        }

        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Reset combo
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }

        // Set game state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.StartLevel();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        // Spawn mới
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.RestartAfterGameOver();
        }
    }
}
