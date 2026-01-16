using System;
using System.Collections.Generic;
namespace BB
{
    public readonly struct EntityCreatedEvent { }
    public readonly struct EntitySpawnedEvent { }
    public readonly struct PostEntitySpawnedEvent { }
    public readonly struct EntityEnabledEvent { }
    public readonly struct EntityDisabledEvent { }
    public readonly struct EntityDespawnedEvent { }
}
namespace BB.Di
{
    public sealed record EntityV1(
        string Name,
        WorldSetup World,
        IEntityPool Pool,
        IEntityInjector Injector,
        IEntityInstaller Installer) : BaseEntity(Name, World, Pool, Injector, Installer);

    public abstract record BaseEntity(
        string Name,
        WorldSetup World,
        IEntityPool Pool,
        IEntityInjector Injector,
        IEntityInstaller Installer) : IFullEntity
    {
        static ulong _lastSpawnId = 0;
        Dictionary<Type, EntityComponentData> _components;


        public ulong CurrentSpawnId { get; private set; }
        public void Init()
        {
            if (Injector.Components.IsNullOrEmpty())
                return;
            _components = new(Injector.Components.Count);

            foreach (var comp in Injector.Components.Values)
                _components.Add(comp.ContractType, new EntityComponentData(this, comp, comp.ContractType));

            var entityWrapperComponent = GetComponentData(new()
            {
                ContractType = typeof(EntityWrapper),
                Init = true
            });

            var entityWrapper = (EntityWrapper)entityWrapperComponent.Instance;
            entityWrapper.Entity = this;
        }

        public bool TryResolve(Type type, out object result)
        {
            if (!_components.TryGetValue(type, out var comp))
            {
                result = null;
                return Parent?.TryResolve(type, out result) is true;
            }

            if (comp.Instance is null)
                Injector.InjectSingleEntityComponent(this, comp.FactoryComponent);

            result = comp.Instance;
            return true;
        }
        bool _injected;
        public void Inject()
        {
            if (_injected)
            {
                Injector.InjectEntityBeforeSpawn(this);
            }
            else
            {
                Injector.InjectEntityAfterCreate(this);
                _injected = true;
            }
        }

        #region State
        bool _createEventInvoked;
        EntityState
            _effectiveState = EntityState.Despawned,
            _previousEffectiveState = EntityState.Despawned,
            _assignedState = EntityState.Despawned;
        public EntityState State => _effectiveState;
        public void SetState(EntityState state)
        {
            if (_assignedState == state)
                return;
            _assignedState = state;
            UpdateEffectiveState();
            if (_effectiveState == _previousEffectiveState)
                return;

            var dir = (int)_effectiveState - (int)_previousEffectiveState;
            //downstream direction
            if (dir > 0)
            {
                PublishDisableEvent();
                PublishDespawnEvent();
                FinalizeDespawn();
                FinalizeDestroy();
            }
            //upstream direction
            else
            {
                PrepareForSpawn();
                PublishSpawnEvent();
                PublishPostSpawnEvent();
                PublishEnableEvent();
            }
        }
        bool EnteredStateUpstream(EntityState state)
        {
            var s = (int)state;
            return s >= (int)_effectiveState && s < (int)_previousEffectiveState;
        }
        bool EnteredStateDownstream(EntityState state)
        {
            var s = (int)state;
            return s <= (int)_effectiveState && s > (int)_previousEffectiveState;
        }

        public void UpdateEffectiveState()
        {
            _previousEffectiveState = _effectiveState;
            _effectiveState = Parent is BaseEntity parent
                ? (EntityState)Math.Max((int)_assignedState, (int)parent._effectiveState)
                : _assignedState;
            foreach (var i in -_children?.Count)
                _children[i].UpdateEffectiveState();
        }
        public void FinalizeDespawn()
        {
            if (!EnteredStateDownstream(EntityState.Despawned))
                return;
            ClearSubscriptions(_worldSubscriptions);
            ClearSubscriptions(_tempSubscriptions);
            foreach (var i in -_children?.Count)
                _children[i].FinalizeDespawn();
            CurrentSpawnId = 0;
            Parent = null;
            Pool?.ReturnEntity(this);
        }
        private void Subscribe(List<ISubscription> subscriptions)
        {
            if (subscriptions is null)
                return;
            foreach (var subscription in subscriptions)
                subscription.Subscribe();
        }
        private void ClearSubscriptions(List<ISubscription> subscriptions)
        {
            if (subscriptions is null)
                return;
            foreach (var subscription in subscriptions)
                subscription.Unsubscribe();
            subscriptions.Clear();
        }
        public void PrepareForSpawn()
        {
            if (!EnteredStateUpstream(EntityState.Disabled))
                return;
            CurrentSpawnId = ++_lastSpawnId;
            Subscribe(_worldSubscriptions);
            Subscribe(_tempSubscriptions);
            if (!_createEventInvoked)
            {
                _createEventInvoked = true;
                this.Publish<EntityCreatedEvent>();
            }
            foreach (var i in _children?.Count)
                _children[i].PrepareForSpawn();
        }
        public void PublishSpawnEvent()
        {
            if (!EnteredStateUpstream(EntityState.Disabled))
                return;
            this.Publish(new EntitySpawnedEvent());
            foreach (var i in _children?.Count)
                _children[i].PublishSpawnEvent();
        }

        public void PublishPostSpawnEvent()
        {
            if (!EnteredStateUpstream(EntityState.Disabled))
                return;
            this.Publish(new PostEntitySpawnedEvent());
            foreach (var i in _children?.Count)
                _children[i].PublishPostSpawnEvent();
        }

        public void PublishEnableEvent()
        {
            if (!EnteredStateUpstream(EntityState.Enabled))
                return;
            this.Publish(new EntityEnabledEvent());
            foreach (var i in _children?.Count)
                _children[i].PublishEnableEvent();
            _previousEffectiveState = _effectiveState;
        }

        public void PublishDisableEvent()
        {
            if (!EnteredStateDownstream(EntityState.Disabled))
                return;
            this.Publish(new EntityDisabledEvent());
            foreach (var i in -_children?.Count)
                _children[i].PublishDisableEvent();
        }

        public void PublishDespawnEvent()
        {
            if (!EnteredStateDownstream(EntityState.Despawned))
                return;
            this.Publish(new EntityDespawnedEvent());
            foreach (var i in -_children?.Count)
                _children[i].PublishDespawnEvent();
        }
        public void FinalizeDestroy()
        {
            var isDestroyed = EnteredStateDownstream(EntityState.Destroyed);
            _previousEffectiveState = _effectiveState;
            if (!isDestroyed)
                return;
            ClearSubscriptions(_selfSubscriptions);
        }
        #endregion
        #region Subscriptions
        List<ISubscription> _selfSubscriptions, _worldSubscriptions, _tempSubscriptions;
        public void AddSubscription(in EntitySubscriptionContext context)
        {
            switch (context.Source)
            {
                case InjectionSource.Self:
                    _selfSubscriptions ??= new();
                    _selfSubscriptions.Add(context.Subscription);
                    break;
                case InjectionSource.Core:
                case InjectionSource.Game:
                    _worldSubscriptions ??= new();
                    _worldSubscriptions.Add(context.Subscription);
                    break;
                default:
                    _tempSubscriptions ??= new();
                    _tempSubscriptions.Add(context.Subscription);
                    break;
            }

            if (context.Source is InjectionSource.Self
                || State is EntityState.Enabled or EntityState.Disabled)
                context.Subscription.Subscribe();
        }
        public void RemoveSubscription(in EntitySubscriptionContext context)
        {
            var subscriptions = context.Source switch
            {
                InjectionSource.Self => _selfSubscriptions,
                InjectionSource.Core => _worldSubscriptions,
                InjectionSource.Game => _worldSubscriptions,
                _ => _tempSubscriptions
            };
            subscriptions?.Remove(context.Subscription);
        }
        #endregion
        #region Children
        public IReadOnlyCollection<IEntity> Children
            => _children
            ?? (IReadOnlyCollection<IEntity>)Array.Empty<IEntity>();
        List<IFullEntity> _children;
        IEntity _parent;
        public IEntity Parent
        {
            get => _parent;
            set
            {
                if (_parent is BaseEntity parent)
                    parent._children?.Remove(this);
                _parent = value;
                if (_parent is BaseEntity newParent)
                {
                    newParent._children ??= new();
                    newParent._children.Add(this);
                }
            }
        }

        public IReadOnlyCollection<EntityComponentData> GetElements()
            => _components?.Values
            ?? (IReadOnlyCollection<EntityComponentData>)Array.Empty<EntityComponentData>();

        public EntityComponentData GetComponentData(in GetComponentDataContext context)
        {
            var data = GetComponentData(this, context.ContractType);
            if (data is null)
                throw new DiException(
                    $"Could not resolve {context.ContractType} " +
                    $"in component {context.RequestingType?.Name} " +
                    $"in entity {Name}");

            if (context.Init)
                data.Init();
            return data;
        }

        static EntityComponentData GetComponentData(BaseEntity entity, Type contractType)
        {
            if (entity._components.TryGetValue(contractType, out var result))
                return result;
            if (entity.Parent is BaseEntity parent)
                return GetComponentData(parent, contractType);
            return null;
        }
        #endregion
    }
    public sealed record EntityComponentData(
        IEntity Entity,
        IDiComponent FactoryComponent,
        Type ContractType)
    {
        public object Instance { get; set; }
        public bool Init()
        {
            if (FactoryComponent.AlwaysCreate)
                Instance = FactoryComponent.Create(Entity);

            if (Instance is not null)
                return false;

            Instance = FactoryComponent.Create(Entity);
            return true;
        }
        public override string ToString()
        {
            var cType = ContractType;
            var iType = FactoryComponent.InstanceType;
            if (typeof(IEvent).IsAssignableFrom(cType) && cType.IsGenericType)
                return cType.GetGenericArguments()[0].Name;
            var typeName = iType == cType ? iType.Name : $"{cType.Name}:{iType.Name}";
            if (Instance is null)
                return $"{typeName}:NULL";
            return typeName;
        }
    }
}