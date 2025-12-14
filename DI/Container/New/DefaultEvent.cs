using System;
using System.Collections.Generic;
using System.Threading;
namespace BB.Di
{
    public sealed class CascadingEvent<T> : BaseEvent<T>, IEventHandler<T>
    {
        public override CancellationToken NextEventCancellationToken
            => throw new DiException($"Awaiting {typeof(T).Name} events is not supported.");

        IEvent<T> _parentEvent;
        [Inject] EntityWrapper _entity;
        [OnEvent]
        void OnSpawn(EntitySpawnedEvent _)
        {
            _parentEvent = _entity.Entity.Parent?.Require<IEvent<T>>();
            _parentEvent?.Subscribe(this);
        }
        [OnEvent]
        void OnDespawn(EntityDespawnedEvent _)
        {
            _parentEvent?.Unsubscribe(this);
            _parentEvent = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            _parentEvent = null;
        }

        public void OnEvent(T action) => Publish(action);
    }
    public abstract class BaseEvent<T> : IEvent<T>, IDisposable
    {
        readonly List<IEventHandler<T>> _subscriptions = new(), _tempAddSubscriptions = new();
        bool _isInvoking;
        public abstract CancellationToken NextEventCancellationToken { get; }

        public virtual void Dispose()
        {
            _subscriptions.Clear();
            _tempAddSubscriptions.Clear();
        }

        public virtual void Publish(T message)
        {
            try
            {
                _isInvoking = true;
                for (var i = 0; i < _subscriptions.Count; i++)
                    _subscriptions[i].OnEvent(message);
            }
            catch (Exception e)
            {
                Log.Logger.LogException(e);
            }
            finally
            {
                _isInvoking = false;
                _subscriptions.AddRange(_tempAddSubscriptions);
                _tempAddSubscriptions.Clear();
            }
        }

        public void Subscribe(IEventHandler<T> subscription)
        {
            if (_isInvoking)
                _tempAddSubscriptions.Add(subscription);
            else _subscriptions.Add(subscription);
        }

        public void Unsubscribe(IEventHandler<T> subscription)
        {
            _subscriptions.Remove(subscription);
        }
    }
    public sealed class DefaultEvent<T> : BaseEvent<T>
    {
        readonly OptimizedCancellationTokenSource _tokenSource = new();
        public override CancellationToken NextEventCancellationToken => _tokenSource.Token;
        public override void Dispose()
        {
            base.Dispose();
            _tokenSource.Dispose();
        }

        public override void Publish(T message)
        {
            base.Publish(message);
            _tokenSource.Cancel();
        }
    }
}
