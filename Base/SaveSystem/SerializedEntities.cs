using BB.Di;
using System.Collections.Generic;
using System.Linq;

namespace BB
{
    public sealed record SerializedEntities : ISerializedEntities
    {
        readonly List<SerializedEntityName> _entities = new();
        readonly Dictionary<Entity, string> _entityNames = new();
        readonly Dictionary<string, Entity> _entityPaths = new();
        public void Add(Entity entity, string serializedName)
        {
            if (!entity || string.IsNullOrWhiteSpace(serializedName))
                return;

            _entities.Add(new()
            {
                Entity = entity,
                Name = serializedName
            });
        }
        public Dictionary<string, Entity> BuildExistingEntityPaths()
        {
            _entityNames.Clear();
            _entityPaths.Clear();
            _entities.RemoveAll(e => !e.Entity);

            foreach (var es in _entities)
                _entityNames[es.Entity] = es.Name;

            var names = PooledList<string>.GetPooled();
            foreach (var es in _entities)
            {
                names.Clear();
                if (TryBuildPath(es.Entity))
                    _entityPaths.Add(string.Join('/', names), es.Entity);
                bool TryBuildPath(Entity e)
                {
                    if (!e)
                        return false;
                    if (!_entityNames.TryGetValue(e, out var name))
                        return false;
                    names.Prepend(name);
                    if (e._ref.Parent == WorldBootstrap.World.Core.Entity)
                        return true;

                    return TryBuildPath(e._ref.Parent.GetToken());
                }
            }
            names.Dispose();
            return _entityPaths;
        }
    }

}