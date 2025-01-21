using System.Reflection;
namespace BB.Di
{
	public static class DiInjectionUtils
	{
		public static void Inject(this IEntity entity, object instance)
		{
			var type = instance.GetType();
			foreach (var info in ReflectionUtils.GetAllMembersWithAttribute<InjectAttribute>(type))
				switch (info)
				{
					case PropertyInfo prop:
						prop.SetValue(instance, entity.Resolve(prop.PropertyType));
						break;
					case FieldInfo field:
						field.SetValue(instance, entity.Resolve(field.FieldType));
						break;
					case MethodInfo method:
						var parameters = method.GetParameters();
						var args = ArrayManager<object>.GetArgArray(parameters.Length);
						for (var i = 0; i < args.Length; i++)
						{
							args[i] = entity.Resolve(parameters[i].ParameterType);
						}
						method.Invoke(instance, args);
						break;
				}
			if (entity.Parent is null)
				return;
			foreach (var info in ReflectionUtils
				.GetAllMembersWithAttribute<InjectFromParentAttribute>(type))
				switch (info)
				{
					case PropertyInfo prop:
						if (entity.Parent.TryResolve(prop.PropertyType, out var value))
							prop.SetValue(instance, value);
						break;
					case FieldInfo field:
						if (entity.Parent.TryResolve(field.FieldType, out value))
							field.SetValue(instance, value);
						break;
				}
		}
	}
}