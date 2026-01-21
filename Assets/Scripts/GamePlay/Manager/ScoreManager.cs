using Core.Singleton;
using UnityEngine;

public class ScoreManager : SingletonBase<ScoreManager>
{
    private int currentScore = 0;
    
    // Event để UI subscribe
    public delegate void OnScoreChanged(int newScore);
    public event OnScoreChanged ScoreChanged;

    public int CurrentScore => currentScore;

    // Thêm điểm từ block kéo vào (1 block = 1 điểm * combo multiplier)
    public void AddBlockScore(int blockCount = 1)
    {
        int comboMultiplier = ComboManager.Instance.GetComboMultiplier();
        int scoreToAdd = blockCount * comboMultiplier;
        
        currentScore += scoreToAdd;
        
        Debug.Log($"[ScoreManager] Added {scoreToAdd} points ({blockCount} blocks × {comboMultiplier} combo). Total: {currentScore}");
        ScoreChanged?.Invoke(currentScore);
        
        // Check best score khi thả shape xuống
        BestScoreManager.Instance.CheckAndUpdateBestScore(currentScore);
    }

    // Thêm điểm từ clear hàng/cột (1 hàng/cột = 8 điểm * combo multiplier)
    public void AddClearLineScore(int lineCount = 1)
    {
        int comboMultiplier = ComboManager.Instance.GetComboMultiplier();
        int scoreToAdd = lineCount * 8 * comboMultiplier; // 8 ô = 1 hàng/cột
        
        currentScore += scoreToAdd;
        
        Debug.Log($"[ScoreManager] Cleared {lineCount} lines! Added {scoreToAdd} points ({lineCount} × 8 × {comboMultiplier} combo). Total: {currentScore}");
        ScoreChanged?.Invoke(currentScore);
        
        // Check best score
        BestScoreManager.Instance.CheckAndUpdateBestScore(currentScore);
    }

    // Reset score (khi game over hoặc restart)
    public void ResetScore()
    {
        currentScore = 0;
        ScoreChanged?.Invoke(currentScore);
        Debug.Log("[ScoreManager] Score reset to 0");
    }
}
