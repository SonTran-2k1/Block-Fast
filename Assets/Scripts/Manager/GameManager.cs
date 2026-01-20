using UnityEngine;
using Core.Singleton;
using Core.Managers;
namespace Core.Managers
{
    public class GameManager : SingletonBase<GameManager>
    {
        public enum GameState { Menu, Playing, Paused, GameOver, LevelComplete }
        private GameState currentState = GameState.Menu;
        public GameState CurrentState => currentState;
        protected override void OnSingletonInitialized()
        {
            SetGameState(GameState.Menu);
        }
        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnGameStateChanged(currentState);
        }
        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
                case GameState.LevelComplete:
                    Time.timeScale = 0f;
                    break;
            }
            Debug.Log($"Game State Changed: {state}");
        }
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
                SetGameState(GameState.Paused);
        }
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
                SetGameState(GameState.Playing);
        }
        public void EndGame()
        {
            SetGameState(GameState.GameOver);
        }
        public void CompleteLevel()
        {
            SetGameState(GameState.LevelComplete);
        }
    }
}
