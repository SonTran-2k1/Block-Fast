using UnityEngine;
using System.Collections.Generic;
using Core.ObjectPool;

public class ObjectPooling<T> where T : MonoBehaviour
{
    private readonly Queue<T> availableObjects = new Queue<T>();
    private readonly HashSet<T> activeObjects = new HashSet<T>();
    private readonly T prefab;
    private readonly Transform parentTransform;
    private readonly int initialSize;
    private readonly string poolName;

    public int AvailableCount => availableObjects.Count;
    public int ActiveCount => activeObjects.Count;
    public int TotalCount => availableObjects.Count + activeObjects.Count;

    public ObjectPooling(T prefab, int initialSize = 5, Transform parentTransform = null, string poolName = null)
    {
        this.prefab = prefab;
        this.initialSize = Mathf.Max(0, initialSize);
        this.parentTransform = parentTransform;
        this.poolName = string.IsNullOrEmpty(poolName) ? typeof(T).Name + "Pool" : poolName;

        PrewarmPool();
    }

    /// <summary>
    /// Pre-create objects and store them in the pool
    /// </summary>
    private void PrewarmPool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject(true);
        }
    }

    /// <summary>
    /// Create a new object instance and add to available pool
    /// </summary>
    private T CreateNewObject(bool isPooling = false)
    {
        T obj = Object.Instantiate(prefab, parentTransform);
        obj.name = $"{prefab.name}_{AvailableCount + ActiveCount}";

        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnPoolCreated();
        }

        if (isPooling)
        {
            obj.gameObject.SetActive(false);
            availableObjects.Enqueue(obj);
        }

        return obj;
    }

    /// <summary>
    /// Get an object from the pool. Creates new if none available.
    /// </summary>
    public T Spawn(Vector3 position = default, Quaternion rotation = default)
    {
        T obj = availableObjects.Count > 0 ? availableObjects.Dequeue() : CreateNewObject();

        Transform objTransform = obj.transform;
        objTransform.position = position;
        objTransform.rotation = rotation != default ? rotation : Quaternion.identity;

        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);

        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnSpawned();
        }

        return obj;
    }

    /// <summary>
    /// Return an object to the pool
    /// </summary>
    public void Despawn(T obj)
    {
        if (obj == null)
            return;

        if (!activeObjects.Contains(obj))
        {
            Debug.LogWarning($"[{poolName}] Trying to despawn object that isn't in active pool: {obj?.name}");
            return;
        }

        activeObjects.Remove(obj);
        obj.gameObject.SetActive(false);

        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnDespawned();
        }

        availableObjects.Enqueue(obj);
    }

    /// <summary>
    /// Return all active objects to the pool
    /// </summary>
    public void DespawnAll()
    {
        var activeList = new List<T>(activeObjects);
        foreach (var obj in activeList)
        {
            Despawn(obj);
        }
    }

    /// <summary>
    /// Clear the entire pool and destroy all objects
    /// </summary>
    public void Clear()
    {
        DespawnAll();

        while (availableObjects.Count > 0)
        {
            T obj = availableObjects.Dequeue();
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }

        activeObjects.Clear();
    }

    /// <summary>
    /// Get current status of the pool
    /// </summary>
    public string GetStatus()
    {
        return $"[{poolName}] Available: {AvailableCount}, Active: {ActiveCount}, Total: {TotalCount}";
    }
}
