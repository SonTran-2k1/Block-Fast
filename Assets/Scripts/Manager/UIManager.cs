using UnityEngine;
using System.Collections.Generic;
using Core.Singleton;
using Core.UI;
using Core.ObjectPool;

/// <summary>
    /// Centralized UI Manager for handling all popup/overlay management.
    /// Uses object pooling for efficiency and provides stack-based navigation.
    /// </summary>
    public class UIManager : SingletonBase<UIManager>
    {
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private PopupBase popupPrefab;
        [SerializeField] private int poolInitialSize = 5;

        private ObjectPool<PopupBase> popupPool;
        private Stack<PopupBase> popupStack = new Stack<PopupBase>();
        private Dictionary<System.Type, PopupBase> activePopups = new Dictionary<System.Type, PopupBase>();

        // Events for UI state changes
        public delegate void PopupEventHandler(System.Type popupType);
        public event PopupEventHandler OnPopupShown;
        public event PopupEventHandler OnPopupHidden;

        public int ActivePopupCount => popupStack.Count;

        protected override void OnSingletonInitialized()
        {
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("[UIManager] No Canvas found in scene!");
                    return;
                }
            }

            popupPool = new ObjectPool<PopupBase>(popupPrefab, poolInitialSize, mainCanvas.transform, "PopupPool");
            Debug.Log("[UIManager] Initialized with popup pool");
        }

        /// <summary>
        /// Show a popup of type T. Creates instance if needed, reuses from pool.
        /// </summary>
        public T ShowPopup<T>(Vector3 position = default) where T : PopupBase
        {
            System.Type popupType = typeof(T);

            // Check if popup already active
            if (activePopups.TryGetValue(popupType, out var existing))
            {
                Debug.LogWarning($"[UIManager] Popup {popupType.Name} is already active");
                return existing.GetComponent<T>();
            }

            var popup = popupPool.Spawn(position);

            // Ensure we remove any old component and add fresh one
            var oldComponent = popup.GetComponent<T>();
            if (oldComponent != null)
            {
                Object.Destroy(oldComponent);
            }

            var popupComponent = popup.gameObject.AddComponent<T>();
            popupStack.Push(popup);
            activePopups[popupType] = popup;

            popup.Show();
            OnPopupShown?.Invoke(popupType);

            return popupComponent;
        }

        /// <summary>
        /// Hide a specific popup type
        /// </summary>
        public void HidePopup<T>() where T : PopupBase
        {
            System.Type popupType = typeof(T);

            if (!activePopups.TryGetValue(popupType, out var popup))
            {
                Debug.LogWarning($"[UIManager] Popup {popupType.Name} is not active");
                return;
            }

            popup.Hide();
            activePopups.Remove(popupType);

            // Remove from stack if it's there
            if (popupStack.Count > 0 && popupStack.Peek() == popup)
            {
                popupStack.Pop();
            }

            popupPool.Despawn(popup);
            OnPopupHidden?.Invoke(popupType);
        }

        /// <summary>
        /// Hide all active popups in reverse order (LIFO)
        /// </summary>
        public void HideAllPopups()
        {
            while (popupStack.Count > 0)
            {
                var popup = popupStack.Pop();
                if (popup != null)
                {
                    popup.Hide();
                    activePopups.Remove(popup.GetType());
                    popupPool.Despawn(popup);
                }
            }
            activePopups.Clear();
        }

        /// <summary>
        /// Hide the top popup in the stack
        /// </summary>
        public void HideTopPopup()
        {
            if (popupStack.Count > 0)
            {
                var popup = popupStack.Pop();
                if (popup != null)
                {
                    popup.Hide();
                    activePopups.Remove(popup.GetType());
                    popupPool.Despawn(popup);
                }
            }
        }

        /// <summary>
        /// Check if a popup is currently active
        /// </summary>
        public bool IsPopupActive<T>() where T : PopupBase
        {
            return activePopups.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Get reference to an active popup
        /// </summary>
        public T GetActivePopup<T>() where T : PopupBase
        {
            if (activePopups.TryGetValue(typeof(T), out var popup))
            {
                return popup.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// Get pool status for debugging
        /// </summary>
        public string GetPoolStatus()
        {
            return popupPool?.GetStatus() ?? "Pool not initialized";
        }
    }
