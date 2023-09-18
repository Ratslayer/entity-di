using System;

namespace EntityDi;

public sealed class DiException : Exception
{
	public DiException(string msg) : base(msg)
	{

	}
}
