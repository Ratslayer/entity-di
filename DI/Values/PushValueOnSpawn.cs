using BB.Di;

namespace BB
{
    public abstract class PushValueOnSpawn<TStack, TValue> : EntitySystem
        where TStack : StackValue<TStack, TValue>
    {
        [Inject] TStack _stack;
        public abstract TValue Value { get; }
        [OnEvent(typeof(EntitySpawnedEvent))]
        void OnSpawn() => _stack.Push(new()
        {
            Value = Value,
            Source = this
        });
        [OnEvent(typeof(EntityDespawnedEvent))]
        void OnDespawn() => _stack.Pop(new()
        {
            Value = Value,
            Source = this
        });
    }
}