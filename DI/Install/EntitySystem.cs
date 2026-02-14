using BB.Di;
using System.Runtime.CompilerServices;
namespace BB
{
    public abstract class EntitySystem : IEntityProvider
    {
        [Inject] readonly EntityWrapper _entityWrapper;
        public Entity Entity => _entityWrapper.Entity.GetToken();
        public override string ToString()
            => $"{GetType().Name} {Entity}";

        protected void LogError(string message, [CallerMemberName] string caller = null)
        {
            Log.Error($"{Entity}.{GetType().Name}.{caller}: {message}");
        }
        protected void LogInfo(string message, [CallerMemberName] string caller = null)
        {
            Log.Info($"{Entity}.{GetType().Name}.{caller}: {message}");
        }
    }
}