using System;
namespace BB.Di
{
	public interface IDiContainer : IDisposable
	{
		void BindStrategy(Type contract, IDiStrategy strategy);
	}
}