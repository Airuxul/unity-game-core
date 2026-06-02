using System;

namespace Air.UnityGameCore.Runtime.Pool
{
  internal readonly struct PoolKey : IEquatable<PoolKey>
  {
    public readonly int PrefabId;
    public readonly int ParentId;

    public PoolKey(int prefabId, int parentId)
    {
      PrefabId = prefabId;
      ParentId = parentId;
    }

    public bool Equals(PoolKey other) =>
      PrefabId == other.PrefabId && ParentId == other.ParentId;

    public override bool Equals(object obj) =>
      obj is PoolKey other && Equals(other);

    public override int GetHashCode()
    {
      unchecked
      {
        return (PrefabId * 397) ^ ParentId;
      }
    }
  }
}
