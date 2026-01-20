using UnityEngine;

namespace Core.Singleton
{

    public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        protected static bool IsInitialized => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_lock)
                {
                    if (_instance != null)
                        return _instance;

                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                    else
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = GetComponent<T>();
                DontDestroyOnLoad(gameObject);
                OnSingletonInitialized();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                OnSingletonDestroyed();
            }
        }

        /// Called when the singleton is first initialized
        /// Override this instead of Awake for singleton-specific initialization

        protected virtual void OnSingletonInitialized() { }

        /// Called when the singleton is about to be destroyed

        protected virtual void OnSingletonDestroyed() { }
    }
}
