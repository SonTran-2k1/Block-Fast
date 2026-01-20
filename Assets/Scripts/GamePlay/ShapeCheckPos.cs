using UnityEngine;
using System.Collections.Generic;

public class ShapeCheckPos : MonoBehaviour
{
    [SerializeField] private float cellSize = 1.2f;
    [SerializeField] private float snapDistance = 0.5f; // Khoảng cách để snap vào grid

    [SerializeField] private CellManager cellManager;
    private Vector2Int[] shapePattern; // Pattern của shape (offsets)
    private Vector2Int gridOrigin; // Origin của grid (0,0)
    private Vector3 gridOriginWorld; // World position của cell (0,0)
    private Vector2Int gridSize = new Vector2Int(8, 8); // 8x8 grid
    private Vector2Int? currentHighlightedGrid; // Lưu vị trí highlight hiện tại
    private Vector3 anchorLocalOffset; // Local offset của anchor block (min corner)
    public Vector3 AnchorLocalOffset => anchorLocalOffset; // Local offset của anchor block (min corner)
    public Vector2Int? CurrentHighlightedGrid => currentHighlightedGrid;

    private void Start()
    {
        cellManager = CellManager.Instance;

        // Tính grid origin từ listCell
        CalculateGridOrigin();
    }

    // Callback khi shape được drag - PUBLIC để ShapeClick gọi
    public void CheckPositionAndHighlight(Vector3 worldPos)
    {
        // Tính vị trí world của anchor block (block ở góc min)
        Vector3 anchorWorldPos = worldPos + anchorLocalOffset;
        Vector2Int? validGridPos = GetNearestSnappablePosition(anchorWorldPos);

        // Nếu vị trí mới khác vị trí cũ thì update highlight
        if (validGridPos != currentHighlightedGrid)
        {
            ClearHighlights();

            if (validGridPos.HasValue)
            {
                HighlightValidCells(validGridPos.Value, true);
                currentHighlightedGrid = validGridPos;
                Debug.Log($"Can place shape at grid: {validGridPos.Value}");
            }
            else
            {
                currentHighlightedGrid = null;
                Debug.Log("Cannot place shape here");
            }
        }
    }

    // Tính origin của grid từ các cell
    private void CalculateGridOrigin()
    {
        var cells = cellManager.GetCells();
        if (cells.Count == 0) return;

        float minX = cells[0].transform.position.x;
        float minY = cells[0].transform.position.y;
        for (int i = 1; i < cells.Count; i++)
        {
            Vector3 pos = cells[i].transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
        }

        gridOriginWorld = new Vector3(minX, minY, 0f);
        gridOrigin = Vector2Int.zero;
    }

    // Extract shape pattern từ children (offsets) và normalize về (0,0)
    public void SetShapePattern(Transform shapeTransform)
    {
        List<Vector2Int> pattern = new List<Vector2Int>();

        // Lấy tất cả grid positions từ LOCAL positions của children (relative với parent)
        List<Vector2Int> allGridPositions = new List<Vector2Int>();
        List<Vector3> allLocalPositions = new List<Vector3>();

        foreach (Transform child in shapeTransform)
        {
            Vector3 localPos = child.localPosition;
            allLocalPositions.Add(localPos);
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(localPos.x / cellSize),
                Mathf.RoundToInt(localPos.y / cellSize)
            );
            allGridPositions.Add(gridPos);
        }

        if (allGridPositions.Count == 0)
        {
            shapePattern = new Vector2Int[0];
            anchorLocalOffset = Vector3.zero;
            return;
        }

        // Tìm min X và min Y
        int minX = allGridPositions[0].x;
        int minY = allGridPositions[0].y;
        int minIndex = 0;

        for (int i = 0; i < allGridPositions.Count; i++)
        {
            Vector2Int pos = allGridPositions[i];
            if (pos.x < minX || (pos.x == minX && pos.y < minY))
            {
                minX = pos.x;
                minY = pos.y;
                minIndex = i;
            }
        }

        // Lưu local offset của anchor block
        anchorLocalOffset = allLocalPositions[minIndex];

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
        Vector3 local = worldPos - gridOriginWorld;
        int gridX = Mathf.RoundToInt(local.x / cellSize);
        int gridY = Mathf.RoundToInt(local.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    // Convert grid position → world position
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridOriginWorld.x + gridPos.x * cellSize,
            gridOriginWorld.y + gridPos.y * cellSize,
            0f
        );
    }

    // Tìm vị trí grid gần nhất có thể snap vào
    public Vector2Int? GetNearestSnappablePosition(Vector3 worldPos)
    {
        Vector2Int currentGrid = WorldToGrid(worldPos);

        Vector2Int? nearestGrid = null;
        float nearestDistance = float.MaxValue;

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

                    // Tìm vị trí gần nhất trong snap distance
                    if (distance <= snapDistance * cellSize && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestGrid = testGrid;
                    }
                }
            }
        }

        return nearestGrid;
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
            if (targetGrid.x < 0 || targetGrid.x >= gridSize.x ||
                targetGrid.y < 0 || targetGrid.y >= gridSize.y)
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

        Color highlightColor = isValid ? new Color(0.35f, 0.95f, 0.45f) : new Color(0.2f, 0.75f, 0.95f);
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
                    cell.GetComponent<SpriteRenderer>().color = highlightColor;
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
                cell.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
}
