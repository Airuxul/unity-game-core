using Air.GameCore.Entity;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Entity
{
    public sealed class UnityEntityInstance : MonoBehaviour
    {
        UnityEntityManager _manager;
        EntityId _entityId;

        public EntityId EntityId => _entityId;

        public void Initialize(UnityEntityManager manager, EntityId entityId)
        {
            _manager = manager;
            _entityId = entityId;
        }

        void OnDestroy() => _manager?.NotifyViewDestroyed(_entityId, this);
    }
}
