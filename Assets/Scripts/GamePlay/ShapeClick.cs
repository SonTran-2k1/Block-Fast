using System;
using System.Collections.Generic;
using Core.Managers;
using UnityEngine;
using DG.Tweening;

public class ShapeClick : MonoBehaviour
{
    [SerializeField] private Vector2 dragOffset = Vector2.zero;
    [SerializeField] private float returnDuration = 0.3f;

    private Vector3 originalPosition;
    private bool isDragging = false;
    private Tween returnTween;
    private ShapeCheckPos shapeCheckPos;
    private ShapeSnap _shapeSnap;

    private void Start()
    {
        originalPosition = transform.position;
        shapeCheckPos = GetComponent<ShapeCheckPos>();
        _shapeSnap = GetComponent<ShapeSnap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                // Check nếu hit object chính nó hoặc là con của nó
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    Debug.Log("Clicked on " + gameObject.name);
                    isDragging = true;
                    AudioManager.Instance.PlaySFX("Select");
                    SetSortingOrderForAllSprites(hit.collider.transform.parent.gameObject, 1);

                    // Kill return tween nếu đang chạy
                    returnTween?.Kill();
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = (Vector3)mousePos + (Vector3)dragOffset;

            // Gọi trực tiếp ShapeCheckPos trên cùng object
            if (shapeCheckPos != null)
            {
                shapeCheckPos.CheckPositionAndHighlight(transform.position);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            bool snapped = false;
            if (_shapeSnap != null)
            {
                if (_shapeSnap.TryGetSnappedWorldPosition(out Vector3 snapPos))
                {
                    returnTween?.Kill();
                    AudioManager.Instance.PlaySFX("Drop");
                    returnTween = transform.DOMove(snapPos, returnDuration)
                        .OnComplete(() =>
                        {
                            // Sau khi snap animation xong, gán block vào cell
                            AssignBlocksToCells();
                            SetSortingOrderForAllSprites(gameObject, 0);

                            // Kiểm tra và clear hàng/cột full
                            int clearedLines = CellManager.Instance.CheckAndClearFullLines();

                            if (clearedLines > 0)
                            {
                                Debug.Log($"[ShapeClick] Cleared {clearedLines} lines! Score bonus!");

                                // TODO: Thêm scoring system ở đây nếu cần
                                AudioManager.Instance.PlaySFX("Clear1");
                            }

                            // Notify SpawnManager rằng shape đã được đặt
                            SpawnManager.Instance.OnShapePlaced(gameObject);

                            // Destroy shape parent (vì các block đã được re-parent vào cells)
                            Destroy(gameObject);
                        });
                    originalPosition = snapPos;
                    snapped = true;
                }

                // Clear highlights khi buông
                shapeCheckPos.ClearHighlights();
            }

            if (!snapped)
            {
                // Return về vị trí ban đầu với DOTween
                returnTween = transform.DOMove(originalPosition, returnDuration);
            }
        }
    }

    // Gán các block con vào từng cell tương ứng
    private void AssignBlocksToCells()
    {
        if (shapeCheckPos == null || !shapeCheckPos.CurrentHighlightedGrid.HasValue)
        {
            Debug.LogError("[ShapeClick] Cannot assign blocks - no highlighted grid!");
            return;
        }

        Vector2Int anchorGridPos = shapeCheckPos.CurrentHighlightedGrid.Value;
        CellManager cellManager = CellManager.Instance;

        // Lấy tất cả children blocks và local positions
        List<Transform> blockChildren = new List<Transform>();
        foreach (Transform child in transform)
        {
            blockChildren.Add(child);
        }

        // Lấy shape pattern để biết offset của mỗi block
        Vector2Int[] pattern = shapeCheckPos.GetShapePattern();
        float shapeCellSize = shapeCheckPos.GetShapeCellSize();

        if (pattern == null || pattern.Length == 0)
        {
            Debug.LogWarning("[ShapeClick] Pattern is null or empty!");
            return;
        }

        Debug.Log($"[ShapeClick] Assigning {blockChildren.Count} blocks to cells. Anchor: ({anchorGridPos.x}, {anchorGridPos.y})");

        // Gán từng block dựa trên local position của nó
        foreach (Transform block in blockChildren)
        {
            // Tính grid offset của block này từ local position
            Vector3 localPos = block.localPosition;
            Vector2Int blockOffset = new Vector2Int(
                Mathf.RoundToInt(localPos.x / shapeCellSize),
                Mathf.RoundToInt(localPos.y / shapeCellSize)
            );

            // Normalize offset (trừ đi min để về gốc 0,0)
            Vector2Int minOffset = pattern[0];
            foreach (var p in pattern)
            {
                if (p.x < minOffset.x || (p.x == minOffset.x && p.y < minOffset.y))
                    minOffset = p;
            }

            // Tìm xem blockOffset match với pattern nào
            Vector2Int normalizedBlockOffset = new Vector2Int(
                Mathf.RoundToInt(localPos.x / shapeCellSize) - Mathf.RoundToInt(shapeCheckPos.AnchorLocalOffset.x / shapeCellSize),
                Mathf.RoundToInt(localPos.y / shapeCellSize) - Mathf.RoundToInt(shapeCheckPos.AnchorLocalOffset.y / shapeCellSize)
            );

            Vector2Int gridPos = anchorGridPos + normalizedBlockOffset;
            Cell targetCell = cellManager.GetCellAt(gridPos);

            if (targetCell != null)
            {
                // Detach block khỏi parent trước
                block.SetParent(null);

                // Gán block vào cell
                targetCell.SetOccupyingBlock(block.gameObject);

                Debug.Log($"[ShapeClick] Assigned block '{block.name}' to cell at grid ({gridPos.x}, {gridPos.y})");
            }
            else
            {
                Debug.LogWarning($"[ShapeClick] No cell found at grid ({gridPos.x}, {gridPos.y}) for block '{block.name}'");
            }
        }
    }

    // Hàm utility: Lấy tất cả SpriteRenderer từ object cha và con, gán sortingOrder = 1
    public static void SetSortingOrderForAllSprites(GameObject parentObject, int sortingOrder = 1)
    {
        if (parentObject == null)
        {
            Debug.LogError("[ShapeClick] Parent object is null!");
            return;
        }

        // Lấy tất cả SpriteRenderer từ parent và các con của nó
        SpriteRenderer[] allSpriteRenderers = parentObject.GetComponentsInChildren<SpriteRenderer>();

        //Debug.LogError($"[ShapeClick] Found {allSpriteRenderers.Length} SpriteRenderers in '{parentObject.name}' and its children");

        foreach (SpriteRenderer spriteRenderer in allSpriteRenderers)
        {
            spriteRenderer.sortingOrder = sortingOrder;
            Debug.Log($"[ShapeClick] Set sortingOrder = {sortingOrder} for '{spriteRenderer.gameObject.name}'");
        }
    }
}
