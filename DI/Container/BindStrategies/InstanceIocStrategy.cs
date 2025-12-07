//using NUnit.Framework;
//using System;
//namespace BB.Di
//{
//	internal sealed class InstanceIocStrategy : IocStrategy
//	{
//		readonly object _instance;
//		public InstanceIocStrategy(IEntity entity, object instance)
//			: base(entity)
//			=> _instance = instance;
//		public override void AssertValidContract(Type contract)
//		{
//			if (_instance is null)
//				throw new Exception($"Attempted to assign null instance to {contract.Name}");
//			var instanceType = _instance.GetType();
//			if (!contract.IsAssignableFrom(instanceType))
//				throw new Exception(
//					$"Can't assign instance of type {instanceType.Name} " +
//					$"to {contract.Name} contract");
//		}

//		protected override object ResolveInternal() => _instance;
//		protected override Type ContractType => _instance.GetType();
//	}
//}