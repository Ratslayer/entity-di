using BB.Di;

namespace BB
{
    public abstract record PushValueOnSpawn<TStack, TValue>(
        TStack Stack,
        TValue Value) : EntitySystem
        where TStack : StackValue<TStack, TValue>
    {
        [OnSpawn]
        void OnSpawn() => Stack.Push(new()
        {
            Value = Value,
            Source = this
        });
        [OnDespawn]
        void OnDespawn() => Stack.Pop(new()
        {
            Value = Value,
            Source = this
        });
    }
}