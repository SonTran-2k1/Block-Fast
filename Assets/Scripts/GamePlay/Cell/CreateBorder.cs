using UnityEngine;

public class CreateBorder : MonoBehaviour
{
    [SerializeField] Cell prefabSprite;
    [SerializeField] int gridWidth = 8;
    [SerializeField] int gridHeight = 8;
    [SerializeField] float cellSize = 1f;
    [SerializeField] float spacingX = 0f;
    [SerializeField] float spacingY = 0f;
    [SerializeField] Transform parent;
    private Cell[,] grid;
    [SerializeField] private CellManager cellManager;

    void Start()
    {
        if (parent == null)
        {
            parent = transform;
        }

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (prefabSprite == null)
        {
            Debug.LogError("[CreateBorder] PrefabSprite is missing");
            return;
        }

        grid = new Cell[gridWidth, gridHeight];

        // Step between cells includes size and spacing
        float stepX = cellSize + spacingX;
        float stepY = cellSize + spacingY;

        // Center the grid on the current transform so it sits nicely on the board background
        float widthOffset = (gridWidth - 1) * stepX * 0.5f;
        float heightOffset = (gridHeight - 1) * stepY * 0.5f;
        Vector3 startPosition = transform.position - new Vector3(widthOffset, heightOffset, 0f);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = startPosition + new Vector3(x * stepX, y * stepY, 0f);
                grid[x, y] = Instantiate(prefabSprite, position, Quaternion.identity, parent);
                cellManager.AddCell(grid[x, y]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
