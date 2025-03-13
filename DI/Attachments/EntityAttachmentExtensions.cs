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
		public static void SpawnAndAttachTo(
			this IEntityInstaller installer,
			Entity target,
			Entity parent = default)
		{
			if (installer is null || !target)
				return;
			var entity = installer.Spawn(parent);
			entity.AttachTo(target);
		}
		public static void Detach(this Entity entity)
		{
			if (entity)
				(entity._ref as EntityImpl).Detach();
		}
	}
}