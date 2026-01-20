using System;
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

    private void Start()
    {
        originalPosition = transform.position;
        shapeCheckPos = GetComponent<ShapeCheckPos>();
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
            
            // Clear highlights khi buông
            if (shapeCheckPos != null)
            {
                shapeCheckPos.ClearHighlights();
            }

            // Return về vị trí ban đầu với DOTween
            returnTween = transform.DOMove(originalPosition, returnDuration);
        }
    }
}
