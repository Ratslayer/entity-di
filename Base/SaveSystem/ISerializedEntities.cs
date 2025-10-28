using System.Collections.Generic;

namespace BB
{
    public interface ISerializedEntities
    {
        void Add(Entity entity, string serializedPath);
        bool Has(string serializedPath, out Entity entity);
        IEnumerable<SerializableEntity> GetAll();
    }

}