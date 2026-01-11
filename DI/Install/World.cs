using BB.Di;
namespace BB
{
    public static class World
    {
        public static void SetGame(BaseGameInstallerAsset installer)
        {
            if (WorldBootstrap.World.Game is not null)
            {
                Publish<BeforeGameDespawnEvent>();
                WorldBootstrap.World.ClearGame();
                Publish<AfterGameDespawnEvent>();
            }
            if (installer)
            {
                Publish<BeforeGameSpawnEvent>();
                WorldBootstrap.World.CreateGame(installer);
                Publish<AfterGameSpawnEvent>();
            }
        }
        public static T Require<T>()
        {
            if (!Has(out T result))
            {
                using var _ = Log.Logger.UseContext(Entity);
                throw new DiException($"World does not have {typeof(T).Name} bound.");
            }
            return result;
        }
        public static void Require<T1, T2>(out T1 t1, out T2 t2)
        {
            t1 = Require<T1>();
            t2 = Require<T2>();
        }
        public static void Require<T1, T2, T3>(out T1 t1, out T2 t2, out T3 t3)
        {
            t1 = Require<T1>();
            t2 = Require<T2>();
            t3 = Require<T3>();
        }
        public static void Require<T1, T2, T3, T4>(out T1 t1, out T2 t2, out T3 t3, out T4 t4)
        {
            t1 = Require<T1>();
            t2 = Require<T2>();
            t3 = Require<T3>();
            t4 = Require<T4>();
        }
        public static T Get<T>()
        {
            Has(out T result);
            return result;
        }
        public static Entity Entity => (WorldBootstrap.World.Game?.Entity
            ?? WorldBootstrap.World.Core.Entity)?.GetToken() ?? default;
        public static void Publish<T>(T msg = default) => Entity.Publish(msg);
        public static bool Has<T>(out T system) => Entity.Has(out system);
        public static bool Has<T1, T2>(out T1 t1, out T2 t2) => Entity.Has(out t1, out t2);
        public static bool Has<T1, T2, T3>(out T1 t1, out T2 t2, out T3 t3)
            => Entity.Has(out t1, out t2, out t3);
    }
}