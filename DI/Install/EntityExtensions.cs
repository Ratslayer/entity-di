using BB.Di;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
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
		public static async UniTask WaitForEvent<T>(this Entity entity, CancellationToken ct)
		{
			if (!entity.Has(out IEvent<T> e))
				return;
			await e.WaitForEvent(ct);
		}
		public static void Despawn(this Entity entity)
		{
			if (entity)
				entity._ref.State = EntityState.Despawned;
		}
		public static void DespawnAndClear<T>(this ICollection<T> collection)
			where T:Component
		{
			foreach (var element in collection)
				element.Despawn();
			collection.Clear();
		}

		public static void Publish<T>(this Entity entity, T msg = default)
		{
			if (entity.Has(out IEvent<T> publisher))
				publisher.Publish(msg);
		}
		public static void Publish<T>(this IEntity entity, T msg = default)
		{
			if (entity.Has(out IEvent<T> publisher))
				publisher.Publish(msg);
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

		public static bool Has<T1, T2, T3, T4, T5,T6>(
			this Entity entity,
			out T1 var1, out T2 var2, out T3 var3, out T4 var4, out T5 var5, out T6 var6)
		{
			if (!entity)
			{
				var1 = default;
				var2 = default;
				var3 = default;
				var4 = default;
				var5 = default;
				var6 = default;
				return false;
			}
			var result = entity.Has(out var1);
			result &= entity.Has(out var2);
			result &= entity.Has(out var3);
			result &= entity.Has(out var4);
			result &= entity.Has(out var5);
			result &= entity.Has(out var6);
			return result;
		}

		public static void DespawnAndClear(this IList<Entity> entities)
		{
			foreach (var entity in entities)
				entity.Despawn();
			entities.Clear();
		}
	}
}