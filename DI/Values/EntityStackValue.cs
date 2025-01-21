namespace BB.Di
{
	public abstract record EntityStackValue<TSelf> : StackValue<TSelf, Entity>
		where TSelf : EntityStackValue<TSelf>
	{
		public bool Has<T>(out T value) => Value.Has(out value);
		public static implicit operator bool(EntityStackValue<TSelf> e)
			=> e.Value;
	}
}