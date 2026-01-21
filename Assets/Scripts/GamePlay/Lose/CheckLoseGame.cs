using Core.Managers;
using UnityEngine;

/// <summary>
/// Centralized lose handler: listens for game over events and shows the lose popup.
/// Keeps SpawnManager free of UI logic (SOLID: separation of concerns).
/// </summary>
public class CheckLoseGame : MonoBehaviour
{
    [Header("Lose Popup Reference")] [SerializeField] private LosePopup losePopup;
    [SerializeField] private bool deleteSaveOnGameOver = true;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (SpawnManager.Instance)
        {
            SpawnManager.Instance.GameOverDetected += OnGameOver;
        }
    }

    private void Unsubscribe()
    {
        if (SpawnManager.Instance)
        {
            SpawnManager.Instance.GameOverDetected -= OnGameOver;
        }
    }

    private void OnGameOver()
    {
        if (hasTriggered)
        {
            return;
        }

        hasTriggered = true;

        // Update global game state
        if (GameStateManager.Instance)
        {
            GameStateManager.Instance.GameOver();
        }
        else if (GameManager.Instance)
        {
            GameManager.Instance.EndGame();
        }

        // Clear saved run if configured
        if (deleteSaveOnGameOver && SavePosition.Instance)
        {
            SavePosition.Instance.DeleteSavedGameState();
        }

        ShowLosePopup();
    }

    private void ShowLosePopup()
    {
        if (losePopup != null)
        {
            losePopup.Show();
            return;
        }
    }
}
