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

                            // Kiểm tra và clear hàng/cột full
                            int clearedLines = CellManager.Instance.CheckAndClearFullLines();

                            if (clearedLines > 0)
                            {
                                Debug.Log($"[ShapeClick] Cleared {clearedLines} lines! Score bonus!");

                                // TODO: Thêm scoring system ở đây nếu cần
                                AudioManager.Instance.PlaySFX("Clear1");
                            }

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
            return;

        Vector2Int anchorGridPos = shapeCheckPos.CurrentHighlightedGrid.Value;
        CellManager cellManager = CellManager.Instance;

        // Lấy tất cả children blocks
        List<Transform> blockChildren = new List<Transform>();
        foreach (Transform child in transform)
        {
            blockChildren.Add(child);
        }

        // Lấy shape pattern để biết offset của mỗi block
        Vector2Int[] pattern = shapeCheckPos.GetShapePattern();

        if (pattern == null || pattern.Length != blockChildren.Count)
        {
            Debug.LogWarning("[ShapeClick] Pattern và số block children không khớp!");
            return;
        }

        // Gán từng block vào cell tương ứng
        for (int i = 0; i < blockChildren.Count && i < pattern.Length; i++)
        {
            Vector2Int gridPos = anchorGridPos + pattern[i];
            Cell targetCell = cellManager.GetCellAt(gridPos);

            if (targetCell != null)
            {
                // Detach block khỏi parent trước
                Transform block = blockChildren[i];
                block.SetParent(null);

                // Gán block vào cell
                targetCell.SetOccupyingBlock(block.gameObject);

                Debug.Log($"[ShapeClick] Assigned block to cell at grid ({gridPos.x}, {gridPos.y})");
            }
            else
            {
                Debug.LogWarning($"[ShapeClick] No cell found at grid ({gridPos.x}, {gridPos.y})");
            }
        }
    }
}
