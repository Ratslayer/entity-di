namespace BB.Di
{
	public static class EntityAttachmentExtensions
	{
		public static void AttachTo(this Entity entity, Entity target)
		{
			if (!entity)
				return;
			if (target)
				(entity._ref as EntityImpl).AttachTo(target._ref);
			else (entity._ref as EntityImpl).Detach();
		}
		public static Entity SpawnAndAttachTo(this IEntityInstaller installer, Entity target)
		{
			if (installer is null || !target)
				return default;
			var entity = installer.Spawn();
			entity.AttachTo(target);
			return entity;
		}
		public static void Detach(this Entity entity)
		{
			if (entity)
				(entity._ref as EntityImpl).Detach();
		}
	}
}