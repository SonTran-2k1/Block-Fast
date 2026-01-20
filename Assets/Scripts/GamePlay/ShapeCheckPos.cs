using UnityEngine;
using System.Collections.Generic;

public class ShapeCheckPos : MonoBehaviour
{
    [SerializeField] private float cellSize = 1.2f;
    [SerializeField] private float snapDistance = 0.5f; // Khoảng cách để snap vào grid

    [SerializeField] private CellManager cellManager;
    private Vector2Int[] shapePattern; // Pattern của shape (offsets)
    private Vector2Int gridOrigin; // Origin của grid (0,0)
    private Vector2Int gridSize = new Vector2Int(8, 8); // 8x8 grid

    private void Start()
    {
        cellManager = CellManager.Instance;

        // Tính grid origin từ listCell
        CalculateGridOrigin();
    }

    // Tính origin của grid từ các cell
    private void CalculateGridOrigin()
    {
        var cells = cellManager.GetCells();
        if (cells.Count == 0) return;

        Vector3 minPos = cells[0].transform.position;
        gridOrigin = WorldToGrid(minPos);
    }

    // Extract shape pattern từ children (offsets) và normalize về (0,0)
    public void SetShapePattern(Transform shapeTransform)
    {
        List<Vector2Int> pattern = new List<Vector2Int>();
        Vector3 shapeCenter = shapeTransform.position;

        // Lấy tất cả grid positions của children
        List<Vector2Int> allGridPositions = new List<Vector2Int>();
        foreach (Transform child in shapeTransform)
        {
            Vector3 childWorldPos = child.position;
            Vector2Int gridPos = WorldToGrid(childWorldPos);
            allGridPositions.Add(gridPos);
        }

        if (allGridPositions.Count == 0)
        {
            shapePattern = new Vector2Int[0];
            return;
        }

        // Tìm min X và min Y
        int minX = allGridPositions[0].x;
        int minY = allGridPositions[0].y;
        
        foreach (Vector2Int pos in allGridPositions)
        {
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
        }

        // Normalize bằng cách trừ đi min values
        foreach (Vector2Int pos in allGridPositions)
        {
            Vector2Int normalized = new Vector2Int(pos.x - minX, pos.y - minY);
            pattern.Add(normalized);
        }

        shapePattern = pattern.ToArray();
    }

    // Convert world position → grid position
    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int gridX = Mathf.RoundToInt(worldPos.x / cellSize);
        int gridY = Mathf.RoundToInt(worldPos.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    // Convert grid position → world position
    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }

    // Tìm vị trí grid gần nhất có thể snap vào
    public Vector2Int? GetNearestSnappablePosition(Vector3 worldPos)
    {
        Vector2Int currentGrid = WorldToGrid(worldPos);

        // Duyệt qua tất cả các vị trí grid trong bán kính snap
        for (int x = currentGrid.x - 2; x <= currentGrid.x + 2; x++)
        {
            for (int y = currentGrid.y - 2; y <= currentGrid.y + 2; y++)
            {
                Vector2Int testGrid = new Vector2Int(x, y);

                // Check xem shape có fit vào vị trí này không
                if (CanPlaceShape(testGrid))
                {
                    Vector3 testWorldPos = GridToWorld(testGrid);
                    float distance = Vector3.Distance(worldPos, testWorldPos);

                    if (distance <= snapDistance * cellSize)
                    {
                        return testGrid;
                    }
                }
            }
        }

        return null;
    }

    // Check xem shape có fit vào grid position không
    public bool CanPlaceShape(Vector2Int gridPosition)
    {
        if (shapePattern == null || shapePattern.Length == 0)
            return false;

        var cells = cellManager.GetCells();
        Dictionary<Vector2Int, Cell> gridDict = BuildGridDictionary(cells);

        // Check từng cell của shape
        foreach (Vector2Int offset in shapePattern)
        {
            Vector2Int targetGrid = gridPosition + offset;

            // Check bounds
            if (targetGrid.x < gridOrigin.x || targetGrid.x >= gridOrigin.x + gridSize.x ||
                targetGrid.y < gridOrigin.y || targetGrid.y >= gridOrigin.y + gridSize.y)
            {
                return false; // Out of bounds
            }

            // Check xem cell này có block rồi chưa
            if (gridDict.ContainsKey(targetGrid))
            {
                Cell cell = gridDict[targetGrid];
                if (cell.HasBlock())
                {
                    return false; // Cell đã có block
                }
            }
        }

        return true;
    }

    // Build dictionary từ listCell để dễ lookup
    private Dictionary<Vector2Int, Cell> BuildGridDictionary(List<Cell> cells)
    {
        Dictionary<Vector2Int, Cell> dict = new Dictionary<Vector2Int, Cell>();

        foreach (Cell cell in cells)
        {
            Vector2Int gridPos = WorldToGrid(cell.transform.position);
            dict[gridPos] = cell;
        }

        return dict;
    }

    // Highlight các cell có thể gắn vào
    public void HighlightValidCells(Vector2Int gridPosition, bool isValid)
    {
        if (shapePattern == null)
            return;

        var cells = cellManager.GetCells();
        Dictionary<Vector2Int, Cell> gridDict = BuildGridDictionary(cells);

        Color highlightColor = isValid ? Color.green : Color.red;
        highlightColor.a = 0.5f;

        foreach (Vector2Int offset in shapePattern)
        {
            Vector2Int targetGrid = gridPosition + offset;

            if (gridDict.ContainsKey(targetGrid))
            {
                Cell cell = gridDict[targetGrid];
                if (cell.GetComponent<SpriteRenderer>() != null)
                {
                    // TODO: Implement highlight (có thể dùng Color hoặc overlay)
                    // cell.GetComponent<SpriteRenderer>().color = highlightColor;
                }
            }
        }
    }

    // Clear highlights
    public void ClearHighlights()
    {
        var cells = cellManager.GetCells();
        foreach (Cell cell in cells)
        {
            if (cell.GetComponent<SpriteRenderer>() != null)
            {
                // cell.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
}
