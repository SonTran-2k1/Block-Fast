using System;
using System.Collections.Generic;
using Core.Singleton;
using UnityEngine;

public class SpawnManager : SingletonBase<SpawnManager>
{
    [Serializable]
    private class BlockShape
    {
        public string id = "shape";
        public Vector2Int[] cells;
        [Range(1, 100)] public int weight = 10;

        public bool IsValid() => cells != null && cells.Length > 0;
        
        // Normalize cells về anchor (0,0)
        public Vector2Int[] GetNormalizedCells()
        {
            if (cells == null || cells.Length == 0) return cells;
            
            int minX = cells[0].x;
            int minY = cells[0].y;
            
            foreach (var c in cells)
            {
                if (c.x < minX) minX = c.x;
                if (c.y < minY) minY = c.y;
            }
            
            Vector2Int[] normalized = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                normalized[i] = new Vector2Int(cells[i].x - minX, cells[i].y - minY);
            }
            return normalized;
        }
    }

    [Header("Setup")] 
    [SerializeField] private Transform[] spawnSlots;
    [SerializeField] private GameObject shapeClickPrefab;
    [SerializeField] private Material materialUnlit;
    [SerializeField] private int piecesPerBatch = 3;
    [SerializeField, Range(0.25f, 2f)] private float unitSpacing = 1f;
    [SerializeField] private bool autoSpawnOnStart = true;
    [SerializeField] private List<Sprite> spriteRenderers = new List<Sprite>();
    [SerializeField] private float SpaceShape;

    [Header("Shapes (optional, uses defaults if empty)")]
    [SerializeField]
    private List<BlockShape> shapes = new List<BlockShape>();

    // Current spawned pieces tracking
    private readonly List<GameObject> currentBatchPieces = new List<GameObject>();
    private int piecesPlacedInBatch = 0;
    
    private System.Random rng;

    protected override void Awake()
    {
        base.Awake();
        rng = new System.Random(Guid.NewGuid().GetHashCode());
    }

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnBatch();
        }
    }
    
    // Gọi khi một shape được đặt xuống grid
    public void OnShapePlaced(GameObject placedShape)
    {
        currentBatchPieces.Remove(placedShape);
        piecesPlacedInBatch++;
        
        Debug.Log($"[SpawnManager] Shape placed. Remaining: {currentBatchPieces.Count}, Placed this batch: {piecesPlacedInBatch}");
        
        // Kiểm tra các shapes còn lại có fit không
        if (currentBatchPieces.Count > 0)
        {
            CheckRemainingShapesCanFit();
        }
        
        // Nếu đã thả hết 3 shapes thì spawn batch mới
        if (currentBatchPieces.Count == 0)
        {
            Debug.Log("[SpawnManager] All shapes placed! Spawning new batch...");
            piecesPlacedInBatch = 0;
            SpawnBatch();
        }
    }
    
    // Kiểm tra các shapes còn lại có thể đặt vào grid không
    private void CheckRemainingShapesCanFit()
    {
        CellManager cellManager = CellManager.Instance;
        bool anyCanFit = false;
        
        foreach (GameObject piece in currentBatchPieces)
        {
            if (piece == null) continue;
            
            ShapeCheckPos shapeCheckPos = piece.GetComponent<ShapeCheckPos>();
            if (shapeCheckPos == null) continue;
            
            Vector2Int[] pattern = shapeCheckPos.GetShapePattern();
            if (cellManager.CanShapeFitAnywhere(pattern))
            {
                anyCanFit = true;
                break;
            }
        }
        
        if (!anyCanFit)
        {
            Debug.LogError("[SpawnManager] GAME OVER! Không còn shape nào có thể đặt vào grid!");
            // TODO: Trigger Game Over UI
        }
    }

    public void SpawnBatch()
    {
        // Clear old pieces nếu còn
        foreach (var piece in currentBatchPieces)
        {
            if (piece != null) Destroy(piece);
        }
        currentBatchPieces.Clear();
        piecesPlacedInBatch = 0;

        if (spawnSlots == null || spawnSlots.Length == 0)
        {
            Debug.LogError("[SpawnManager] Spawn slots are not set");
            return;
        }

        if (shapeClickPrefab == null)
        {
            Debug.LogError("[SpawnManager] ShapeClickPrefab is not assigned");
            return;
        }

        EnsureShapeCatalog();
        
        // Tìm các shapes có thể fit vào grid
        List<BlockShape> fittingShapes = FindShapesThatFit();
        
        if (fittingShapes.Count == 0)
        {
            Debug.LogError("[SpawnManager] GAME OVER! Không có shape nào fit vào grid!");
            return;
        }
        
        Debug.Log($"[SpawnManager] Found {fittingShapes.Count} shapes that can fit on grid");
        
        // Chọn shapes cho batch này
        List<BlockShape> selectedShapes = SelectShapesForBatch(fittingShapes);

        for (int i = 0; i < selectedShapes.Count && i < spawnSlots.Length; i++)
        {
            Transform slot = spawnSlots[i];
            GameObject pieceRoot = Instantiate(shapeClickPrefab, slot, false);
            pieceRoot.name = $"Piece_{i}";

            BlockShape shape = selectedShapes[i];

            // Pick a random sprite
            Sprite randomSprite = spriteRenderers.Count > 0
                ? spriteRenderers[rng.Next(spriteRenderers.Count)]
                : null;

            // Spawn each cell of the shape with 1.2f spacing
            foreach (Vector2Int cell in shape.cells)
            {
                GameObject block = new GameObject($"Block_{cell.x}_{cell.y}");
                block.transform.SetParent(pieceRoot.transform, false);
                block.transform.localPosition = new Vector3(cell.x * 1.2f, cell.y * 1.2f, 0);
                block.AddComponent<BoxCollider2D>().size = new Vector2(1.52f, 1.52f);
                SpriteRenderer spriteRenderer = block.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerName = "CellShape";
                spriteRenderer.material = materialUnlit;
                spriteRenderer.sprite = randomSprite;
            }

            // Set shape pattern cho ShapeCheckPos
            ShapeCheckPos shapeCheckPos = pieceRoot.GetComponent<ShapeCheckPos>();
            if (shapeCheckPos != null)
            {
                shapeCheckPos.SetShapePattern(pieceRoot.transform);
            }

            currentBatchPieces.Add(pieceRoot);
        }
        
        Debug.Log($"[SpawnManager] Spawned {currentBatchPieces.Count} pieces");
    }
    
    // Tìm tất cả shapes có thể fit vào grid hiện tại
    private List<BlockShape> FindShapesThatFit()
    {
        List<BlockShape> fittingShapes = new List<BlockShape>();
        CellManager cellManager = CellManager.Instance;
        
        foreach (BlockShape shape in shapes)
        {
            Vector2Int[] normalizedCells = shape.GetNormalizedCells();
            if (cellManager.CanShapeFitAnywhere(normalizedCells))
            {
                fittingShapes.Add(shape);
            }
        }
        
        return fittingShapes;
    }
    
    // Chọn shapes cho batch - đảm bảo có ít nhất shapes có thể fit
    private List<BlockShape> SelectShapesForBatch(List<BlockShape> fittingShapes)
    {
        List<BlockShape> selected = new List<BlockShape>();
        
        // Số shapes fit cần chọn (tối thiểu 1, tối đa là số shapes fit có sẵn hoặc piecesPerBatch)
        int guaranteedFitCount = Mathf.Min(2, fittingShapes.Count, piecesPerBatch);
        
        // Random chọn các shapes fit (weighted)
        List<BlockShape> availableFitting = new List<BlockShape>(fittingShapes);
        
        for (int i = 0; i < guaranteedFitCount && availableFitting.Count > 0; i++)
        {
            BlockShape chosen = WeightedRandomSelect(availableFitting);
            selected.Add(chosen);
            // Có thể chọn lại shape cùng loại nên không remove
        }
        
        // Fill còn lại với random từ fitting shapes (vì chỉ có shapes fit mới được spawn)
        while (selected.Count < piecesPerBatch && fittingShapes.Count > 0)
        {
            BlockShape chosen = WeightedRandomSelect(fittingShapes);
            selected.Add(chosen);
        }
        
        // Shuffle để không phải lúc nào shapes fit cũng ở đầu
        ShuffleList(selected);
        
        return selected;
    }
    
    // Weighted random selection
    private BlockShape WeightedRandomSelect(List<BlockShape> shapeList)
    {
        if (shapeList.Count == 0) return null;
        if (shapeList.Count == 1) return shapeList[0];
        
        int totalWeight = 0;
        foreach (var shape in shapeList)
        {
            totalWeight += shape.weight;
        }
        
        int randomValue = rng.Next(totalWeight);
        int cumulative = 0;
        
        foreach (var shape in shapeList)
        {
            cumulative += shape.weight;
            if (randomValue < cumulative)
            {
                return shape;
            }
        }
        
        return shapeList[shapeList.Count - 1];
    }
    
    // Fisher-Yates shuffle
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void EnsureShapeCatalog()
    {
        bool hasValidShape = false;
        if (shapes != null)
        {
            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                if (shapes[i] == null || !shapes[i].IsValid())
                {
                    shapes.RemoveAt(i);
                }
            }

            hasValidShape = shapes.Count > 0;
        }

        if (hasValidShape)
        {
            return;
        }

        shapes = CreateDefaultShapes();
    }

    private List<BlockShape> CreateDefaultShapes()
    {
        return new List<BlockShape>
        {
            new BlockShape { id = "single", weight = 25, cells = new[] { new Vector2Int(0, 0) } },
            new BlockShape { id = "domino", weight = 20, cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) } },
            new BlockShape { id = "trio_line", weight = 18, cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) } },
            new BlockShape { id = "quad_square", weight = 12, cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) } },
            new BlockShape { id = "l_small", weight = 12, cells = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0) } },
            new BlockShape { id = "l_large", weight = 10, cells = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0) } },
            new BlockShape { id = "t_shape", weight = 8, cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) } },
            new BlockShape { id = "plus", weight = 6, cells = new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) } },
            new BlockShape { id = "five_line", weight = 4, cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(4, 0) } },
            new BlockShape { id = "big_square", weight = 3, cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) } }
        };
    }
}
