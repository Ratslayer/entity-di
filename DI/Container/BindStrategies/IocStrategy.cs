using System;
namespace BB.Di
{
	internal abstract class IocStrategy : IDiStrategy
	{
		public IocParams Params { get; set; }
		enum IocStrategyState
		{
			None,
			PreResolved,
			Resolved
		}
		IocStrategyState _state = IocStrategyState.None;
		public IocStrategy(IEntity entity) => _entity = entity;
		protected abstract Type ContractType { get; }
		protected abstract object ResolveInternal();
		public abstract void AssertValidContract(Type contractType);
		object _instance;
		protected readonly IEntity _entity;
		public object Resolve()
		{
			if (_instance is not null)
				return _instance;
			using var _ = Log.Logger.UseContext(this);
			if (_state is IocStrategyState.PreResolved)
				throw new Exception($"[{_entity.Name}:{ContractType.Name}] " +
					"Attempting to resolve a strategy twice. This usually indicates a circular dependency.");
			
			_state = IocStrategyState.PreResolved;
			_instance = ResolveInternal();
			_state = IocStrategyState.Resolved;

			if (Params.HasFlag(IocParams.Inject))
				DiInjectionUtils.Inject(_entity, _instance);
			if (Params.HasFlag(IocParams.BindEvents))
				DiEventsUtils.BindMembersWithAttributes((IEntityEventsBinder)_entity, _instance);
			return _instance;
		}
	}
}