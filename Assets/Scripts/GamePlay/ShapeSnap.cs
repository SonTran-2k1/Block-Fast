using System;
using UnityEngine;

public class ShapeSnap : MonoBehaviour
{
    [SerializeField] ShapeCheckPos shapeCheckPos;

    private void Start()
    {
        if (shapeCheckPos == null)
        {
            shapeCheckPos = GetComponent<ShapeCheckPos>();
        }
    }

    // Lấy vị trí snap world nếu đang highlight hợp lệ
    public bool TryGetSnappedWorldPosition(out Vector3 snappedWorldPos)
    {
        snappedWorldPos = Vector3.zero;
        if (!shapeCheckPos.CurrentHighlightedGrid.HasValue)
            return false;

        Vector2Int gridPos = shapeCheckPos.CurrentHighlightedGrid.Value;
        if (!shapeCheckPos.CanPlaceShape(gridPos))
            return false;

        Vector3 anchorWorldPos = shapeCheckPos.GridToWorld(gridPos);
        snappedWorldPos = anchorWorldPos - shapeCheckPos.AnchorLocalOffset;
        return true;
    }
}
