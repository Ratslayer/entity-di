using System;
using System.Collections.Generic;
using System.Reflection;
public static class ReflectionUtils
{
	static readonly Dictionary<Type, AttributeData> _datas = new();

	sealed class AttributeData
	{
		public readonly Dictionary<Type, List<MemberInfo>> _members = new();
	}
	public const BindingFlags AllNonStatic = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
	public static bool HasAttribute<T>(this MemberInfo member, out T attribute)
		where T : Attribute
	{
		attribute = member.GetCustomAttribute<T>();
		return attribute != null;
	}
	public static FieldInfo GetField(object target, string fieldName)
	{
		var type = target.GetType();
		return type.GetField(fieldName, AllNonStatic)
			?? throw new Exception($"{type} field '{fieldName}' not found.");
	}
	public static T GetFieldValue<T>(object target, string fieldName)
	{
		var field = GetField(target, fieldName);
		var value = (T)field.GetValue(target);
		return value;
	}
	public static bool TryGetInheritedType(Type type, Type genericTypeDefinition, out Type inheritedType)
	{
		inheritedType = null;
		var baseType = type;
		while (null != (baseType = baseType.BaseType))
		{
			if (baseType.IsGenericType)
			{
				var generic = baseType.GetGenericTypeDefinition();
				if (generic == genericTypeDefinition)
				{
					inheritedType = baseType;
					return true;
				}
			}
		}
		return false;
	}
	public static void ProcessAllMethods(Type objectType, Action<MethodInfo[]> process)
	{
		var type = objectType;
		while (type is not null && type != typeof(object))
		{
			var methods = type.GetMethods(AllNonStatic);
			process(methods);
			type = type.BaseType;
		}
	}
	public static void ProcessAllFieldsAndProperties(
		Type objectType,
		Action<FieldInfo> processField,
		Action<PropertyInfo> processProperty)
	{
		var type = objectType;
		while (type is not null && type != typeof(object))
		{
			foreach (var field in type.GetFields(AllNonStatic))
				processField(field);
			foreach (var prop in type.GetProperties(AllNonStatic))
				processProperty(prop);
			type = type.BaseType;
		}
	}
	public static List<MemberInfo> GetAllMembersWithAttribute<AttributeType>(Type type)
		where AttributeType : Attribute
		=> GetAllMembersWithAttribute(type, typeof(AttributeType));
	public static List<MemberInfo> GetAllMembersWithAttribute(Type objectType, Type attributeType)
	{
		if (!_datas.TryGetValue(objectType, out var data))
		{
			data = new AttributeData();
			_datas.Add(objectType, data);
		}
		if (!data._members.TryGetValue(attributeType, out var members))
		{
			members = new List<MemberInfo>();
			data._members.Add(attributeType, members);
			var type = objectType;
			while (type is not null)
			{
				var typeMembers = type.GetMembers(
					BindingFlags.Public 
					| BindingFlags.Instance 
					| BindingFlags.NonPublic);
				foreach (var member in typeMembers)
					if (member.GetCustomAttribute(attributeType) != null)
						members.Add(member);
				type = type.BaseType;
			}
		}
		return members;
	}
	public static void GetAllFieldsRecursive(
		Type type,
		List<FieldInfo> list,
		Type attributeType = null)
		=> GetAllMembersRecursive(type, list, (t, f) => t.GetFields(f), attributeType);
	public static void GetAllMembersRecursive(
		Type type,
		List<MemberInfo> list,
		Type attributeType = null)
		=> GetAllMembersRecursive(type, list, (t, f) => t.GetMembers(f), attributeType);
	public static void GetAllMembersRecursive<InfoType>(
		Type type,
		List<InfoType> list,
		Func<Type, BindingFlags, InfoType[]> memberGetter,
		Type attributeType = null)
		where InfoType : MemberInfo
	{
		while (type is not null)
		{
			var infos = memberGetter(
				type,
				BindingFlags.Public
				| BindingFlags.NonPublic
				| BindingFlags.Instance
				| BindingFlags.Static);
			foreach (var member in infos)
				if (attributeType is null
					|| member.GetCustomAttribute(attributeType) is not null)
					list.Add(member);
			type = type.BaseType;
		}
	}
	public static IEnumerable<PropertyInfo> GetAllPropertiesWithAttribute<TAttribute>(Type type)
		where TAttribute : Attribute
		=> GetInfos<TAttribute, PropertyInfo>(type);
	public static IEnumerable<MethodInfo> GetAllMethodsWithAttribute<TAttribute>(Type type)
		where TAttribute : Attribute
		=> GetInfos<TAttribute, MethodInfo>(type);
	public static IEnumerable<FieldInfo> GetAllFieldsWithAttribute<TAttribute>(Type type)
		where TAttribute : Attribute
		=> GetInfos<TAttribute, FieldInfo>(type);
	public static IEnumerable<TInfo> GetInfos<TAttribute, TInfo>(Type type)
		where TAttribute : Attribute
		where TInfo : MemberInfo
	{
		foreach (var info in GetAllMembersWithAttribute(type, typeof(TAttribute)))
			if (info is TInfo t)
				yield return t;
	}
	public static object InvokeStaticMethod(object target, Type type, string name, params object[] args)
	{
		var method = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		return method.Invoke(target, args);
	}
	public static object InvokeStaticMethod<T>(T target, string name, params object[] args)
		=> InvokeStaticMethod(target, typeof(T), name, args);
}