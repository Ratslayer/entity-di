using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace EntityDi.Container;

public static class DiCreationUtils
{
	static readonly Dictionary<Type, CreationInfo> _infoCache = new();
	sealed record CreationInfo(ConstructorInfo Constructor, ParameterInfo[] ParamInfos, object[] Params);
	static readonly List<bool> _usedParams = new();
	public static object Create(DiContainer container, Type instanceType, IList<(Type, object)> instanceArgs)
	{
		if (!_infoCache.TryGetValue(instanceType, out var info))
		{
			var activator = instanceType.GetConstructors().FirstOrDefault();
			if (activator is null)
			{
				throw container.Exception($"{instanceType.Name} does not have a constructor");
			}
			var parameters = activator.GetParameters();
			var args = new object[parameters.Length];
			info = new(activator, parameters, args);
			_infoCache[instanceType] = info;
		}
		var name = instanceType.Name;
		//init usage map
		_usedParams.Clear();
		for (var i = 0; i < instanceArgs.Count; i++)
			_usedParams.Add(false);
		for (var i = 0; i < info.Params.Length; i++)
		{
			var type = info.ParamInfos[i].ParameterType;
			var foundInArgs = false;
			for (var j = 0; j < _usedParams.Count; j++)
				if (!_usedParams[j] && type.IsAssignableFrom(instanceArgs[j].Item1))
				{
					info.Params[i] = instanceArgs[j].Item2;
					_usedParams[j] = true;
					foundInArgs = true;
					break;
				}
			//if (instanceArgs.TryGet(out var arg, a => type.IsAssignableFrom(a.Item1)))
			//	info.Params[i] = arg.Item2;
			if (foundInArgs)
				continue;
			else if (container.TryResolve(type, out var resolvedArg))
				info.Params[i] = resolvedArg;
			else throw container.Exception($"Couldn't create {instanceType.FullName}. {type.FullName} is not bound, nor among args.");
		}
		var result = info.Constructor.Invoke(info.Params);
		return result;
	}
}