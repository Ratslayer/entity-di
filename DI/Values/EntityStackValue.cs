namespace BB.Di
{
	public abstract class EntityStackValue<TSelf> : StackValue<TSelf, Entity>
		where TSelf : EntityStackValue<TSelf>
	{
		public bool Has<T>(out T value) => Value.Has(out value);
		public static implicit operator bool(EntityStackValue<TSelf> e)
			=> e.Value;
	}

	public abstract class BoolStackValue<TSelf> : StackValue<TSelf, bool>
		where TSelf : BoolStackValue<TSelf>
	{
		public static implicit operator bool(BoolStackValue<TSelf> e)
			=> e?.Value is true;
	}
}