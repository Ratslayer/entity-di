using System.Threading;
namespace BB
{
    public interface IEvent { }
    public interface IEvent<T> : IEvent
    {
        CancellationToken NextEventCancellationToken { get; }
        void Publish(T message);
        void Subscribe(IEventHandler<T> subscription);
        void Unsubscribe(IEventHandler<T> subscription);
    }
}
