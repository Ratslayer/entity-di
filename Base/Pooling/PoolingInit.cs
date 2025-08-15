using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace BB
{
	public static class PoolingInit
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void ClearPools()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var pooledType = typeof(IPooledDisposable);
			foreach (var assembly in assemblies)
				foreach (var type in assembly.GetTypes())
				{
					if (type.ContainsGenericParameters)
						continue;
					if (!pooledType.IsAssignableFrom(type))
						continue;
					var fields = new List<FieldInfo>();
					ReflectionUtils.GetAllFieldsRecursive(type, fields);
					foreach (var field in fields)
					{
						if (!field.IsStatic)
							continue;
						if (field.Name != "_pool")
							continue;
						var pool = (IList)field.GetValue(null);
						pool.Clear();
					}
				}
		}
	}
}