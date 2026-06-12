using BB.Di;

namespace BB
{
    public interface ILoggerScopeFactory
    {
        ILoggerScope GetScope();
        ILoggerScope GetScopeFromEntity(IEntity entity);
    }
}