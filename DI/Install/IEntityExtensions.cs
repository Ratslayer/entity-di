using UnityEngine;
namespace BB.Di
{
	public static class IEntityExtensions
	{
		public static Entity GetToken(this IEntity entity)
			=> entity is null ? default : new(entity, entity.CurrentSpawnId);
		public static T Require<T>(this IEntity entity)
		{
			if (!entity.Has(out T instance))
				throw new DiException($"{entity.Name}: Could not resolve {typeof(T).Name}");
			return instance;
		}
		public static IEntity GetEntityRef(this Component comp)
			=> comp.gameObject.GetComponent<EntityGameObject>().Entity._ref;
		public static IEntity GetEntityRef(this GameObject go)
			=> go.GetComponent<EntityGameObject>().Entity._ref;
		public static bool Has<T>(this IEntity entity, out T instance)
		{
			if (entity is not null
				&& entity.TryResolve(typeof(T), out var obj))
			{
				instance = (T)obj;
				return true;
			}
			instance = default;
			return false;
		}
	}
}
