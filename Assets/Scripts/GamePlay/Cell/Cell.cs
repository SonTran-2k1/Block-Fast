using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer blockRenderer;

    public bool HasBlock()
    {
        return blockRenderer != null && blockRenderer.sprite != null;
    }

    public void ClearBlock()
    {
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
}
