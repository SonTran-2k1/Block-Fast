using System.Collections.Generic;
using Core.Singleton;
using UnityEngine;

public class CellManager : SingletonBase<CellManager>
{
    [SerializeField] private List<Cell> listCell;
    private Cell[,] grid;
    private const int GridSize = 8;
    
    // Cache dictionary để lookup cell theo grid position
    private Dictionary<Vector2Int, Cell> gridDict;
    private Vector3 gridOriginWorld;
    private float cellSize = 1.2f;

    void Start()
    {
        BuildGridDictionary();
    }
    
    // Build dictionary và tính cellSize từ các cell
    private void BuildGridDictionary()
    {
        gridDict = new Dictionary<Vector2Int, Cell>();
        
        if (listCell == null || listCell.Count == 0) return;
        
        // Tính grid origin và cellSize
        float minX = listCell[0].transform.position.x;
        float minY = listCell[0].transform.position.y;
        float minPositiveDx = float.MaxValue;
        float minPositiveDy = float.MaxValue;

        for (int i = 0; i < listCell.Count; i++)
        {
            Vector3 posA = listCell[i].transform.position;
            if (posA.x < minX) minX = posA.x;
            if (posA.y < minY) minY = posA.y;

            for (int j = 0; j < listCell.Count; j++)
            {
                if (i == j) continue;
                Vector3 posB = listCell[j].transform.position;
                float dx = Mathf.Abs(posA.x - posB.x);
                float dy = Mathf.Abs(posA.y - posB.y);
                if (dx > 0.0001f && dx < minPositiveDx) minPositiveDx = dx;
                if (dy > 0.0001f && dy < minPositiveDy) minPositiveDy = dy;
            }
        }

        if (minPositiveDx < float.MaxValue && minPositiveDy < float.MaxValue)
        {
            cellSize = Mathf.Min(minPositiveDx, minPositiveDy);
        }

        gridOriginWorld = new Vector3(minX, minY, 0f);
        
        // Build dictionary
        foreach (Cell cell in listCell)
        {
            Vector2Int gridPos = WorldToGrid(cell.transform.position);
            gridDict[gridPos] = cell;
        }
    }
    
    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - gridOriginWorld;
        int gridX = Mathf.RoundToInt(local.x / cellSize);
        int gridY = Mathf.RoundToInt(local.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }
    
    // Lấy Cell theo grid position
    public Cell GetCellAt(Vector2Int gridPos)
    {
        if (gridDict == null) BuildGridDictionary();
        
        if (gridDict.TryGetValue(gridPos, out Cell cell))
        {
            return cell;
        }
        return null;
    }
    
    // Lấy grid position từ world position
    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        return WorldToGrid(worldPos);
    }
    
    // Kiểm tra và clear các hàng/cột đã full
    // Trả về số hàng/cột đã clear (để tính score)
    public int CheckAndClearFullLines()
    {
        if (gridDict == null) BuildGridDictionary();
        
        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();
        
        // Check các hàng (row - cùng y)
        for (int y = 0; y < GridSize; y++)
        {
            bool rowFull = true;
            for (int x = 0; x < GridSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!gridDict.TryGetValue(pos, out Cell cell) || !cell.HasBlock())
                {
                    rowFull = false;
                    break;
                }
            }
            if (rowFull) fullRows.Add(y);
        }
        
        // Check các cột (column - cùng x)
        for (int x = 0; x < GridSize; x++)
        {
            bool colFull = true;
            for (int y = 0; y < GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!gridDict.TryGetValue(pos, out Cell cell) || !cell.HasBlock())
                {
                    colFull = false;
                    break;
                }
            }
            if (colFull) fullCols.Add(x);
        }
        
        // Clear các hàng full
        foreach (int row in fullRows)
        {
            ClearRow(row);
        }
        
        // Clear các cột full
        foreach (int col in fullCols)
        {
            ClearColumn(col);
        }
        
        int totalCleared = fullRows.Count + fullCols.Count;
        
        if (totalCleared > 0)
        {
            Debug.Log($"[CellManager] Cleared {fullRows.Count} rows and {fullCols.Count} columns!");
        }
        
        return totalCleared;
    }
    
    // Clear toàn bộ một hàng
    private void ClearRow(int row)
    {
        for (int x = 0; x < GridSize; x++)
        {
            Vector2Int pos = new Vector2Int(x, row);
            if (gridDict.TryGetValue(pos, out Cell cell))
            {
                cell.ClearBlock();
            }
        }
        Debug.Log($"[CellManager] Cleared row {row}");
    }
    
    // Clear toàn bộ một cột
    private void ClearColumn(int col)
    {
        for (int y = 0; y < GridSize; y++)
        {
            Vector2Int pos = new Vector2Int(col, y);
            if (gridDict.TryGetValue(pos, out Cell cell))
            {
                cell.ClearBlock();
            }
        }
        Debug.Log($"[CellManager] Cleared column {col}");
    }
    
    // Kiểm tra xem một shape pattern có thể đặt vào grid không (có ít nhất 1 vị trí hợp lệ)
    public bool CanShapeFitAnywhere(Vector2Int[] shapePattern)
    {
        if (shapePattern == null || shapePattern.Length == 0)
            return false;
            
        if (gridDict == null) BuildGridDictionary();
        
        // Duyệt qua tất cả các vị trí có thể trên grid
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                Vector2Int anchorPos = new Vector2Int(x, y);
                if (CanPlaceShapeAt(shapePattern, anchorPos))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Kiểm tra xem shape có thể đặt tại vị trí cụ thể không
    public bool CanPlaceShapeAt(Vector2Int[] shapePattern, Vector2Int anchorPos)
    {
        if (shapePattern == null || shapePattern.Length == 0)
            return false;
            
        if (gridDict == null) BuildGridDictionary();
        
        foreach (Vector2Int offset in shapePattern)
        {
            Vector2Int targetPos = anchorPos + offset;
            
            // Check bounds
            if (targetPos.x < 0 || targetPos.x >= GridSize ||
                targetPos.y < 0 || targetPos.y >= GridSize)
            {
                return false;
            }
            
            // Check xem cell có block chưa
            if (gridDict.TryGetValue(targetPos, out Cell cell))
            {
                if (cell.HasBlock())
                {
                    return false;
                }
            }
            else
            {
                return false; // Cell không tồn tại
            }
        }
        
        return true;
    }

    // Hàm nạp vào list
    public void AddCell(Cell cell)
    {
        if (cell != null)
        {
            listCell.Add(cell);
            // Rebuild dictionary khi thêm cell mới
            BuildGridDictionary();
        }
    }

    // Hàm get được list
    public List<Cell> GetCells()
    {
        return listCell;
    }

    // Hàm sửa
    public void EditCell(int index, Cell newCell)
    {
        if (index >= 0 && index < listCell.Count && newCell != null)
        {
            listCell[index] = newCell;
            BuildGridDictionary();
        }
    }

    // Hàm tìm
    public Cell FindCell(int index)
    {
        if (index >= 0 && index < listCell.Count)
        {
            return listCell[index];
        }

        return null;
    }

    // Hàm xóa by index
    public void RemoveCell(int index)
    {
        if (index >= 0 && index < listCell.Count)
        {
            listCell.RemoveAt(index);
            BuildGridDictionary();
        }
    }

    // Hàm xóa by cell
    public void RemoveCell(Cell cell)
    {
        if (cell != null)
        {
            listCell.Remove(cell);
            BuildGridDictionary();
        }
    }
}
