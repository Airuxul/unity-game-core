using Air.GameCore.Entity;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Entity
{
    public interface IEntityAssetProvider
    {
        string GetAssetPath(EntityTypeId typeId);
    }

    public sealed class DictionaryEntityAssetProvider : IEntityAssetProvider
    {
        readonly System.Collections.Generic.Dictionary<int, string> _paths = new();

        public void Register(EntityTypeId typeId, string path) => _paths[typeId.Value] = path;

        public string GetAssetPath(EntityTypeId typeId) =>
            _paths.TryGetValue(typeId.Value, out var path) ? path : null;
    }
}
