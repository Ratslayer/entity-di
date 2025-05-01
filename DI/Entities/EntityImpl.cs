using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
namespace BB.Di
{
	public readonly partial struct UpdateTime
	{
		public readonly float _delta;

		public static implicit operator float(UpdateTime time)
			=> time._delta;
	}
	public sealed partial class EntityImpl :
		IEntity,
		IDisposable,
		IDiContainer,
		IEntityEventsBinder,
		IEntityLifecycle,
		IEntityDetails
	{
		static ulong _lastId = 0;
		public IEntity Parent => _parent;
		readonly bool _isRoot;
		readonly EntityImpl _parent;
		readonly IEntityPool _pool;
		readonly OptimizedCancellationTokenSource _despawnCtSource;
		public ulong CurrentSpawnId { get; private set; }
		public CancellationToken DespawnCancellationToken
			=> _despawnCtSource.Token;
		public string Name { get; init; }
		EntityImpl(
			string name,
			EntityImpl parent,
			Action<IDiContainer> install,
			IEntityPool pool,
			bool isRoot)
		{
			Name = name;
			_pool = pool;
			_parent = parent;
			if (_parent is not null)
			{
				_parent._children ??= new();
				_parent._children.Add(this);
			}
			_assignedState = _effectiveState = EntityState.Despawned;
			_install = install;
			_isRoot = isRoot;
		}
		public static EntityImpl CreateEntity(
			string name,
			EntityImpl parent,
			Action<IDiContainer> install,
			IEntityPool pool,
			bool isRoot)
		{
			var entity = new EntityImpl(name, parent, install, pool, isRoot);
			using var _ = Log.Logger.UseContext(entity);
			entity.Install();
			return entity;
		}
		public override string ToString() => Name;

		#region Lifecycle
		~EntityImpl()
		{
			using var _ = Log.Logger.UseContext(this);
			Log.Logger.Info($"Entity is being destroyed.");
		}
		public void FixedUpdate(UpdateTime time)
		{
			FixedUpdateEvent?.Invoke(time);
			ProcessChild(e => e.FixedUpdate(time));
		}
		public void LateUpdate(UpdateTime time)
		{
			LateUpdateEvent?.Invoke(time);
			ProcessChild(e => e.LateUpdate(time));
		}

		public void Update(UpdateTime time)
		{
			UpdateEvent?.Invoke(time);
			ProcessChild(e => e.Update(time));
		}
		void ProcessChild(Action<EntityImpl> processor)
		{
			if (_children is null)
				return;
			for (var i = 0; i < _children.Count; i++)
			{
				var child = _children[i];
				if (child.State == EntityState.Enabled)
					processor(child);
			}
		}
		#endregion
		#region Events
		private interface IEvents
		{
			void Subscribe(MethodInfo info);
		}
		private sealed class Events<T> : IEvents
		{
			public Action<T> OnEvent;
			public void Subscribe(MethodInfo info)
			{
				var action = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), info);
				OnEvent += action;
			}
		}
		public event Action

			CreateEvent,
			SpawnEvent,
			DespawnEvent,
			PostSpawnEvent,
			AttachEvent,
			EnableEvent,
			DisableEvent;
		public event Action<UpdateTime>
			UpdateEvent,
			LateUpdateEvent,
			FixedUpdateEvent;
		private readonly List<ISubscription> _subscriptions = new();
		private readonly List<IEntitySubscription> _subscriptionsOnAttach = new();
		public void RegisterSubscription(ISubscription sub)
			=> _subscriptions.Add(sub);
		public void RegisterAttachedSubscription(IEntitySubscription subscription)
			=> _subscriptionsOnAttach.Add(subscription);
		#endregion
		#region Container
		readonly Dictionary<Type, IDiStrategy> _elements = new();
		readonly IEntityEventsBinder _eventsBinder;
		public IEnumerable<(Type, object)> GetElements()
		{
			foreach (var kvp in _elements)
				yield return (kvp.Key, kvp.Value.Resolve());
		}
		public bool Installed { get; private set; }
		public void BindStrategy(Type contract, IDiStrategy element)
		{
			if (Installed)
				throw new Exception($"Attempting to bind {contract.Name} after installing.");
			element.AssertValidContract(contract);
			_elements.TryAdd(contract, element);
		}
		public bool TryResolve(Type contract, out object result)
		{
			if (!Installed)
				throw new Exception($"Attempting to resolve {contract.Name} before installing");
			if (_elements.TryGetValue(contract, out var element))
			{
				result = element.Resolve();
				return true;
			}
			if (_attachedTo is not null
				&& _attachedTo.TryResolve(contract, out result))
				return true;
			if (_parent is not null
				&& _parent.TryResolve(contract, out result))
				return true;
			result = default;
			return false;
		}

		#endregion
		#region State
		EntityState _assignedState, _effectiveState, _previousEffectiveState;

		public EntityState State
		{
			get => _effectiveState;
			set => AssignState(value);
		}
		public void InitState(EntityState state) => _assignedState = state;
		private void AssignState(EntityState state)
		{
			//what is dead may never die
			if (_effectiveState == EntityState.Disposed)
				return;
			_assignedState = state;
			BeginStateChange();
			StateFirstPass();
			StateSecondPass();
			FinalizeStateChange();
		}
		bool StateChanged => _effectiveState != _previousEffectiveState;
		void BeginStateChange()
		{
			//compute effective states
			_previousEffectiveState = _effectiveState;
			_effectiveState = _parent is null
				? _assignedState
				: (EntityState)Math.Max((int)_parent._effectiveState, (int)_assignedState);
			//if the effective state is despawned/disposed, there is no turning back
			if (_isRoot && (int)_effectiveState >= (int)EntityState.Despawned)
				_assignedState = _effectiveState;
			//if there is no pool and the object is root, despawned = disposed
			if (_pool is null && _isRoot && _effectiveState == EntityState.Despawned)
				_effectiveState = EntityState.Disposed;
			//update children
			if (StateChanged && _children is not null)
				foreach (var child in _children)
					child.BeginStateChange();
		}
		void ProcessState(
			Action enable,
			Action disable,
			Action spawn,
			Action despawn)
		{
			switch (_previousEffectiveState)
			{
				case EntityState.Enabled:
					disable();
					if (_effectiveState is EntityState.Despawned or EntityState.Disposed)
						despawn();
					break;
				case EntityState.Disabled:
					if (_effectiveState is EntityState.Enabled)
						enable();
					else despawn();
					break;
				case EntityState.Despawned:
					if (_effectiveState is EntityState.Disabled or EntityState.Enabled)
						spawn();
					if (_effectiveState is EntityState.Enabled)
						enable();
					break;
			}
		}

		void StateFirstPass()
		{
			if (!StateChanged)
				return;
			ProcessState(
				EnableState,
				RaiseDisableStateEvent,
				SpawnState,
				RaiseDespawnedStateEvent);
			if (_children is not null)
				foreach (var child in _children)
					child.StateFirstPass();
		}
		void StateSecondPass()
		{
			if (!StateChanged)
				return;
			ProcessState(
				RaiseEnableStateEvent,
				DisableState,
				RaiseSpawnedStateEvent,
				DespawnState);
			if (_children is not null)
				foreach (var child in _children)
					child.StateSecondPass();
		}
		void RaiseDisableStateEvent()
		{
			DisableEvent?.Invoke();
		}
		void RaiseDespawnedStateEvent()
		{
			DespawnEvent?.Invoke();
			foreach (var sub in _externalSubscriptions)
				if (sub is IOnDespawn d)
					d.OnDespawn();
			_despawnCtSource.Cancel();
		}
		void RaiseSpawnedStateEvent()
		{
			SpawnEvent?.Invoke();
			foreach (var sub in _externalSubscriptions)
				if (sub is IOnSpawn s)
					s.OnSpawn();
			PostSpawnEvent?.Invoke();
		}
		void RaiseEnableStateEvent()
		{
			if (_effectiveState == EntityState.Enabled)
				EnableEvent?.Invoke();
		}

		void EnableState()
		{
			foreach (var sub in _subscriptions)
				sub.Subscribe();
			foreach (var sub in _tempSubscriptions)
				sub.Subscribe(this);
		}
		void DisableState()
		{
			foreach (var sub in _tempSubscriptions)
				sub.Unsubscribe(this);
			foreach (var sub in _subscriptions)
				sub.Unsubscribe();
		}
		void SpawnState()
		{
			CurrentSpawnId = ++_lastId;
			SyncParentAttachments();
		}
		void DespawnState()
		{
			CurrentSpawnId = 0;
			DetachFromCurrentEntity();
			_tempSubscriptions.Clear();
			if (_effectiveState != EntityState.Disposed)
				_pool?.Return(this);
		}
		void FinalizeStateChange()
		{
			_previousEffectiveState = _effectiveState;
			if (_effectiveState != EntityState.Disposed)
				return;
			//clear subscriptions
			_subscriptions.DisposeAndClear();
			//dispose components
			foreach ((var _, var strategy) in _elements)
				if (strategy is IDisposable disposable)
					disposable.Dispose();
			//remove yourself from parent and pool if it's not disposed
			//otherwise it will remove you itself later
			if (_parent is not null && _parent._effectiveState != EntityState.Disposed)
			{
				_pool?.Remove(this);
				_parent._children.Remove(this);
			}
			if (_children is not null)
			{
				foreach (var child in _children)
					child.FinalizeStateChange();
				_children.Clear();
			}
			DisposeUnity();
			//clear collections
			if (_childPools is not null)
			{
				foreach (var pool in _childPools)
					pool.Value._entities.Clear();
				_childPools.Clear();
			}
		}
		public void Dispose()
		{
			State = EntityState.Disposed;
		}
		partial void DetachFromCurrentEntity();
		partial void SyncParentAttachments();
		partial void DisposeUnity();

		#endregion
		#region Temporary Subscriptions
		private readonly List<IEntitySubscription> _tempSubscriptions = new();
		public void AddTemporarySubscription(IEntitySubscription subscription)
		{
			if (_tempSubscriptions.AddUnique(subscription)
				&& State == EntityState.Enabled)
				subscription.Subscribe(this);
		}
		public void RemoveTemporarySubscription(IEntitySubscription subscription)
		{
			if (_tempSubscriptions.Remove(subscription)
				&& State == EntityState.Enabled)
				subscription.Unsubscribe(this);
		}
		#endregion
		#region Install
		readonly Action<IDiContainer> _install;
		void Install()
		{
			InstallInternal();
			RaiseCreateEvent();
			if (_children is not null)
				foreach (var child in _children)
					child.RaiseCreateEvent();
		}
		void RaiseCreateEvent() => CreateEvent?.Invoke();
		void InstallInternal()
		{
			this.Instance<IEntity>(this);
			_install?.Invoke(this);
			Installed = true;
			foreach (var element in _elements.Values)
				if (!element.Params.HasFlag(IocParams.Lazy))
					element.Resolve();
			if (_children is not null)
				foreach (var child in _children)
					child.InstallInternal();
		}
		#endregion
		#region Pooling
		sealed class Pool : IEntityPool
		{
			public readonly List<EntityImpl> _entities = new();
			public int SpawnCount = 0;
			public void Return(IEntity entity) => _entities.Add(entity as EntityImpl);
			public void Remove(IEntity entity) => _entities.Remove(entity as EntityImpl);
		}
		Dictionary<IEntityInstaller, Pool> _childPools;
		List<EntityImpl> _children;
		public IEntity CreateChild(IEntityInstaller installer)
		{
			_childPools ??= new();

			if (!_childPools.TryGetValue(installer, out var pool))
			{
				pool = new Pool();
				_childPools.Add(installer, pool);
			}
			EntityImpl child;
			if (pool._entities.Count == 0)
			{
				_children ??= new();
				child = CreateEntity(
					$"{installer.Name} {++pool.SpawnCount}",
					this,
					installer.Install,
					pool,
					true);
				_children.Add(child);
			}
			else child = pool._entities.RemoveLast();
			return child;
		}
		#endregion
		#region Subscriptions
		readonly List<IEntityEventMethod> _externalSubscriptions = new();
		public void AddSubscription(IEntityEventMethod subscription)
			=> _externalSubscriptions.Add(subscription);
		public void RemoveSubscription(IEntityEventMethod subscription)
			=> _externalSubscriptions.Remove(subscription);
		#endregion
	}
}