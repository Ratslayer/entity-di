using System.Reflection;

namespace EntityDi.Container;
public static class ReflectionUtils
{
	
	static readonly Dictionary<Type, AttributeData> _datas = new();
	
	public sealed class AttributeData
	{
		public readonly Dictionary<Type, List<MemberInfo>> _members = new();
	}
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
				var typeMembers = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
				foreach (var member in typeMembers)
					if (member.GetCustomAttribute(attributeType) != null)
						members.Add(member);
				type = type.BaseType;
			}
		}
		return members;
	}
	public static IEnumerable<PropertyInfo> GetAllPropertiesWithAttribute<TAttribute>(Type type)
		where TAttribute : Attribute
		=> GetInfos<TAttribute, PropertyInfo>(type);
	public static IEnumerable<MethodInfo> GetAllMethodsWithAttribute<TAttribute>(Type type)
		where TAttribute : Attribute
		=> GetInfos<TAttribute, MethodInfo>(type);
	public static IEnumerable<TInfo> GetInfos<TAttribute, TInfo>(Type type)
		where TAttribute : Attribute
		where TInfo : MemberInfo
	{
		foreach (var info in GetAllMembersWithAttribute(type, typeof(TAttribute)))
			if (info is TInfo t)
				yield return t;
	}
}