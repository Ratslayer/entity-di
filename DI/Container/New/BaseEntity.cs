using System;
using System.Collections.Generic;
namespace BB
{
    public readonly struct CreatedEvent { }
    public readonly struct SpawnedEvent { }
    public readonly struct PostSpawnedEvent { }
    public readonly struct EnabledEvent { }
    public readonly struct DisabledEvent { }
    public readonly struct DespawnedEvent { }
}
namespace BB.Di
{
    public sealed class EntityV1 : BaseEntity
    {
        public EntityV1(
            string name,
            IEntityPool pool,
            IReadOnlyDictionary<Type, IDiComponent> components)
            : base(name, pool) { }
    }
    public abstract class BaseEntity : IFullEntity
    {
        static ulong _lastSpawnId = 0;
        Dictionary<Type, Component> _components;
        readonly IEntityPool _pool;
        public BaseEntity(
            string name,
            IEntityPool pool)
        {
            Name = name;
            _pool = pool;
        }

        public string Name { get; private set; }

        public IEntity Parent { get; set; }

        public ulong CurrentSpawnId { get; private set; }
        public void Init(IReadOnlyCollection<IDiComponent> components)
        {
            if (components.IsNullOrEmpty())
                return;
            _components = new(components.Count);
            var createContext = new DiComponentCreateContext
            {
                Entity = this
            };
            foreach (var comp in components)
            {
                var instance = comp.Lazy ? null : comp.Create(createContext);
                _components.Add(comp.ContractType, new Component
                {
                    Instance = instance,
                    FactoryComponent = comp
                });
            }
            foreach (var value in _components.Values)
            {
                if (value.Instance is not null)
                    value.FactoryComponent.Inject(new()
                    {
                        Instance = value.Instance,
                        Entity = this
                    });
            }
        }

        public bool TryResolve(Type type, out object result)
        {
            if (!_components.TryGetValue(type, out var comp))
            {
                result = null;
                return false;
            }

            result = comp.Instance;
            if (result is null)
            {
                comp.Instance = comp.FactoryComponent.Create(new()
                {
                    Entity = this,
                });
                comp.FactoryComponent.Inject(new()
                {
                    Instance = comp.Instance,
                    Entity = this,
                });
                result = comp.Instance;
            }

            return true;
        }

        #region State
        EntityState _effectiveState, _previousEffectiveState, _assignedState;
        public EntityState State => _effectiveState;
        public void SetState(in SetEntityStateContext context)
        {
            if (_assignedState == context.State)
                return;
            _assignedState = context.State;
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
        bool IsEntered(EntityState state)
        {
            var from = (int)_previousEffectiveState;
            var to = (int)_effectiveState;
            var value = (int)state;
            var dir = to - from;
            return dir switch
            {
                > 0 => value > from && value <= to,
                < 0 => value < to && value >= from,
                _ => false
            };
        }
        public void UpdateEffectiveState()
        {
            _previousEffectiveState = _effectiveState;
            _effectiveState = (EntityState)Math.Max((int)_assignedState, (int)_effectiveState);
            foreach (var child in _children)
                child.UpdateEffectiveState();
        }

        public void PrepareForSpawn()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            CurrentSpawnId = ++_lastSpawnId;
            Subscribe(_worldSubscriptions);
            Subscribe(_tempSubscriptions);
        }
        public void FinalizeDespawn()
        {
            if (!IsEntered(EntityState.Despawned))
                return;
            ClearSubscriptions(_worldSubscriptions);
            ClearSubscriptions(_tempSubscriptions);
            foreach (var child in _children)
                child.FinalizeDespawn();
            CurrentSpawnId = 0;
            Parent.RemoveChild(this);
            _pool.ReturnEntity(this);
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
        public void PublishSpawnEvent()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            this.Publish(new SpawnedEvent());
            foreach (var child in _children)
                child.PublishSpawnEvent();
        }

        public void PublishPostSpawnEvent()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            this.Publish(new PostSpawnedEvent());
            foreach (var child in _children)
                child.PublishPostSpawnEvent();
        }

        public void PublishEnableEvent()
        {
            if (!IsEntered(EntityState.Enabled))
                return;
            this.Publish(new EnabledEvent());
            foreach (var child in _children)
                child.PublishEnableEvent();
        }

        public void PublishDisableEvent()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            this.Publish(new DisabledEvent());
            foreach (var child in _children)
                child.PublishSpawnEvent();
        }

        public void PublishDespawnEvent()
        {
            if (!IsEntered(EntityState.Despawned))
                return;
            this.Publish(new DespawnedEvent());
            foreach (var child in _children)
                child.PublishSpawnEvent();
        }
        public void FinalizeDestroy()
        {
            if (!IsEntered(EntityState.Destroyed))
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
                case InjectionSource.World:
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
                InjectionSource.World => _worldSubscriptions,
                InjectionSource.Game => _worldSubscriptions,
                _ => _tempSubscriptions
            };
            subscriptions?.Remove(context.Subscription);
        }
        #endregion
        #region Update
        List<Action<UpdateTime>> _updateActions, _fixedUpdateActions, _lateUpdateActions;
        public void AddUpdateSubscription(Action<UpdateTime> action, UpdateType type)
        {
            switch (type)
            {
                case UpdateType.Fixed:
                    _fixedUpdateActions ??= new();
                    _fixedUpdateActions.Add(action);
                    break;
                case UpdateType.Late:
                    _lateUpdateActions ??= new();
                    _lateUpdateActions.Add(action);
                    break;
                default:
                    _updateActions ??= new();
                    _updateActions.Add(action);
                    break;
            }
        }
        public void Update(in UpdateTime time, UpdateType type)
        {
            if (State is not EntityState.Enabled)
                return;

            var subscriptions = type switch
            {
                UpdateType.Fixed => _fixedUpdateActions,
                UpdateType.Late => _lateUpdateActions,
                _ => _updateActions
            };
            if (subscriptions is not null)
                foreach (var action in subscriptions)
                    action(time);
            if (_children is not null)
                foreach (var child in _children)
                    child.Update(time, type);
        }
        #endregion
        #region Children
        public IReadOnlyCollection<IEntity> Children => _children;
        List<IFullEntity> _children;
        public void AddChild(IFullEntity entity)
        {
            _children ??= new();
            _children.Add(entity);
        }

        public void RemoveChild(IFullEntity entity)
        {
            _children?.Remove(entity);
        }
        #endregion

        sealed class Component
        {
            public object Instance { get; set; }
            public IDiComponent FactoryComponent { get; init; }
        }
    }
}