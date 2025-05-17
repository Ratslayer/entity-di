namespace BB
{
	public static class EntityVariableExtensions
	{
		public static void Set<TVar, TValue>(this Entity e, TValue value)
			where TVar : Variable<TVar, TValue>
		{
			if (e.Has(out TVar v))
				v.Value = value;
		}
	}
}