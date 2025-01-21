namespace BB.Di
{
	public sealed class EntityVal : IValue<Entity>, IPriority
	{
		public int Priority { get; private set; }
		public Entity Value { get; set; }
		public EntityVal() { }
		public EntityVal(Entity value, int priority = 0)
		{
			Value = value;
			Priority = priority;
		}
		public EntityVal(int priority)
		{
			Value = default;
			Priority = priority;
		}
		public static implicit operator EntityVal(Entity value) => new(value);
		public static implicit operator Entity(EntityVal v) => v is null ? default : v.Value;
	}
}