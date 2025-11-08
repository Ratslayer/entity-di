using BB.Di;
using System.Collections.Generic;

namespace BB
{
    public sealed record SerializedEntities : ISerializedEntities
    {
        readonly Dictionary<IEntity, string> _entityNames = new();
        readonly Dictionary<string, IEntity> _invertedEntityNames = new();
        public void Add(Entity entity, string serializedName)
        {
            var e = entity._ref;
            if (e is null)
                return;

            if (!string.IsNullOrWhiteSpace(serializedName))
            {
                var parent = e.Parent;
                while (parent != World.EntityRef && parent is not null)
                {
                    if (_entityNames.TryGetValue(parent, out var parentPath))
                    {
                        var path = $"{parentPath}/{serializedName}";
                        _entityNames.Add(e, path);
                        _invertedEntityNames.Add(path, e);
                        return;
                    }
                    parent = parent.Parent;
                }
            }
            _entityNames.Add(e, serializedName);
            _invertedEntityNames.Add(serializedName, e);
        }
        public bool Has(string path, out Entity entity)
        {
            if(!_invertedEntityNames.TryGetValue(path, out var entityRef))
            {
                entity = default;
                return false;
            }

            entity = entityRef.GetToken();
            return true;
        }

        public IEnumerable<SerializableEntity> GetAll()
        {
            foreach (var kvp in _entityNames)
                if (kvp.Key.State is EntityState.Enabled or EntityState.Disabled)
                    yield return new()
                    {
                        Entity = kvp.Key,
                        SerializedName = kvp.Value
                    };
        }
    }

}