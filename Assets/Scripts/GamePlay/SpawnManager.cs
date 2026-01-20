using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Serializable]
    private class BlockShape
    {
        public string id = "shape";
        public Vector2Int[] cells;
        [Range(1, 100)] public int weight = 10;

        public bool IsValid() => cells != null && cells.Length > 0;
    }

    [Header("Setup")] [SerializeField] private Transform[] spawnSlots;
    [SerializeField] private int piecesPerBatch = 3;
    [SerializeField, Range(0.25f, 2f)] private float unitSpacing = 1f;
    [SerializeField] private bool autoSpawnOnStart = true;
    [SerializeField] private List<Sprite> spriteRenderers = new List<Sprite>();
    [SerializeField] private float SpaceShape;

    [Header("Shapes (optional, uses defaults if empty)")]
    [SerializeField]
    private List<BlockShape> shapes = new List<BlockShape>();

    private readonly List<GameObject> spawnedPieces = new List<GameObject>();
    private System.Random rng;

    private void Awake()
    {
        rng = new System.Random(Guid.NewGuid().GetHashCode());
    }

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnBatch();
        }
    }

    public void SpawnBatch()
    {
        if (spawnSlots == null || spawnSlots.Length == 0)
        {
            Debug.LogError("[SpawnManager] Spawn slots are not set");
            return;
        }

        EnsureShapeCatalog();

        for (int i = 0; i < piecesPerBatch && i < spawnSlots.Length; i++)
        {
            Transform slot = spawnSlots[i];
            GameObject pieceRoot = new GameObject($"Piece_{i}");
            pieceRoot.transform.SetParent(slot, false);

            // Pick a random shape
            BlockShape randomShape = shapes[rng.Next(shapes.Count)];

            // Pick a random sprite
            Sprite randomSprite = spriteRenderers.Count > 0
                ? spriteRenderers[rng.Next(spriteRenderers.Count)]
                : null;

            // Spawn each cell of the shape with 1.2f spacing
            foreach (Vector2Int cell in randomShape.cells)
            {
                GameObject block = new GameObject($"Block_{cell.x}_{cell.y}");
                block.transform.SetParent(pieceRoot.transform, false);
                block.transform.localPosition = new Vector3(cell.x * 1.2f, cell.y * 1.2f, 0);

                SpriteRenderer spriteRenderer = block.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = randomSprite;
            }

            spawnedPieces.Add(pieceRoot);
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
