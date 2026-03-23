using BB.Di;
using System.Runtime.CompilerServices;

namespace BB
{
    public abstract class EntitySystem : IEntityProvider
    {
        [Inject] EntityWrapper _entityWrapper;
        public Entity Entity => _entityWrapper.Entity.GetToken();

        public override string ToString()
            => $"{GetType().Name} {Entity}";

        public ILoggerScope GetLogger([CallerMemberName] string caller = null)
        {
            var scope = _entityWrapper.Entity.World.Logger.GetScopeFromEntity(_entityWrapper.Entity);
            scope.AddToScope(LoggerConstants.ClassContextKey, GetType().Name);
            scope.WithMethod(caller);
            return scope;
        }

        public void LogError(string message, [CallerMemberName] string caller = null)
        {
            GetLogger(caller).Error(message);
        }

        public void LogInfo(string message, [CallerMemberName] string caller = null)
        {
            GetLogger(caller).Info(message);
        }
    }
}