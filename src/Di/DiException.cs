using System;

namespace EntityDi.Container;

public sealed class DiException : Exception
{
	public DiException(string msg) : base(msg)
	{

	}
}
