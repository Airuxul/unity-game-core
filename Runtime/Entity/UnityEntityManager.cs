using System;
using System.Collections.Generic;
using Air.GameCore.Entity;
using Air.UnityGameCore.Runtime.Event;
using Air.UnityGameCore.Runtime.Pool;
using Air.UnityGameCore.Runtime.Resource;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Entity
{
    public sealed class UnityEntityManager : IEntityManager
    {
        readonly EntityManager _core;
        readonly IResManager _resources;
        readonly IPoolRegistry _pools;
        readonly IEntityAssetProvider _assets;
        readonly EventBus _events;
        readonly Dictionary<int, GameObject> _views = new();

        public UnityEntityManager(
            IEntityLogicFactory logicFactory,
            IResManager resources,
            IEntityAssetProvider assets,
            EventBus events,
            IPoolRegistry pools = null)
        {
            _core = new EntityManager(logicFactory ?? throw new ArgumentNullException(nameof(logicFactory)));
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _pools = pools;
        }

        public int EntityCount => _core.EntityCount;

        public EntityId ShowEntity(EntityTypeId typeId, EntityGroupName group, object userData = null)
        {
            var id = _core.ShowEntity(typeId, group, userData);
            _events.Emit(EntityEvents.Created, id);

            var path = _assets.GetAssetPath(typeId);
            if (string.IsNullOrEmpty(path))
                return id;

            _resources.LoadInstanceAsync<GameObject>(path, go =>
            {
                if (go == null || !_core.HasEntity(id))
                    return;

                BindView(id, go);
            });

            return id;
        }

        public bool HideEntity(EntityId id)
        {
            if (!_core.HasEntity(id))
                return false;

            if (_views.TryGetValue(id.Value, out var view) && view != null)
            {
                Object.Destroy(view);
                _views.Remove(id.Value);
            }

            if (!_core.HideEntity(id))
                return false;

            _events.Emit(EntityEvents.Destroyed, id);
            return true;
        }

        public void HideAllLoadedEntities(EntityGroupName group) => _core.HideAllLoadedEntities(group);

        public bool HasEntity(EntityId id) => _core.HasEntity(id);

        public EntityInfo GetEntity(EntityId id) => _core.GetEntity(id);

        public bool AttachEntity(EntityId childId, EntityId parentId) => _core.AttachEntity(childId, parentId);

        public bool DetachEntity(EntityId childId) => _core.DetachEntity(childId);

        public void Tick(float deltaTime) => _core.Tick(deltaTime);

        public IReadOnlyList<EntityId> GetEntitiesInGroup(EntityGroupName group) =>
            _core.GetEntitiesInGroup(group);

        public IReadOnlyList<EntityId> GetAllEntityIds() => _core.GetAllEntityIds();

        public void HideAllEntities()
        {
            var toHide = new List<EntityId>(_core.GetAllEntityIds());
            for (var i = toHide.Count - 1; i >= 0; i--)
                HideEntity(toHide[i]);
        }

        internal void BindView(EntityId id, GameObject instance)
        {
            var view = instance.GetComponent<UnityEntityInstance>();
            if (view == null)
                view = instance.AddComponent<UnityEntityInstance>();
            view.Initialize(this, id);
            _views[id.Value] = instance;
        }

        internal void NotifyViewDestroyed(EntityId id, UnityEntityInstance view)
        {
            if (!_views.TryGetValue(id.Value, out var existing) || existing != view.gameObject)
                return;

            _views.Remove(id.Value);
            if (_core.HasEntity(id))
                HideEntity(id);
        }
    }
}
