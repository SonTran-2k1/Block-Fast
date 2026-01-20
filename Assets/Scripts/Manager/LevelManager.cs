﻿using UnityEngine;
using Core.Singleton;

namespace Core.Managers
{
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int targetScore;
        public int moves;
        public float difficulty;
    }

    /// <summary>
    /// Manages game level progression, data, and state transitions.
    /// </summary>
    public class LevelManager : SingletonBase<LevelManager>
    {
        [SerializeField] private LevelData[] levels;
        private int currentLevelIndex = 0;
        private LevelData currentLevel;

        // Events for level state changes
        public delegate void LevelEventHandler(int levelIndex);
        public event LevelEventHandler OnLevelLoaded;
        public event LevelEventHandler OnLevelCompleted;
        public event LevelEventHandler OnLevelFailed;

        public int CurrentLevel => currentLevelIndex + 1;
        public LevelData CurrentLevelData => currentLevel;
        public int TotalLevels => levels.Length;
        public bool IsLastLevel => currentLevelIndex >= levels.Length - 1;

        protected override void OnSingletonInitialized()
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogError("[LevelManager] No levels configured!");
                return;
            }
            LoadLevel(0);
        }

        /// <summary>
        /// Load a specific level by index
        /// </summary>
        public void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Length)
            {
                Debug.LogError($"[LevelManager] Level {index} does not exist");
                return;
            }

            currentLevelIndex = index;
            currentLevel = levels[index];

            Debug.Log($"[LevelManager] Loaded Level {CurrentLevel}: Target={currentLevel.targetScore}, " +
                     $"Moves={currentLevel.moves}, Difficulty={currentLevel.difficulty}");

            OnLevelLoaded?.Invoke(currentLevelIndex);
        }

        /// <summary>
        /// Load the next level. Wraps to first level if at end.
        /// </summary>
        public void NextLevel()
        {
            if (currentLevelIndex < levels.Length - 1)
            {
                LoadLevel(currentLevelIndex + 1);
            }
            else
            {
                Debug.Log("[LevelManager] All levels completed! Restarting from level 1...");
                LoadLevel(0);
            }
        }

        /// <summary>
        /// Restart the current level
        /// </summary>
        public void RestartLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        /// <summary>
        /// Notify that current level was completed successfully
        /// </summary>
        public void CompleteLevel()
        {
            Debug.Log($"[LevelManager] Level {CurrentLevel} completed!");
            OnLevelCompleted?.Invoke(currentLevelIndex);
        }

        /// <summary>
        /// Notify that current level was failed
        /// </summary>
        public void FailLevel()
        {
            Debug.Log($"[LevelManager] Level {CurrentLevel} failed!");
            OnLevelFailed?.Invoke(currentLevelIndex);
        }

        /// <summary>
        /// Jump to a specific level (for testing)
        /// </summary>
        public void JumpToLevel(int levelNumber)
        {
            LoadLevel(levelNumber - 1);
        }

        /// <summary>
        /// Get level data without loading it
        /// </summary>
        public LevelData GetLevelData(int levelNumber)
        {
            int index = levelNumber - 1;
            if (index >= 0 && index < levels.Length)
            {
                return levels[index];
            }
            return null;
        }
    }
}
