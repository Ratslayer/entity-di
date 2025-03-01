namespace BB
{
	public interface IFlushable
	{
		void ForceFlushChanges();
	}
	public interface IAutoFlushable : IFlushable
	{
		bool AutoFlushDisabled { get; set; }
	}
	public interface IDirtyFlushable : IFlushable
	{
		bool IsDirty { get; set; }
	}
}