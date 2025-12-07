//using System;
//namespace BB.Di
//{
//	internal sealed class ConstructedIocStrategy : IocStrategy, IDiConstructorStrategy
//	{
//		(Type, object)[] _args;
//		public Type InstanceType { get; init; }
//		public ConstructedIocStrategy(IEntity entity, Type instanceType)
//			: base(entity)
//			=> InstanceType = instanceType;
//		public override void AssertValidContract(Type contract)
//		{
//			if (!contract.IsAssignableFrom(InstanceType))
//				throw new DiException(
//					$"[{_entity.Name}] {contract.Name} " +
//					$"is not assignable from {InstanceType.Name}");
//		}
//		protected override Type ContractType => InstanceType;

//		protected override object ResolveInternal()
//			=> DiCreationUtils.Create(_entity, InstanceType, _args);
//		public void SetConstructorArgs((Type, object)[] args)
//			=> _args = args;
//		public override string ToString()
//			=> $"Bind:{InstanceType.Name}";
//	}
//}