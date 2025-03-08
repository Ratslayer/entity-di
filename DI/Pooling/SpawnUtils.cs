using System;
namespace BB.Di
{
	public static class SpawnUtils
	{
		public static Entity Spawn(
			this IEntityInstaller installer,
			Entity parent = default)
		{
			if (!parent)
				parent = World.Entity;
			var entity = parent._ref.CreateChild(installer);
			entity.State = EntityState.Enabled;
			return entity.GetToken();
		}
	}
}
