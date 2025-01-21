namespace BB
{
	public sealed class Val<T> : IValue<T>, IPriority,IClearable
	{
		public int Priority { get; set; }
		public T Value { get; set; }
		public Val() { }
		public Val(T value)
		{
			Value = value;
		}
		public Val(T value, int priority)
		{
			Value = value;
			Priority = priority;
		}
		public static implicit operator Val<T>(T value) => new(value);

		public void Clear()
		{
			Value = default;
			Priority = 0;
		}
	}
}