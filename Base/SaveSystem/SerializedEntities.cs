using BB.Di;
using System.Collections.Generic;
using System.Linq;

namespace BB
{
    public readonly struct SerializedEntityName
    {
        public Entity Entity { get; init; }
        public string Name { get; init; }
    }
    public sealed record SerializedEntities : ISerializedEntities
    {
        //readonly Dictionary<Entity, string> _entityNames = new();
        //readonly Dictionary<string, Entity> _invertedEntityNames = new();
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

            //var e = entity._ref;
            //if (e is null)
            //    return;

            //if (!string.IsNullOrWhiteSpace(serializedName))
            //{
            //    var parent = e.Parent;
            //    while (parent != World.EntityRef && parent is not null)
            //    {
            //        !if (_entityNames.TryGetValue(parent.GetToken(), out var parentPath))
            //        {
            //            var path = $"{parentPath}/{serializedName}";
            //            _entityNames.Add(e, path);
            //            _invertedEntityNames.Add(path, e);
            //            return;
            //        }
            //        parent = parent.Parent;
            //    }
            //}
            //_entityNames.Add(e, serializedName);
            //_invertedEntityNames.Add(serializedName, e);
        }
        //public bool Has(string path, out Entity entity)
        //    => _invertedEntityNames.TryGetValue(path, out entity);
        //public IEnumerable<SerializableEntity> GetAll()
        //{
        //    foreach (var kvp in _entityNames)
        //        if (kvp.Key)
        //            yield return new()
        //            {
        //                Entity = kvp.Key._ref,
        //                SerializedName = kvp.Value
        //            };
        //}

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