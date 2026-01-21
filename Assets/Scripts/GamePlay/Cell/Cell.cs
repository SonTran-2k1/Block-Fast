using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer blockRenderer;
    [SerializeField] private float fadeOutDuration = 0.3f;

    // GameObject block được gán vào cell này
    private GameObject occupyingBlock;

    public GameObject OccupyingBlock => occupyingBlock;

    public bool HasBlock()
    {
        return occupyingBlock != null;
    }

    public void ClearBlock()
    {
        // Fade out animation cho occupyingBlock
        if (occupyingBlock != null)
        {
            Debug.Log($"[Cell] Clearing block '{occupyingBlock.name}' from cell '{gameObject.name}' with fade out");
            
            GameObject blockToDestroy = occupyingBlock;
            occupyingBlock = null; // Set null TRƯỚC khi destroy
            
            // Dùng Coroutine để fade out an toàn, không có lỗi MissingReferenceException
            StartCoroutine(FadeOutAndDestroy(blockToDestroy));
        }
        
        // Clear blockRenderer sprite (nếu có - đây là SpriteRenderer có sẵn trong Cell)
        if (blockRenderer != null && blockRenderer.sprite != null)
        {
            Debug.Log($"[Cell] Clearing blockRenderer sprite on cell '{gameObject.name}'");
            blockRenderer.sprite = null;
        }
        
        // Destroy tất cả children còn sót (trừ blockRenderer nếu nó là con)
        List<Transform> childrenToDestroy = new List<Transform>();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            // Không destroy blockRenderer nếu nó là component của child
            if (blockRenderer != null && child.gameObject == blockRenderer.gameObject)
            {
                continue;
            }
            childrenToDestroy.Add(child);
        }
        
        foreach (Transform child in childrenToDestroy)
        {
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

    /// <summary>
    /// Coroutine để fade out block an toàn, không có lỗi MissingReferenceException
    /// </summary>
    private IEnumerator FadeOutAndDestroy(GameObject blockToDestroy)
    {
        if (blockToDestroy == null) yield break;
        
        // Lấy tất cả SpriteRenderers trong block
        SpriteRenderer[] spriteRenderers = blockToDestroy.GetComponentsInChildren<SpriteRenderer>();
        
        // Lưu lại original colors
        Color[] originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            // Kiểm tra block còn tồn tại không
            if (blockToDestroy == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);
            
            // Update alpha cho tất cả sprites
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                // Kiểm tra null trước khi access - tránh lỗi hoàn toàn
                if (spriteRenderers[i] != null)
                {
                    Color c = originalColors[i];
                    c.a = alpha;
                    spriteRenderers[i].color = c;
                }
            }
            
            yield return null;
        }
        
        // Destroy sau khi fade xong
        if (blockToDestroy != null)
        {
            Destroy(blockToDestroy);
        }
    }
}
