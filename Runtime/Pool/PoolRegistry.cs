using System;
using System.Collections.Generic;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Pool
{
    public sealed class PoolRegistry : IPoolRegistry
    {
        readonly Dictionary<string, IPool> _objectPools = new();
        readonly Dictionary<PoolKey, GameObjectPool> _gameObjectPools = new();
        readonly Dictionary<int, GameObjectPool> _gameObjectInstanceToPool = new();
        readonly Dictionary<PoolKey, IComponentPool> _componentPools = new();
        readonly Dictionary<int, IComponentPool> _componentInstanceToPool = new();

        public UnityObjectPool<T> GetOrCreatePool<T>(
            string key = null,
            Func<T> createFunc = null,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue,
            bool collectionCheck = false)
        {
            var poolKey = BuildKey<T>(key);
            if (_objectPools.TryGetValue(poolKey, out var pool))
                return (UnityObjectPool<T>)pool;

            createFunc ??= GetDefaultFactory<T>();
            var newPool = new UnityObjectPool<T>(createFunc, onGet, onRelease, onDestroy, defaultCapacity, maxSize, collectionCheck);
            _objectPools[poolKey] = newPool;
            return newPool;
        }

        public T Get<T>(string key = null) => GetOrCreatePool<T>(key).Get();

        public void Release<T>(T instance, string key = null)
        {
            var poolKey = BuildKey<T>(key);
            UnityObjectPool<T> pool;

            if (_objectPools.TryGetValue(poolKey, out var existing))
                pool = (UnityObjectPool<T>)existing;
            else
            {
                pool = GetOrCreatePool<T>(
                    key,
                    () => throw new InvalidOperationException($"Pool \"{poolKey}\" was created by Release; provide a factory before calling Get."),
                    collectionCheck: true);
            }

            pool.Release(instance);
        }

        public GameObject GetGameObject(GameObject prefab, Transform parent = null, int defaultCapacity = 0, int maxSize = int.MaxValue, bool collectionCheck = false)
        {
            var pool = GetGameObjectPool(prefab, parent, defaultCapacity, maxSize, collectionCheck);
            return pool.Get(parent);
        }

        public void ReleaseGameObject(GameObject instance)
        {
            if (instance == null)
                return;

            if (_gameObjectInstanceToPool.TryGetValue(instance.GetInstanceID(), out var pool))
                pool.ReleaseInstance(instance);
            else
            {
                Debug.LogWarning("Trying to release a GameObject that was not created by PoolRegistry. Destroying it instead.");
                UnityEngine.Object.Destroy(instance);
            }
        }

        public T GetComponent<T>(T prefab, Transform parent = null, int defaultCapacity = 0, int maxSize = int.MaxValue, bool collectionCheck = false) where T : Component
        {
            var pool = GetComponentPool(prefab, parent, defaultCapacity, maxSize, collectionCheck);
            return pool.Get(parent);
        }

        public void ReleaseComponent(Component component)
        {
            if (component == null)
                return;

            if (_componentInstanceToPool.TryGetValue(component.GetInstanceID(), out var pool))
                pool.Release(component);
            else
            {
                Debug.LogWarning("Trying to release a Component that was not created by PoolRegistry. Destroying it instead.");
                UnityEngine.Object.Destroy(component.gameObject);
            }
        }

        public void ClearAll()
        {
            foreach (var pool in _objectPools.Values)
                pool.Clear();

            foreach (var pool in _gameObjectPools.Values)
                pool.Clear();

            foreach (var pool in _componentPools.Values)
                pool.Clear();

            _objectPools.Clear();
            _gameObjectPools.Clear();
            _componentPools.Clear();
            _gameObjectInstanceToPool.Clear();
            _componentInstanceToPool.Clear();
        }

        static string BuildKey<T>(string key) =>
            string.IsNullOrWhiteSpace(key) ? typeof(T).FullName : key;

        GameObjectPool GetGameObjectPool(GameObject prefab, Transform parent, int defaultCapacity, int maxSize, bool collectionCheck)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var key = new PoolKey(prefab.GetInstanceID(), parent == null ? 0 : parent.GetInstanceID());
            if (_gameObjectPools.TryGetValue(key, out var pool))
                return pool;

            var newPool = new GameObjectPool(prefab, parent, RegisterGameObjectInstance, ForgetGameObjectInstance, defaultCapacity, maxSize, collectionCheck);
            _gameObjectPools[key] = newPool;
            return newPool;
        }

        ComponentPool<T> GetComponentPool<T>(T prefab, Transform parent, int defaultCapacity, int maxSize, bool collectionCheck) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var key = new PoolKey(prefab.GetInstanceID(), parent == null ? 0 : parent.GetInstanceID());
            if (_componentPools.TryGetValue(key, out var pool))
                return (ComponentPool<T>)pool;

            var newPool = new ComponentPool<T>(prefab, parent, RegisterComponentInstance, ForgetComponentInstance, defaultCapacity, maxSize, collectionCheck);
            _componentPools[key] = newPool;
            return newPool;
        }

        void RegisterGameObjectInstance(GameObject instance, GameObjectPool pool)
        {
            if (instance == null || pool == null)
                return;
            _gameObjectInstanceToPool[instance.GetInstanceID()] = pool;
        }

        void ForgetGameObjectInstance(GameObject instance, GameObjectPool pool)
        {
            if (instance == null)
                return;
            _gameObjectInstanceToPool.Remove(instance.GetInstanceID());
        }

        void RegisterComponentInstance(Component component, IComponentPool pool)
        {
            if (component == null || pool == null)
                return;
            _componentInstanceToPool[component.GetInstanceID()] = pool;
        }

        void ForgetComponentInstance(Component component, IComponentPool pool)
        {
            if (component == null)
                return;
            _componentInstanceToPool.Remove(component.GetInstanceID());
        }

        static Func<T> GetDefaultFactory<T>()
        {
            var type = typeof(T);

            if (type.IsValueType)
                return () => default;

            if (type.GetConstructor(Type.EmptyTypes) != null)
                return Activator.CreateInstance<T>;

            throw new InvalidOperationException($"Type {type.Name} does not have a default constructor. Provide a factory when creating the pool.");
        }
    }
}
