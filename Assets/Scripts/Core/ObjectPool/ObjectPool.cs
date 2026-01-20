using UnityEngine;
using System.Collections.Generic;
using Core.ObjectPool;

public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly Queue<T> available = new Queue<T>();
        private readonly HashSet<T> active = new HashSet<T>();
        private readonly T prefab;
        private readonly Transform poolParent;
        private readonly string poolName;

        public int AvailableCount => available.Count;
        public int ActiveCount => active.Count;
        public int TotalCount => available.Count + active.Count;

        public ObjectPool(T prefab, int initialSize = 10, Transform poolParent = null, string poolName = null)
        {
            this.prefab = prefab;
            this.poolParent = poolParent;
            this.poolName = string.IsNullOrEmpty(poolName) ? typeof(T).Name + "Pool" : poolName;

            int prewarmCount = Mathf.Max(0, initialSize);
            for (int i = 0; i < prewarmCount; i++)
            {
                CreateNewObject();
            }
        }

        public T Spawn(Vector3 position, Quaternion rotation)
        {
            T obj = available.Count > 0 ? available.Dequeue() : CreateNewObject();
            Transform objTransform = obj.transform;
            objTransform.position = position;
            objTransform.rotation = rotation;

            obj.gameObject.SetActive(true);
            active.Add(obj);

            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawned();
            }

            return obj;
        }

        public T Spawn(Vector3 position) => Spawn(position, Quaternion.identity);

        public void Despawn(T obj)
        {
            if (obj == null)
                return;

            if (active.Remove(obj))
            {
                obj.gameObject.SetActive(false);

                if (obj.TryGetComponent<IPoolable>(out var poolable))
                {
                    poolable.OnDespawned();
                }

                available.Enqueue(obj);
            }
            else
            {
                Debug.LogWarning($"[{poolName}] Tried to despawn object that is not active: {obj.name}");
            }
        }

        private T CreateNewObject()
        {
            T obj = Object.Instantiate(prefab, poolParent);
            obj.name = $"{prefab.name}_{TotalCount}";
            obj.gameObject.SetActive(false);

            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnPoolCreated();
            }

            available.Enqueue(obj);
            return obj;
        }

        public string GetStatus()
        {
            return $"[{poolName}] Available: {AvailableCount}, Active: {ActiveCount}, Total: {TotalCount}";
        }
    }
