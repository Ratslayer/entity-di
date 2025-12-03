using BB.Di;
namespace BB
{
    public readonly struct EntitySpawnContext
    {
        public IEntityInstaller Installer { get; init; }
        public Entity? Parent { get; init; }
        public string SerializationName { get; init; }
        //public Entity Spawn()
        //{
        //    var p = Parent ?? World.Entity;
        //    var pools = World.Require<IEntityPools>();
        //    var pool = pools.GetPool(Installer);

        //    var entity = pool.GetUnspawnedEntity(Parent);
        //    entity.SetState(EntityState.Enabled);
        //    var token = entity.GetToken();

        //    if (!string.IsNullOrWhiteSpace(SerializationName))
        //        EntitySerializationUtils.RegisterAsSerializedEntity(token, SerializationName);

        //    return token;
        //}
    }
}
