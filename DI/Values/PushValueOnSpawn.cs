using BB.Di;

namespace BB
{
	public abstract record PushValueOnSpawn<TStack, TValue>(
		TStack Stack,
		TValue Value)
		where TStack : StackValue<TStack, TValue>
	{
		[OnSpawn]
		void OnSpawn() => Stack.Push(Value);
		[OnDespawn]
		void OnDespawn() => Stack.Remove(Value);
	}
}