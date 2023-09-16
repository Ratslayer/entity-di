using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace EntityDi.Container;

public static class DiCreationUtils
{
	static readonly Dictionary<Type, CreationInfo> _infoCache = new();
	sealed record CreationInfo(ConstructorInfo Constructor, ParameterInfo[] ParamInfos, object[] Params);
	public static object Create(DiContainer container, Type instanceType, IEnumerable<(Type, object)> instanceArgs)
	{
		if(!_infoCache.TryGetValue(instanceType, out var info))
		{
			var activator = instanceType.GetConstructors().FirstOrDefault();
			if (activator is null)
			{
				container.Throw($"{instanceType.Name} does not have a constructor");
			}
			var parameters = activator.GetParameters();
			var args = new object[parameters.Length];
			info = new(activator, parameters, args);
			_infoCache[instanceType] = info;
		}
		
		for (var i = 0; i < info.Params.Length; i++)
		{
			var type = info.ParamInfos[i].ParameterType;
			if (instanceArgs.TryGet(out var arg, a => a.Item1 == type))
				info.Params[i] = arg.Item2;
			else
				info.Params[i] = container.Resolve(type);
		}
		var result = info.Constructor.Invoke(info.Params);
		return result;
	}
}