using Core.Singleton;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Dữ liệu của một cell - lưu sprite name và trạng thái
/// </summary>
[System.Serializable]
public class SavedCellData
{
    public int gridX;
    public int gridY;
    public string spriteName = ""; // Sprite name đang có trong cell, "" nếu trống
    public bool hasOccupyingBlock; // Có block GameObject không
}

/// <summary>
/// Dữ liệu toàn bộ game state
/// </summary>
[System.Serializable]
public class SavedGameData
{
    public List<SavedCellData> gridCells = new List<SavedCellData>();
    public int score = 0;
    public int bestScore = 0;
    public int combo = 0;
    public long savedTimestamp = 0; // Thời điểm lưu
}

public class GameFileManager : SingletonBase<GameFileManager>
{
    private const string SaveFileName = "gamestate.json";
    private string savePath;

    [Header("Sprite Library (assign all block sprites here)")]
    [SerializeField]
    private List<Sprite> spriteLibrary = new List<Sprite>();

    private Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();

    protected override void Awake()
    {
        base.Awake();

        // Lưu vào persistent data path (Documents trên PC, app folder trên mobile)
        savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        Debug.Log($"[GameFileManager] Save path: {savePath}");

        // Build lookup for sprites (name -> sprite)
        spriteLookup.Clear();
        foreach (var s in spriteLibrary)
        {
            if (s != null && !spriteLookup.ContainsKey(s.name))
            {
                spriteLookup.Add(s.name, s);
            }
        }
    }

    /// <summary>
    /// Lưu toàn bộ game state vào file JSON
    /// </summary>
    public void SaveGame()
    {
        try
        {
            SavedGameData gameData = new SavedGameData();

            // Lưu toàn bộ grid state
            CellManager cellManager = CellManager.Instance;
            List<Cell> cells = cellManager.GetCells();

            // Iterate qua từng cell và lấy grid position thực sự từ CellManager
            foreach (Cell cell in cells)
            {
                SavedCellData cellData = new SavedCellData();

                // Lấy grid position thực sự từ world position của cell
                Vector2Int gridPos = cellManager.GetGridPosition(cell.transform.position);
                cellData.gridX = gridPos.x;
                cellData.gridY = gridPos.y;
                cellData.hasOccupyingBlock = cell.HasBlock();

                // Chỉ lưu sprite name từ occupyingBlock (block được đặt lên cell)
                // KHÔNG lấy sprite của Cell vì đó là sprite nền (gameplay_cell_mid)
                string spriteName = "";

                if (cellData.hasOccupyingBlock && cell.OccupyingBlock != null)
                {
                    var sr = cell.OccupyingBlock.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        spriteName = sr.sprite.name;
                    }
                }

                cellData.spriteName = spriteName;

                // Chỉ log nếu cell có block
                if (!string.IsNullOrEmpty(spriteName))
                {
                    Debug.Log($"[GameFileManager] Cell ({gridPos.x}, {gridPos.y}): Saving sprite '{cellData.spriteName}'");
                }

                gameData.gridCells.Add(cellData);
            }

            // Lưu score, combo
            gameData.score = ScoreManager.Instance.CurrentScore;
            gameData.bestScore = BestScoreManager.Instance.BestScore;
            gameData.combo = ComboManager.Instance.CurrentCombo;
            gameData.savedTimestamp = System.DateTime.Now.Ticks;

            // Serialize to JSON
            string json = JsonUtility.ToJson(gameData, true);

            // Ghi vào file
            File.WriteAllText(savePath, json);

            Debug.Log($"[GameFileManager] Game saved successfully! Path: {savePath}");
            Debug.Log($"[GameFileManager] Saved {gameData.gridCells.Count} cells, Score: {gameData.score}, Combo: {gameData.combo}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameFileManager] Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// Load game state từ file JSON
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                Debug.Log("[GameFileManager] No save file found!");
                return false;
            }

            string json = File.ReadAllText(savePath);
            SavedGameData gameData = JsonUtility.FromJson<SavedGameData>(json);

            if (gameData == null)
            {
                Debug.LogError("[GameFileManager] Failed to deserialize game data!");
                return false;
            }

            // Restore grid state - set sprites vào cells
            RestoreGridCells(gameData.gridCells);

            // Restore score, combo
            ScoreManager.Instance.CurrentScore = gameData.score;
            ComboManager.Instance.CurrentCombo = gameData.combo;

            Debug.Log($"[GameFileManager] Game loaded! Score: {gameData.score}, Combo: {gameData.combo}, Cells: {gameData.gridCells.Count}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameFileManager] Failed to load game: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Restore grid cells với sprites được lưu
    /// </summary>
    private void RestoreGridCells(List<SavedCellData> cellDataList)
    {
        CellManager cellManager = CellManager.Instance;

        if (cellManager == null)
        {
            Debug.LogError("[GameFileManager] CellManager.Instance is NULL! Cannot restore cells.");
            return;
        }

        List<Cell> allCells = cellManager.GetCells();
        Debug.Log($"[GameFileManager] CellManager has {allCells.Count} cells available");
        Debug.Log($"[GameFileManager] Sprite lookup has {spriteLookup.Count} sprites");

        int restoredCount = 0;
        int cellsWithSprite = 0;

        foreach (SavedCellData cellData in cellDataList)
        {
            if (!string.IsNullOrEmpty(cellData.spriteName))
            {
                cellsWithSprite++;
            }

            Vector2Int gridPos = new Vector2Int(cellData.gridX, cellData.gridY);
            Cell cell = cellManager.GetCellAt(gridPos);

            if (cell == null)
            {
                Debug.LogWarning($"[GameFileManager] Cell at ({cellData.gridX}, {cellData.gridY}) is NULL!");
                continue;
            }

            if (!string.IsNullOrEmpty(cellData.spriteName))
            {
                // Lookup sprite trong thư viện đã serialized (không phụ thuộc Resources)
                Sprite sprite = null;
                spriteLookup.TryGetValue(cellData.spriteName, out sprite);

                if (sprite != null)
                {
                    // Tạo một block runtime đơn giản và gán vào cell
                    GameObject block = new GameObject($"Block_{cellData.spriteName}");
                    var sr = block.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingLayerName = "CellShape";
                    sr.sortingOrder = 0;
                    cell.SetOccupyingBlock(block);

                    Debug.Log($"[GameFileManager] Cell ({cellData.gridX}, {cellData.gridY}): Restored sprite '{cellData.spriteName}'");
                    restoredCount++;
                }
                else
                {
                    Debug.LogWarning($"[GameFileManager] Could not find sprite '{cellData.spriteName}' in spriteLookup. Available: {string.Join(", ", spriteLookup.Keys)}");
                }
            }
            else
            {
                // Cell trống - không set sprite
                cell.SetBlock(null);
            }
        }

        Debug.Log($"[GameFileManager] Grid cells restoration completed! Restored {restoredCount}/{cellsWithSprite} cells with sprites");
    }

    /// <summary>
    /// Kiểm tra xem có save file không
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    /// <summary>
    /// Xóa save file (khi game over hoặc reset)
    /// </summary>
    public void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("[GameFileManager] Save file deleted!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameFileManager] Failed to delete save file: {e.Message}");
        }
    }

    /// <summary>
    /// Lấy thông tin save file (debug)
    /// </summary>
    public string GetSaveFileInfo()
    {
        if (!HasSaveFile())
            return "No save file";

        FileInfo fileInfo = new FileInfo(savePath);
        return $"Save file: {fileInfo.Name}, Size: {fileInfo.Length} bytes, Modified: {fileInfo.LastWriteTime}";
    }
}
