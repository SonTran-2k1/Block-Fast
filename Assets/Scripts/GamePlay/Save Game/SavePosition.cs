using Core.Singleton;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CellData
{
    public int gridX;
    public int gridY;
    public bool hasBlock;
    public string blockName;
    public int blockChildCount; // Số lượng block con
    public float blockLocalScaleX;
    public float blockLocalScaleY;
    public float blockLocalScaleZ;
}

[System.Serializable]
public class GameStateData
{
    public List<CellData> cellDataList = new List<CellData>();
    public int currentScore = 0;
    public int bestScore = 0;
    public int currentCombo = 0;
}

public class SavePosition : SingletonBase<SavePosition>
{
    private const string SaveKey = "GAME_STATE_DATA";
    private GameStateData gameStateData;

    [Header("Auto Load Config")] [SerializeField] private bool autoLoadOnStart = true;

    private bool hasLoadedOnce = false;

    protected override void Awake()
    {
        base.Awake();
        gameStateData = new GameStateData();
    }

    private void Start()
    {
        // Subscribe to relevant events để tự động save khi có thay đổi
        if (ScoreManager.IsInitialized)
        {
            ScoreManager.Instance.ScoreChanged += OnScoreChanged;
        }

        if (ComboManager.IsInitialized)
        {
            ComboManager.Instance.ComboChanged += OnComboChanged;
        }

        // Auto-load với delay để đảm bảo CellManager đã sẵn sàng
        if (autoLoadOnStart)
        {
            StartCoroutine(DelayedLoad());
        }
    }

    /// <summary>
    /// Delay load 1 frame để đảm bảo các Manager khác đã Start() xong
    /// </summary>
    private IEnumerator DelayedLoad()
    {
        // Chờ end of frame để tất cả scripts đã Start() xong
        yield return new WaitForEndOfFrame();

        // Chờ thêm 1 frame nữa cho chắc
        yield return null;

        Debug.Log("[SavePosition] Attempting delayed load...");
        TryLoadSavedGame();
    }

    /// <summary>
    /// Lưu trạng thái toàn bộ game (grid, score, combo) vào JSON file
    /// </summary>
    public void SaveGameState()
    {
        GameFileManager.Instance.SaveGame();
    }

    /// <summary>
    /// Load trạng thái game từ JSON file
    /// </summary>
    public void LoadGameState()
    {
        if (GameFileManager.Instance.LoadGame())
        {
            hasLoadedOnce = true;
        }
    }

    /// <summary>
    /// Thử load nếu có file; tránh load lặp nhiều lần
    /// </summary>
    public void TryLoadSavedGame()
    {
        if (hasLoadedOnce)
        {
            return;
        }

        if (GameFileManager.Instance.HasSaveFile())
        {
            Debug.Log("[SavePosition] Found saved game file. Loading...");
            LoadGameState();
        }
        else
        {
            Debug.Log("[SavePosition] No saved game file found. Starting fresh game.");
        }
    }

    /// <summary>
    /// Xóa saved game state (khi game over)
    /// </summary>
    public void DeleteSavedGameState()
    {
        GameFileManager.Instance.DeleteSaveFile();
    }

    /// <summary>
    /// Kiểm tra xem có saved game state không
    /// </summary>
    public bool HasSavedGameState()
    {
        return GameFileManager.Instance.HasSaveFile();
    }

    /// <summary>
    /// Callback khi score thay đổi - tự động save
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        SaveGameState();
    }

    /// <summary>
    /// Callback khi combo thay đổi - tự động save
    /// </summary>
    private void OnComboChanged(int newCombo)
    {
        SaveGameState();
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        if (ScoreManager.IsInitialized)
        {
            ScoreManager.Instance.ScoreChanged -= OnScoreChanged;
        }

        if (ComboManager.IsInitialized)
        {
            ComboManager.Instance.ComboChanged -= OnComboChanged;
        }
    }
}
