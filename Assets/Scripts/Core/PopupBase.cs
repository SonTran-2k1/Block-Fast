 using UnityEngine;
using Core.ObjectPool;
using DG.Tweening;

namespace Core.UI
{
    /// <summary>
    /// Base class for all popup UI elements.
    /// Handles show/hide animations and integrates with object pooling.
    /// </summary>
    public abstract class PopupBase : MonoBehaviour, IPoolable
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected float animationDuration = 0.3f;
        [SerializeField] protected bool useScaleAnimation = true;

        private Sequence currentSequence;
        protected bool isVisible = false;

        protected virtual void Start()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Show popup with fade and optional scale animation
        /// </summary>
        public virtual void Show()
        {
            isVisible = true;
            gameObject.SetActive(true);

            // Kill any existing animation sequence
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                currentSequence.Append(
                    DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, animationDuration)
                );
            }

            if (useScaleAnimation && rectTransform != null)
            {
                rectTransform.localScale = Vector3.zero;
                if (canvasGroup != null)
                {
                    currentSequence.Join(
                        rectTransform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack)
                    );
                }
                else
                {
                    currentSequence.Append(
                        rectTransform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack)
                    );
                }
            }

            OnPopupShown();
        }

        /// <summary>
        /// Hide popup with fade and optional scale animation
        /// </summary>
        public virtual void Hide()
        {
            isVisible = false;

            // Kill any existing animation sequence
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            if (useScaleAnimation && rectTransform != null)
            {
                currentSequence.Append(
                    rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack)
                );
            }

            if (canvasGroup != null)
            {
                if (useScaleAnimation && rectTransform != null)
                {
                    currentSequence.Join(
                        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, animationDuration)
                    );
                }
                else
                {
                    currentSequence.Append(
                        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, animationDuration)
                    );
                }
            }

            currentSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnPopupHidden();
            });
        }

        /// <summary>
        /// Called when popup is shown
        /// </summary>
        protected virtual void OnPopupShown() { }

        /// <summary>
        /// Called when popup is hidden
        /// </summary>
        protected virtual void OnPopupHidden() { }

        /// <summary>
        /// IPoolable implementation: Called when object is first created
        /// </summary>
        public virtual void OnPoolCreated()
        {
            // Reset initial state
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (rectTransform != null)
                rectTransform.localScale = Vector3.zero;
        }

        /// <summary>
        /// IPoolable implementation: Called when object is retrieved from pool
        /// </summary>
        public virtual void OnSpawned()
        {
            isVisible = false;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            if (rectTransform != null)
                rectTransform.localScale = Vector3.zero;
        }

        /// <summary>
        /// IPoolable implementation: Called when object is returned to pool
        /// </summary>
        public virtual void OnDespawned()
        {
            currentSequence?.Kill();
            isVisible = false;

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (rectTransform != null)
                rectTransform.localScale = Vector3.zero;
        }

        /// <summary>
        /// Check if popup is currently visible
        /// </summary>
        public bool IsVisible => isVisible;

        protected virtual void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
