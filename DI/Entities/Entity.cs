using System;
using BB.Di;
namespace BB
{
	public readonly partial struct Entity : IDisposable
	{
		public readonly IEntity _ref;
		public readonly ulong _id;
		public Entity(IEntity entity, ulong id)
		{
			_ref = entity;
			_id = id;
		}
		public bool Enabled => this && _ref.State == EntityState.Enabled;
		public bool IsAlive()
			=> _ref is not null
			&& _ref.State is EntityState.Enabled or EntityState.Disabled
			&& _ref.CurrentSpawnId == _id;
		public static implicit operator bool(Entity entity) => entity.IsAlive();
		public static bool operator ==(Entity lhs, Entity rhs)
		{
			return lhs._ref == rhs._ref && lhs._id == rhs._id;
		}
		public static bool operator !=(Entity lhs, Entity rhs)
		{
			return lhs._ref != rhs._ref || lhs._id != rhs._id;
		}

		public override string ToString() => _ref?.Name ?? "None";
		public static implicit operator string(Entity e) => e.ToString();
		public override bool Equals(object obj) => obj is Entity e && e == this;
		public override int GetHashCode() => base.GetHashCode();

		public void Dispose() => this.Despawn();
	}
}