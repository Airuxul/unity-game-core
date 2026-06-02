using System;
using System.Collections.Generic;
using Air.GameCore.Entity;

namespace Air.UnityGameCore.Runtime.Entity
{
    public sealed class UnityEntityLogicFactory : IEntityLogicFactory
    {
        readonly Dictionary<int, Func<IEntityLogic>> _creators = new();

        public void Register<T>(EntityTypeId typeId) where T : IEntityLogic, new() =>
            _creators[typeId.Value] = () => new T();

        public void Register(EntityTypeId typeId, Func<IEntityLogic> factory) =>
            _creators[typeId.Value] = factory ?? throw new ArgumentNullException(nameof(factory));

        public IEntityLogic Create(EntityTypeId typeId) =>
            _creators.TryGetValue(typeId.Value, out var factory)
                ? factory()
                : throw new InvalidOperationException($"No entity logic registered for type {typeId}.");
    }
}
