using UnityEngine;
using Core.Singleton;

namespace Core.Managers
{
    /// <summary>
    /// Game state management - tracks pause, active gameplay state.
    /// </summary>
    public enum GameState
    {
        Menu,
        Loading,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }

    /// <summary>
    /// Centralized game state manager.
    /// Controls pause state and current game phase.
    /// </summary>
    public class GameStateManager : SingletonBase<GameStateManager>
    {
        private GameState currentState = GameState.Menu;
        private GameState previousState = GameState.Menu;
        private bool isPaused = false;

        // Events for state changes
        public delegate void StateEventHandler(GameState newState, GameState oldState);
        public event StateEventHandler OnStateChanged;

        public delegate void PauseEventHandler(bool isPaused);
        public event PauseEventHandler OnPauseStateChanged;

        public GameState CurrentState => currentState;
        public GameState PreviousState => previousState;
        public bool IsPaused => isPaused;

        /// <summary>
        /// Change game state
        /// </summary>
        public void SetState(GameState newState)
        {
            if (currentState == newState)
                return;

            previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameStateManager] State changed: {previousState} -> {currentState}");
            OnStateChanged?.Invoke(currentState, previousState);

            // Handle pause state based on game state
            if (newState == GameState.Paused)
            {
                SetPaused(true);
            }
            else if (previousState == GameState.Paused && newState == GameState.Playing)
            {
                SetPaused(false);
            }
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!isPaused);
        }

        /// <summary>
        /// Set pause state explicitly
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (isPaused == paused)
                return;

            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;

            Debug.Log($"[GameStateManager] Game {(paused ? "PAUSED" : "RESUMED")}");
            OnPauseStateChanged?.Invoke(isPaused);
        }

        /// <summary>
        /// Reset to menu state
        /// </summary>
        public void ReturnToMenu()
        {
            SetPaused(false);
            SetState(GameState.Menu);
        }

        /// <summary>
        /// Start gameplay
        /// </summary>
        public void StartLevel()
        {
            SetPaused(false);
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Complete current level
        /// </summary>
        public void LevelComplete()
        {
            SetPaused(true);
            SetState(GameState.LevelComplete);
        }

        /// <summary>
        /// Fail current level
        /// </summary>
        public void GameOver()
        {
            SetPaused(true);
            SetState(GameState.GameOver);
        }

        /// <summary>
        /// Get state info for debugging
        /// </summary>
        public string GetStateInfo()
        {
            return $"[GameStateManager] Current: {currentState} | Paused: {isPaused} | TimeScale: {Time.timeScale}";
        }

        protected override void OnSingletonInitialized()
        {
            // Ensure timeScale is reset
            Time.timeScale = 1f;
        }

        protected override void OnSingletonDestroyed()
        {
            // Reset timeScale when destroyed
            Time.timeScale = 1f;
        }
    }
}
