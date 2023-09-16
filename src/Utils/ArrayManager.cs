using System.Collections.Generic;

namespace EntityDi;
public static class ArrayManager<T>
{
	static readonly List<T[]> _argsArrays = new();
	public static T[] GetArgArray(int numArgs)
	{
		if (numArgs >= _argsArrays.Count)
		{
			for (var i = _argsArrays.Count; i <= numArgs; i++)
				_argsArrays.Add(new T[i]);
		}
		return _argsArrays[numArgs];
	}
}
