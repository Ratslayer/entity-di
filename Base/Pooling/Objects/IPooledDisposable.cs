using System;

namespace BB
{
	public interface IPooledDisposable : IDisposable
	{
		ulong Counter { get; }
	}
}