using System.Collections.Generic;
namespace BB.Di
{
	public sealed class ListVal<T> : IReadOnlyValue<List<T>>, IPriority
	{
		public int Priority { get; set; }
		public List<T> Value { get; init; }
		public ListVal(IEnumerable<T> value, int priority = 0)
			: this(priority)
		{
			if (value is not null)
				Value.AddRange(value);
		}
		public ListVal(int priority = 0)
		{
			Value = new();
			Priority = priority;
		}
		public static implicit operator ListVal<T>(List<T> value) => new(value);
	}
}