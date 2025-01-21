using Cysharp.Threading.Tasks;

namespace BB
{
	public interface IFlushable
	{
		void FlushChanges();
	}
	public interface IAutoFlushable : IFlushable
	{
		bool AutoFlushDisabled { get; set; }
	}
}