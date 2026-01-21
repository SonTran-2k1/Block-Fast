using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer blockRenderer;

    // GameObject block được gán vào cell này
    private GameObject occupyingBlock;

    public GameObject OccupyingBlock => occupyingBlock;

    public bool HasBlock()
    {
        return occupyingBlock != null;
    }

    public void ClearBlock()
    {
        // Destroy occupyingBlock (block được gán từ shape)
        if (occupyingBlock != null)
        {
            Debug.Log($"[Cell] Clearing occupyingBlock '{occupyingBlock.name}' from cell '{gameObject.name}'");
            GameObject blockToDestroy = occupyingBlock;
            occupyingBlock = null; // Set null TRƯỚC khi destroy
            blockToDestroy.SetActive(false);
            Destroy(blockToDestroy);
        }
        
        // Clear blockRenderer sprite (nếu có - đây là SpriteRenderer có sẵn trong Cell)
        if (blockRenderer != null && blockRenderer.sprite != null)
        {
            Debug.Log($"[Cell] Clearing blockRenderer sprite on cell '{gameObject.name}'");
            blockRenderer.sprite = null;
        }
        
        // Destroy tất cả children còn sót (trừ blockRenderer nếu nó là con)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            // Không destroy blockRenderer nếu nó là component của child
            if (blockRenderer != null && child.gameObject == blockRenderer.gameObject)
            {
                continue;
            }
            Debug.Log($"[Cell] Destroying leftover child '{child.name}' from cell '{gameObject.name}'");
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    public void SetBlock(Sprite sprite)
    {
        if (blockRenderer != null)
        {
            blockRenderer.sprite = sprite;
        }
    }

    // Gán block GameObject vào cell và re-parent nó
    public void SetOccupyingBlock(GameObject block)
    {
        occupyingBlock = block;

        if (block != null)
        {
            // Lưu lại world scale trước khi re-parent
            Vector3 originalWorldScale = block.transform.lossyScale;

            // Re-parent block vào cell, giữ world position
            block.transform.SetParent(transform, true);

            // Đặt local position về zero (center của cell)
            block.transform.localPosition = Vector3.zero;

            // Khôi phục lại world scale bằng cách tính local scale mới
            Vector3 parentScale = transform.lossyScale;
            block.transform.localScale = new Vector3(
                originalWorldScale.x / parentScale.x,
                originalWorldScale.y / parentScale.y,
                originalWorldScale.z / parentScale.z
            );
        }
    }
}
