using UnityEngine;
using System.Collections.Generic;
using Core.Singleton;
using Core.ObjectPool;


    public class PoolManager : SingletonBase<PoolManager>
    {
        private Dictionary<string, object> pools = new Dictionary<string, object>();

        /// <summary>
        /// Create and register a new pool
        /// </summary>
        public ObjectPooling<T> CreatePool<T>(string poolName, T prefab, int initialSize = 5, Transform parentTransform = null)
            where T : MonoBehaviour
        {
            if (pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists!");
                return pools[poolName] as ObjectPooling<T>;
            }

            var pool = new ObjectPooling<T>(prefab, initialSize, parentTransform, poolName);
            pools[poolName] = pool;
            Debug.Log($"[PoolManager] Created pool: {poolName} (Initial size: {initialSize})");
            return pool;
        }

        /// <summary>
        /// Get an existing pool by name
        /// </summary>
        public ObjectPooling<T> GetPool<T>(string poolName) where T : MonoBehaviour
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                return pool as ObjectPooling<T>;
            }

            Debug.LogError($"[PoolManager] Pool '{poolName}' not found!");
            return null;
        }

        /// <summary>
        /// Remove and clear a pool
        /// </summary>
        public void DestroyPool(string poolName)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                pools.Remove(poolName);
                Debug.Log($"[PoolManager] Destroyed pool: {poolName}");
            }
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            pools.Clear();
            Debug.Log("[PoolManager] Cleared all pools");
        }

        public int PoolCount => pools.Count;
    }
