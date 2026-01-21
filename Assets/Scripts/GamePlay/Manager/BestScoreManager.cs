using Core.Singleton;
using UnityEngine;

public class BestScoreManager : SingletonBase<BestScoreManager>
{
    private const string BestScoreKey = "BEST_SCORE";
    private int bestScore = 0;

    public int BestScore => bestScore;
    
    // Event khi best score update
    public delegate void OnBestScoreUpdated(int newBestScore);
    public event OnBestScoreUpdated BestScoreUpdated;

    protected override void Awake()
    {
        base.Awake();
        LoadBestScore();
    }

    // Load best score từ PlayerPrefs
    private void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        Debug.Log($"[BestScoreManager] Loaded best score: {bestScore}");
    }

    // Check và update best score nếu current score cao hơn
    public void CheckAndUpdateBestScore(int currentScore)
    {
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
            
            Debug.Log($"[BestScoreManager] New best score! {bestScore}");
            BestScoreUpdated?.Invoke(bestScore);
        }
    }

    // Reset best score (debug)
    public void ResetBestScore()
    {
        bestScore = 0;
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.Save();
        Debug.Log("[BestScoreManager] Best score reset!");
    }
}
