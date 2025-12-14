using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading;
namespace BB.Di
{
    public sealed class EventSubscription<TEvent>
        : ProtectedPooledObject<EventSubscription<TEvent>>,
        ISubscription,
        IEventHandler<TEvent>
    {
        IEvent<TEvent> _event;
        Action<TEvent> _action;
        object _instance;
        IEntity _entity;
        MethodInfo _method;
        public static EventSubscription<TEvent> GetPooled(
            IEvent<TEvent> @event,
            MethodInfo method,
            object instance,
            IEntity entity)
        {
            var result = GetPooledInternal();
            result._entity = entity;
            result._event = @event;
            result._instance = instance;
            result._method = method;
            result._action = CreateAction(method, instance, entity);
            return result;
        }
        public override string ToString()
            => $"{_entity.Name}:{_instance.GetType().Name}:{_method.Name}";

        public void Subscribe()
        {
            _event?.Subscribe(this);
        }

        public void Unsubscribe()
        {
            _event?.Unsubscribe(this);
        }
        public override void Dispose()
        {
            _event = null;
            _action = null;
            _entity = null;
            base.Dispose();
        }
        static Action<TEvent> CreateAction(MethodInfo method, object target, IEntity entity)
        {
            var args = method.GetParameters();
            if (method.ReturnType == typeof(void))
            {
                switch (args.Length)
                {
                    case 1:
                        return (Action<TEvent>)Delegate.CreateDelegate(typeof(Action<TEvent>), target, method);
                    default:
                        var action = (Action)Delegate.CreateDelegate(typeof(Action), target, method);
                        return _ => action();
                }
                ;
            }
            else if (method.ReturnType == typeof(UniTaskVoid))
            {
                switch (args.Length)
                {
                    case 2:
                        var a2 = (Func<TEvent, CancellationToken, UniTaskVoid>)Delegate
                            .CreateDelegate(typeof(Func<TEvent, CancellationToken, UniTaskVoid>), target, method);
                        return t => a2(t, entity.Require<IEvent<EntityDespawnedEvent>>().NextEventCancellationToken).Forget();
                    case 1:
                        if (args[0].ParameterType == typeof(CancellationToken))
                        {
                            var a1 = (Func<CancellationToken, UniTaskVoid>)Delegate
                                .CreateDelegate(typeof(Func<CancellationToken, UniTaskVoid>), target, method);
                            return _ => a1(entity.Require<IEvent<EntityDespawnedEvent>>().NextEventCancellationToken).Forget();
                        }
                        else
                        {
                            var a1 = (Func<TEvent, UniTaskVoid>)Delegate
                                .CreateDelegate(typeof(Func<TEvent, UniTaskVoid>), target, method);
                            return t => a1(t).Forget();
                        }
                    default:
                        var a0 = (Func<UniTaskVoid>)Delegate.CreateDelegate(typeof(Func<UniTaskVoid>), target, method);
                        return _ => a0().Forget();
                }
            }
            else
            {
                Log.Error(
                    $"Can't bind {target.ToString()}:{method.Name}. " +
                    $"Action methods can only have return type of void or UniTaskVoid");
                return null;
            }
        }

        public void OnEvent(TEvent msg) => _action?.Invoke(msg);
    }
}