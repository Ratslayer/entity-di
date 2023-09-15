namespace EntityDi.Container;

public static class DiInjectionUtils
{
	public static void Inject(DiContainer container, object instance)
	{
		var type = instance.GetType();
		foreach (var info in ReflectionUtils.GetAllPropertiesWithAttribute<InjectAttribute>(type))
			info.SetValue(instance, container.Resolve(info.PropertyType));
		foreach (var info in ReflectionUtils.GetAllMethodsWithAttribute<InjectAttribute>(type))
		{
			var parameters = info.GetParameters();
			var args = ReflectionUtils.GetArgArray(parameters.Length);
			for (var i = 0; i < args.Length; i++)
			{
				args[i] = container.Resolve(parameters[i].ParameterType);
			}
			info.Invoke(instance, args);
		}
	}
}
