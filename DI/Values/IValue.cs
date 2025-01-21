namespace BB
{
	public interface IValue<T> : IReadOnlyValue<T>
	{
		new T Value { get; set; }
	}
}