using System;
using System.Collections.Generic;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Pool
{
    public interface IPool
    {
        int CountAll { get; }
        int CountInactive { get; }
        int CountActive { get; }
        void Clear();
    }

    public interface IPool<T> : IPool
    {
        T Get();
        void Release(T element);
        void Prewarm(int count);
    }

    public class UnityObjectPool<T> : IPool<T>
    {
        private readonly Stack<T> _stack;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly bool _collectionCheck;
        private readonly int _maxSize;

        private int _countAll;

        public int CountAll => _countAll;
        public int CountInactive => _stack.Count;
        public int CountActive => _countAll - _stack.Count;

        public UnityObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue,
            bool collectionCheck = false)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _collectionCheck = collectionCheck;
            _maxSize = maxSize <= 0 ? int.MaxValue : maxSize;
            _stack = new Stack<T>(Math.Max(defaultCapacity, 0));

            if (defaultCapacity > 0)
                Prewarm(defaultCapacity);
        }

        public virtual T Get()
        {
            var element = _stack.Count > 0 ? _stack.Pop() : CreateItem();
            _onGet?.Invoke(element);
            return element;
        }

        public virtual void Release(T element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element), "Cannot release a null instance back to the pool.");

            if (_collectionCheck && _stack.Contains(element))
                throw new InvalidOperationException("Trying to release an object that is already in the pool.");

            if (_stack.Count >= _maxSize)
            {
                OnItemDestroyed(element);
                return;
            }

            _onRelease?.Invoke(element);
            _stack.Push(element);
        }

        public void Prewarm(int count)
        {
            for (var i = _stack.Count; i < count; i++)
            {
                var element = CreateItem();
                _stack.Push(element);
            }
        }

        public virtual void Clear()
        {
            while (_stack.Count > 0)
            {
                var element = _stack.Pop();
                OnItemDestroyed(element);
            }

            _countAll = 0;
        }

        protected virtual T CreateItem()
        {
            var element = _createFunc();
            _countAll++;
            OnItemCreated(element);
            return element;
        }

        protected virtual void OnItemCreated(T element)
        {
        }

        protected virtual void OnItemDestroyed(T element)
        {
            _onDestroy?.Invoke(element);
            if (_countAll > 0)
                _countAll--;
        }
    }

    public interface IComponentPool : IPool
    {
        void Release(Component component);
    }

    internal readonly struct PoolKey : IEquatable<PoolKey>
    {
        public readonly int PrefabId;
        public readonly int ParentId;

        public PoolKey(int prefabId, int parentId)
        {
            PrefabId = prefabId;
            ParentId = parentId;
        }

        public bool Equals(PoolKey other)
        {
            return PrefabId == other.PrefabId && ParentId == other.ParentId;
        }

        public override bool Equals(object obj)
        {
            return obj is PoolKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PrefabId * 397) ^ ParentId;
            }
        }
    }

    public class GameObjectPool : UnityObjectPool<GameObject>
    {
        private readonly Action<GameObject, GameObjectPool> _onCreated;
        private readonly Action<GameObject, GameObjectPool> _onDestroyed;

        public GameObjectPool(
            GameObject prefab,
            Transform parent,
            Action<GameObject, GameObjectPool> onCreated,
            Action<GameObject, GameObjectPool> onDestroyed,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue,
            bool collectionCheck = false)
            : base(
                CreateFactory(prefab, parent),
                go =>
                {
                    if (go != null)
                        go.SetActive(true);
                },
                go =>
                {
                    if (go == null) return;
                    if (parent != null)
                        go.transform.SetParent(parent, false);
                    go.SetActive(false);
                },
                null,
                defaultCapacity,
                maxSize,
                collectionCheck)
        {
            _onCreated = onCreated;
            _onDestroyed = onDestroyed;
        }

        public GameObject Get(Transform parentOverride = null)
        {
            var instance = base.Get();
            if (instance != null && parentOverride != null)
                instance.transform.SetParent(parentOverride, false);
            return instance;
        }

        public void ReleaseInstance(GameObject instance)
        {
            base.Release(instance);
        }

        protected override void OnItemCreated(GameObject element)
        {
            _onCreated?.Invoke(element, this);
        }

        protected override void OnItemDestroyed(GameObject element)
        {
            _onDestroyed?.Invoke(element, this);
            if (element != null)
                UnityEngine.Object.Destroy(element);
        }

        private static Func<GameObject> CreateFactory(GameObject prefab, Transform parent)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            return () => UnityEngine.Object.Instantiate(prefab, parent);
        }
    }

    public class ComponentPool<T> : UnityObjectPool<T>, IComponentPool where T : Component
    {
        private readonly Action<Component, IComponentPool> _onCreated;
        private readonly Action<Component, IComponentPool> _onDestroyed;

        public ComponentPool(
            T prefab,
            Transform parent,
            Action<Component, IComponentPool> onCreated,
            Action<Component, IComponentPool> onDestroyed,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue,
            bool collectionCheck = false)
            : base(
                CreateFactory(prefab, parent),
                component =>
                {
                    if (component != null)
                        component.gameObject.SetActive(true);
                },
                component =>
                {
                    if (component == null) return;
                    if (parent != null)
                        component.transform.SetParent(parent, false);
                    component.gameObject.SetActive(false);
                },
                null,
                defaultCapacity,
                maxSize,
                collectionCheck)
        {
            _onCreated = onCreated;
            _onDestroyed = onDestroyed;
        }

        public T Get(Transform parentOverride = null)
        {
            var instance = base.Get();
            if (instance != null && parentOverride != null)
                instance.transform.SetParent(parentOverride, false);
            return instance;
        }

        void IComponentPool.Release(Component component)
        {
            if (component == null) return;
            if (component is not T typedComponent)
                throw new InvalidOperationException($"Component of type {component.GetType().Name} does not belong to this pool.");
            Release(typedComponent);
        }

        protected override void OnItemCreated(T element)
        {
            _onCreated?.Invoke(element, this);
        }

        protected override void OnItemDestroyed(T element)
        {
            _onDestroyed?.Invoke(element, this);
            if (element != null)
                UnityEngine.Object.Destroy(element.gameObject);
        }

        private static Func<T> CreateFactory(T prefab, Transform parent)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            return () =>
            {
                var instance = UnityEngine.Object.Instantiate(prefab.gameObject, parent);
                return instance.GetComponent<T>();
            };
        }
    }

    public static class PoolManager
    {
        private static readonly Dictionary<string, IPool> ObjectPools = new();
        private static readonly Dictionary<PoolKey, GameObjectPool> GameObjectPools = new();
        private static readonly Dictionary<int, GameObjectPool> GameObjectInstanceToPool = new();
        private static readonly Dictionary<PoolKey, IComponentPool> ComponentPools = new();
        private static readonly Dictionary<int, IComponentPool> ComponentInstanceToPool = new();

        public static UnityObjectPool<T> GetOrCreatePool<T>(
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
            if (ObjectPools.TryGetValue(poolKey, out var pool))
                return (UnityObjectPool<T>)pool;

            createFunc ??= GetDefaultFactory<T>();
            var newPool = new UnityObjectPool<T>(createFunc, onGet, onRelease, onDestroy, defaultCapacity, maxSize, collectionCheck);
            ObjectPools[poolKey] = newPool;
            return newPool;
        }

        public static T Get<T>(string key = null)
        {
            var pool = GetOrCreatePool<T>(key);
            return pool.Get();
        }

        public static void Release<T>(T instance, string key = null)
        {
            var poolKey = BuildKey<T>(key);
            UnityObjectPool<T> pool;

            if (ObjectPools.TryGetValue(poolKey, out var existing))
            {
                pool = (UnityObjectPool<T>)existing;
            }
            else
            {
                pool = GetOrCreatePool<T>(
                    key,
                    () => throw new InvalidOperationException($"Pool \"{poolKey}\" was created by Release; provide a factory before calling Get."),
                    collectionCheck: true);
            }

            pool.Release(instance);
        }

        public static GameObject GetGameObject(GameObject prefab, Transform parent = null, int defaultCapacity = 0, int maxSize = int.MaxValue, bool collectionCheck = false)
        {
            var pool = GetGameObjectPool(prefab, parent, defaultCapacity, maxSize, collectionCheck);
            return pool.Get(parent);
        }

        public static void ReleaseGameObject(GameObject instance)
        {
            if (instance == null) return;

            if (GameObjectInstanceToPool.TryGetValue(instance.GetInstanceID(), out var pool))
            {
                pool.ReleaseInstance(instance);
            }
            else
            {
                Debug.LogWarning("Trying to release a GameObject that was not created by PoolManager. Destroying it instead.");
                UnityEngine.Object.Destroy(instance);
            }
        }

        public static T GetComponent<T>(T prefab, Transform parent = null, int defaultCapacity = 0, int maxSize = int.MaxValue, bool collectionCheck = false) where T : Component
        {
            var pool = GetComponentPool(prefab, parent, defaultCapacity, maxSize, collectionCheck);
            return pool.Get(parent);
        }

        public static void ReleaseComponent(Component component)
        {
            if (component == null) return;

            if (ComponentInstanceToPool.TryGetValue(component.GetInstanceID(), out var pool))
            {
                pool.Release(component);
            }
            else
            {
                Debug.LogWarning("Trying to release a Component that was not created by PoolManager. Destroying it instead.");
                UnityEngine.Object.Destroy(component.gameObject);
            }
        }

        public static void ClearAll()
        {
            foreach (var pool in ObjectPools.Values)
                pool.Clear();

            foreach (var pool in GameObjectPools.Values)
                pool.Clear();

            foreach (var pool in ComponentPools.Values)
                pool.Clear();

            ObjectPools.Clear();
            GameObjectPools.Clear();
            ComponentPools.Clear();
            GameObjectInstanceToPool.Clear();
            ComponentInstanceToPool.Clear();
        }

        private static string BuildKey<T>(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? typeof(T).FullName : key;
        }

        private static GameObjectPool GetGameObjectPool(GameObject prefab, Transform parent, int defaultCapacity, int maxSize, bool collectionCheck)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            var key = new PoolKey(prefab.GetInstanceID(), parent == null ? 0 : parent.GetInstanceID());
            if (GameObjectPools.TryGetValue(key, out var pool))
                return pool;

            var newPool = new GameObjectPool(prefab, parent, RegisterGameObjectInstance, ForgetGameObjectInstance, defaultCapacity, maxSize, collectionCheck);
            GameObjectPools[key] = newPool;
            return newPool;
        }

        private static ComponentPool<T> GetComponentPool<T>(T prefab, Transform parent, int defaultCapacity, int maxSize, bool collectionCheck) where T : Component
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            var key = new PoolKey(prefab.GetInstanceID(), parent == null ? 0 : parent.GetInstanceID());
            if (ComponentPools.TryGetValue(key, out var pool))
                return (ComponentPool<T>)pool;

            var newPool = new ComponentPool<T>(prefab, parent, RegisterComponentInstance, ForgetComponentInstance, defaultCapacity, maxSize, collectionCheck);
            ComponentPools[key] = newPool;
            return newPool;
        }

        private static void RegisterGameObjectInstance(GameObject instance, GameObjectPool pool)
        {
            if (instance == null || pool == null) return;
            GameObjectInstanceToPool[instance.GetInstanceID()] = pool;
        }

        private static void ForgetGameObjectInstance(GameObject instance, GameObjectPool pool)
        {
            if (instance == null) return;
            GameObjectInstanceToPool.Remove(instance.GetInstanceID());
        }

        private static void RegisterComponentInstance(Component component, IComponentPool pool)
        {
            if (component == null || pool == null) return;
            ComponentInstanceToPool[component.GetInstanceID()] = pool;
        }

        private static void ForgetComponentInstance(Component component, IComponentPool pool)
        {
            if (component == null) return;
            ComponentInstanceToPool.Remove(component.GetInstanceID());
        }

        private static Func<T> GetDefaultFactory<T>()
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
