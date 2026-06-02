using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Pool
{
  public class ComponentPool<T> : UnityObjectPool<T>, IComponentPool where T : Component
  {
    readonly Action<Component, IComponentPool> _onCreated;
    readonly Action<Component, IComponentPool> _onDestroyed;

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
          if (component == null)
            return;
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
      if (component == null)
        return;
      if (component is not T typedComponent)
        throw new InvalidOperationException($"Component of type {component.GetType().Name} does not belong to this pool.");
      Release(typedComponent);
    }

    protected override void OnItemCreated(T element) =>
      _onCreated?.Invoke(element, this);

    protected override void OnItemDestroyed(T element)
    {
      _onDestroyed?.Invoke(element, this);
      if (element != null)
        Object.Destroy(element.gameObject);
    }

    static Func<T> CreateFactory(T prefab, Transform parent)
    {
      if (prefab == null)
        throw new ArgumentNullException(nameof(prefab));
      return () =>
      {
        var instance = Object.Instantiate(prefab.gameObject, parent);
        return instance.GetComponent<T>();
      };
    }
  }
}
