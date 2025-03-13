using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace BB.Di
{
	public static class DiCreationUtils
	{
		static readonly Dictionary<Type, CreationInfo> _infoCache = new();
		sealed record CreationInfo(ConstructorInfo Constructor, ParameterInfo[] ParamInfos, object[] Params);
		public static object Create(
			this IEntity container,
			Type instanceType,
			(Type, object)[] instanceArgs)
		{
			if (!_infoCache.TryGetValue(instanceType, out var info))
			{
				var activator = instanceType.GetConstructors().FirstOrDefault() 
					?? throw new Exception($"{instanceType.Name} does not have a constructor");
				var parameters = activator.GetParameters();
				var args = new object[parameters.Length];
				info = new(activator, parameters, args);
				_infoCache[instanceType] = info;
			}
			var name = instanceType.Name;
			//resolve constructor params
			if (instanceArgs is not null && instanceArgs.Length > 0)
				unsafe
				{
					//select args first from instanceArgs and then from 
					Span<bool> usedParams = stackalloc bool[instanceArgs.Length];
					for (var i = 0; i < info.Params.Length; i++)
					{
						var type = info.ParamInfos[i].ParameterType;
						var foundInArgs = false;
						for (var j = 0; j < usedParams.Length; j++)
							if (!usedParams[j] && type.IsAssignableFrom(instanceArgs[j].Item1))
							{
								info.Params[i] = instanceArgs[j].Item2;
								usedParams[j] = true;
								foundInArgs = true;
								break;
							}
						if (foundInArgs)
							continue;
						else ResolveParamFromContainer(type, i);
					}
				}
			else for (var i = 0; i < info.Params.Length; i++)
				{
					var type = info.ParamInfos[i].ParameterType;
					ResolveParamFromContainer(type, i);
				}
			var result = info.Constructor.Invoke(info.Params);
			return result;
			void ResolveParamFromContainer(Type type, int paramId)
			{
				if (container.TryResolve(type, out var resolvedArg))
					info.Params[paramId] = resolvedArg;
				else throw new Exception(
					$"Couldn't create {instanceType.FullName}. " +
					$"{type.FullName} is not bound, nor among args.");
			}
		}
	}
}