using UnityEngine;

namespace Core.ObjectPool
{
    /// <summary>
    /// Contract for pooled objects to receive lifecycle callbacks.
    /// </summary>
    public interface IPoolable
    {
        void OnPoolCreated(); // Called once when instance is first created
        void OnSpawned();     // Called every time the instance is taken from the pool
        void OnDespawned();   // Called every time the instance is returned to the pool
    }
}
