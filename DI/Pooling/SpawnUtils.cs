using System;
namespace BB.Di
{
    public static class SpawnUtils
    {
        public static Entity Spawn(
            this IEntityInstaller installer,
            Entity? parent = null)
        {
            var p = parent ?? World.Entity;
            var entity = p._ref.CreateChild(installer);
            entity.State = EntityState.Enabled;
            return entity.GetToken();
        }
    }
}
