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
        IEntityPool Pool,
        IEntityInstaller Installer) : BaseEntity(Name, Pool, Installer);

    public abstract record BaseEntity(
        string Name,
        IEntityPool Pool,
        IEntityInstaller Installer) : IFullEntity
    {
        static ulong _lastSpawnId = 0;
        Dictionary<Type, Component> _components;

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
            (Parent as IFullEntity).RemoveChild(this);
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
        public void PublishSpawnEvent()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            this.Publish(new EntitySpawnedEvent());
            foreach (var child in _children)
                child.PublishSpawnEvent();
        }

        public void PublishPostSpawnEvent()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            this.Publish(new PostEntitySpawnedEvent());
            foreach (var child in _children)
                child.PublishPostSpawnEvent();
        }

        public void PublishEnableEvent()
        {
            if (!IsEntered(EntityState.Enabled))
                return;
            this.Publish(new EntityEnabledEvent());
            foreach (var child in _children)
                child.PublishEnableEvent();
        }

        public void PublishDisableEvent()
        {
            if (!IsEntered(EntityState.Disabled))
                return;
            this.Publish(new EntityDisabledEvent());
            foreach (var child in _children)
                child.PublishSpawnEvent();
        }

        public void PublishDespawnEvent()
        {
            if (!IsEntered(EntityState.Despawned))
                return;
            this.Publish(new EntityDespawnedEvent());
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
        #region Children
        public IReadOnlyCollection<IEntity> Children
            => _children
            ?? (IReadOnlyCollection<IEntity>)Array.Empty<IEntity>();
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

        public IEnumerable<EntityElement> GetElements()
        {
            if (_components is null)
                yield break;

            foreach (var (type, comp) in _components)
                yield return new EntityElement
                {
                    ContractType = type,
                    Instance = comp.Instance,
                    DiComponent = comp.FactoryComponent
                };
        }
        #endregion

        sealed class Component
        {
            public object Instance { get; set; }
            public IDiComponent FactoryComponent { get; init; }
        }
    }
}