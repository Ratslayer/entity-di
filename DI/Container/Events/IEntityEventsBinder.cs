using System.Reflection;
namespace BB.Di
{
    public interface ISubscription
    {
        void Subscribe();
        void Unsubscribe();
    }
    public abstract class InternalSubscription : ISubscription
    {
        public MethodInfo _method;
        public object _target;
        public IEntity _entity;
        public abstract void Init();
        public abstract void Subscribe();
        public abstract void Unsubscribe();
        public override string ToString()
            => DiEventsUtils.GetTypeMethodName(_target, _method);
    }
}
namespace BB
{
    public interface IEventHandler { }
    public interface IEventHandler<T> : IEventHandler
    {
        void OnEvent(T msg);
    }
}