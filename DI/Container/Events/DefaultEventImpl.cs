using System;
using System.Collections.Generic;
using System.Threading;
namespace BB
{
    public sealed class DefaultEventImpl<T> : IEvent<T>, IDisposable
    {
        event Action<T> Events;
        readonly List<Action<T>> _tempAddEvents = new();
        readonly OptimizedCancellationTokenSource _tokenSource = new();
        bool _isInvoking;
        public CancellationToken NextEventCancellationToken
            => _tokenSource.Token;
        public void Dispose()
        {
            Events = null;
            _tokenSource.Dispose();
        }
        public void Publish(T message)
        {
            try
            {
                _isInvoking = true;
                Events?.Invoke(message);
            }
            catch (Exception e)
            {
                Log.Logger.LogException(e);
            }
            finally
            {
                _isInvoking = false;
                foreach (var action in _tempAddEvents)
                    Events += action;
                _tempAddEvents.Clear();
            }
            //invoke cancellation token
            _tokenSource.Cancel();
        }
        public void Subscribe(Action<T> action)
        {
            if (_isInvoking)
                _tempAddEvents.Add(action);
            else Events += action;
        }

        public void Unsubscribe(Action<T> action)
        {
            Events -= action;
        }
    }
}
