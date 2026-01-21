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

        Vector3 anchorWorldPos = shapeCheckPos.GetCellWorldPosition(gridPos);
        Vector3 anchorWorldOffset = shapeCheckPos.transform.TransformVector(shapeCheckPos.AnchorLocalOffset);
        snappedWorldPos = anchorWorldPos - anchorWorldOffset;
        return true;
    }
}
