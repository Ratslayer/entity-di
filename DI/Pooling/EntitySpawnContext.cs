using BB.Di;
namespace BB
{
    public readonly struct EntitySpawnContext
    {
        public IEntityInstaller Installer { get; init; }
        public Entity? Parent { get; init; }
        public string SerializationName { get; init; }
        public Entity Spawn()
        {
            var p = Parent ?? World.Entity;
            var entity = p._ref.CreateChild(Installer);
            entity.State = EntityState.Enabled;
            var token = entity.GetToken();

            if (!string.IsNullOrWhiteSpace(SerializationName))
                EntitySerializationUtils.RegisterAsSerializedEntity(token, SerializationName);

            return token;
        }
    }
}
