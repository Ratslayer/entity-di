using System.Collections.Generic;

namespace BB
{
    public interface ISerializedEntities
    {
        void Add(Entity entity, string serializedPath);
        Dictionary<string, Entity> BuildExistingEntityPaths();
    }
}