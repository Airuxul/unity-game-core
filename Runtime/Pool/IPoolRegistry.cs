using System;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Pool
{
  public interface IPoolRegistry
  {
    UnityObjectPool<T> GetOrCreatePool<T>(
      string key = null,
      Func<T> createFunc = null,
      Action<T> onGet = null,
      Action<T> onRelease = null,
      Action<T> onDestroy = null,
      int defaultCapacity = 0,
      int maxSize = int.MaxValue,
      bool collectionCheck = false);

    T Get<T>(string key = null);
    void Release<T>(T instance, string key = null);
    GameObject GetGameObject(GameObject prefab, Transform parent = null, int defaultCapacity = 0, int maxSize = int.MaxValue, bool collectionCheck = false);
    void ReleaseGameObject(GameObject instance);
    T GetComponent<T>(T prefab, Transform parent = null, int defaultCapacity = 0, int maxSize = int.MaxValue, bool collectionCheck = false) where T : Component;
    void ReleaseComponent(Component component);
    void ClearAll();
  }
}
