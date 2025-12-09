using System;
using System.Reflection;
namespace BB.Di
{
    //public sealed class EntityInstaller : IEntityInstaller
    //{
    //    public string Name { get; init; }
    //    readonly Action<IDiContainer> _install;
    //    public EntityInstaller(string name, Action<IDiContainer> install)
    //    {
    //        Name = name;
    //        _install = install;
    //    }
    //    public void Install(IDiContainer container)
    //        => _install(container);
    //}
    public interface ISubscription
    {
        void Subscribe();
        void Unsubscribe();
    }

    public interface IEntityEventsBinder
    {
        public event Action<UpdateTime>
            UpdateEvent,
            LateUpdateEvent,
            FixedUpdateEvent;

        event Action
            CreateEvent,
            SpawnEvent,
            DespawnEvent,
            PostSpawnEvent,
            AttachEvent,
            EnableEvent,
            DisableEvent;
        void RegisterSubscription(ISubscription subscription);
        void RegisterAttachedSubscription(IEntitySubscription subscription);
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
    //public sealed class PublisherSubscription<T> : ISubscription
    //{
    //    readonly IEvent<T> _publisher;
    //    readonly Action<T> _action;
    //    public PublisherSubscription(IEvent<T> publisher, Action<T> action)
    //    {
    //        _publisher = publisher;
    //        _action = action;
    //    }

    //    public void Subscribe()
    //    {
    //        _publisher.Subscribe(_action);
    //    }

    //    public void Unsubscribe()
    //    {
    //        _publisher.Unsubscribe(_action);
    //    }
    //}
    //public sealed class MethodInfoSubscription<T> : InternalSubscription
    //{
    //    Action<T> _action;
    //    IEvent<T> _lastPublisher;
    //    public override void Init()
    //    {
    //        _action = DiEventsUtils.CreateAction<T>(_method, _target, _entity);
    //    }

    //    public override void Subscribe()
    //    {
    //        if (!_entity.TryResolve(out _lastPublisher))
    //        {
    //            DiEventsUtils.LogError(
    //                _target, _method, $"{typeof(T).Name} event is not registered");
    //            return;
    //        }
    //        _lastPublisher.Subscribe(_action);
    //    }

    //    public override void Unsubscribe()
    //    {
    //        _lastPublisher?.Unsubscribe(_action);
    //    }
    //}
}
namespace BB
{
    public interface IEventHandler<T>
    {
        void OnEvent(T msg);
    }
}