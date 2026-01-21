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
        if (occupyingBlock != null)
        {
            Debug.Log($"[Cell] Clearing block '{occupyingBlock.name}' from cell '{gameObject.name}'");
            GameObject blockToDestroy = occupyingBlock;
            occupyingBlock = null; // Set null TRƯỚC khi destroy
            Destroy(blockToDestroy);
        }
        else
        {
            Debug.Log($"[Cell] ClearBlock called on '{gameObject.name}' but no block to clear");
        }
        
        if (blockRenderer != null)
        {
            blockRenderer.sprite = null;
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
