using UnityEngine;
using BB.Di;
namespace BB
{
	public static class EntityExtensions
	{
		public static T Require<T>(this Entity entity)
		{
			if (entity._ref is null)
				throw new DiException($"Trying to get component from a null entity");
			return entity._ref.Require<T>();
		}
		public static T Get<T>(this Entity entity)
		{
			entity.Has(out T instance);
			return instance;
		}
		public static bool HasEntity(this Component comp, out Entity entity)
		{
			entity = default;
			if (!comp)
				return false;
			return HasEntity(comp.gameObject, out entity);
		}
		public static bool HasEntity(this GameObject obj, out Entity entity)
		{
			entity = default;
			if (!obj.HasEntityBehaviour(out var bh))
				return false;
			entity = bh.Entity;
			return true;
		}
		public static bool HasEntityRedirect(this GameObject obj, out IEntityBehaviour bh)
		{
			bh = obj.TryGetComponent(out EntityRedirectBehaviour rbh) && rbh._redirectTo
				? rbh._redirectTo : null;
			return bh is not null;
		}
		public static bool HasEntityBehaviour(this GameObject obj, out IEntityBehaviour bh)
		{
			bh = default;
			if (!obj)
				return false;
			bh = obj.GetComponentInParent<IEntityBehaviour>();
			return bh is not null;
		}
		public static Entity GetEntity(this Component comp)
			=> comp && comp.HasEntity(out var entity) ? entity : default;
		public static Entity GetEntity(this GameObject go)
			=> go && go.HasEntity(out var entity) ? entity : default;

		public static void Despawn(this Entity entity)
		{
			if (entity)
				entity._ref.State = EntityState.Despawned;
		}

		public static void RaiseEvent<T>(this Entity entity, T msg = default)
		{
			if (entity.Has(out IEvent<T> publisher))
				publisher.Raise(msg);
		}
		public static void RaiseEvent<T>(this IEntity entity, T msg = default)
		{
			if (entity.Has(out IEvent<T> publisher))
				publisher.Raise(msg);
		}
		public static void Warp(this Entity entity, Vector3 pos)
		{
			if (entity.Has(out Root root))
				root.Position = pos;
		}

		public static void Get<VarType>(this Entity entity, ref VarType var)
			where VarType : class
		{
			if (var is null)
				entity.Has(out var);
		}
		public static bool Has<VarType>(this Entity entity, out VarType var)
		{
			if (!entity)
			{
				var = default;
				return false;
			}
			return entity._ref.Has(out var);
		}
		public static bool Has<T1, T2>(this Entity entity, out T1 var1, out T2 var2)
		{
			if (!entity)
			{
				var1 = default;
				var2 = default;
				return false;
			}
			var result = entity.Has(out var1);
			result &= entity.Has(out var2);
			return result;
		}
		public static bool Has<T1, T2, T3>(this Entity entity, out T1 var1, out T2 var2, out T3 var3)
		{
			if (!entity)
			{
				var1 = default;
				var2 = default;
				var3 = default;
				return false;
			}
			var result = entity.Has(out var1);
			result &= entity.Has(out var2);
			result &= entity.Has(out var3);
			return result;
		}
		public static bool Has<T1, T2, T3, T4>(
			this Entity entity,
			out T1 var1, out T2 var2, out T3 var3, out T4 var4)
		{
			if (!entity)
			{
				var1 = default;
				var2 = default;
				var3 = default;
				var4 = default;
				return false;
			}
			var result = entity.Has(out var1);
			result &= entity.Has(out var2);
			result &= entity.Has(out var3);
			result &= entity.Has(out var4);
			return result;
		}
		public static bool Has<T1, T2, T3, T4, T5>(
			this Entity entity,
			out T1 var1, out T2 var2, out T3 var3, out T4 var4, out T5 var5)
		{
			if (!entity)
			{
				var1 = default;
				var2 = default;
				var3 = default;
				var4 = default;
				var5 = default;
				return false;
			}
			var result = entity.Has(out var1);
			result &= entity.Has(out var2);
			result &= entity.Has(out var3);
			result &= entity.Has(out var4);
			result &= entity.Has(out var5);
			return result;
		}
	}
	public static class EntityUsageExtensions
	{
		public static bool TryGetDirection(this Entity e1, Entity e2, out Vector3 dir)
		{
			if (e1.Has(out Root r1) && e2.Has(out Root r2))
			{
				dir = r2.Position - r1.Position;
				return true;
			}
			dir = default;
			return false;
		}
		public static bool TryGetDistance(this Entity e1, Entity e2, out float distance)
		{
			var result = TryGetDirection(e1, e2, out var dir);
			distance = dir.magnitude;
			return result;
		}
	}
}