using System;
namespace BB.Di
{
	public static class SpawnUtils
	{
		public static Entity SpawnChild(this Entity parent, IEntityInstaller installer)
		{
			if (!parent)
				parent = World.Entity;
			var entity = parent._ref.CreateChild(installer);
			entity.State = EntityState.Enabled;
			return entity.GetToken();
		}
	}
}
