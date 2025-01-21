using System;
namespace BB.Di
{
	public interface IDiStrategy
	{
		IocParams Params { get; set; }
		object Resolve();
		void AssertValidContract(Type type);
	}
	public interface IDiConstructorStrategy : IDiStrategy
	{
		void SetConstructorArgs(params (Type, object)[] args);
	}
	public enum IocParams
	{
		None = 0,
		Inject = 1 << 1,
		Lazy = 1 << 2,
		BindEvents = 1 << 3
	}
	public static class IIocStrategyExtensions
	{
		
	}
}