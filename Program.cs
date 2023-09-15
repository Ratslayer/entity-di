using EntityDi;
try
{
	var entity = EntityCreationUtils.CreateEntity("World", null, Bind);
	entity.Spawn();
	entity.Despawn();
}
catch (Exception ex)
{
	Console.WriteLine(ex.ToString());
}
void Bind(IEntity entity)
{
	entity.Bind<Test>();
	entity.Bind<ILogger, Logger>();
}
public sealed class Logger : ILogger
{
	public void Log(string message, LogType type)
	{
		Console.WriteLine(message);
	}
}
sealed record Test : EntitySystem
{
	[Subscribe]
	void OnSpawn(SpawnedEvent _)
	{
		Console.WriteLine("I spawned!");
	}
	[Subscribe]
	void OnDespawn(DespawnedEvent _)
	{
		Console.WriteLine("I despawned!");
	}
}