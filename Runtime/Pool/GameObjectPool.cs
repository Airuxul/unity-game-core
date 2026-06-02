using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Pool
{
  public class GameObjectPool : UnityObjectPool<GameObject>
  {
    readonly Action<GameObject, GameObjectPool> _onCreated;
    readonly Action<GameObject, GameObjectPool> _onDestroyed;

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
          if (go == null)
            return;
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

    public void ReleaseInstance(GameObject instance) => base.Release(instance);

    protected override void OnItemCreated(GameObject element) =>
      _onCreated?.Invoke(element, this);

    protected override void OnItemDestroyed(GameObject element)
    {
      _onDestroyed?.Invoke(element, this);
      if (element != null)
        Object.Destroy(element);
    }

    static Func<GameObject> CreateFactory(GameObject prefab, Transform parent)
    {
      if (prefab == null)
        throw new ArgumentNullException(nameof(prefab));
      return () => Object.Instantiate(prefab, parent);
    }
  }
}
