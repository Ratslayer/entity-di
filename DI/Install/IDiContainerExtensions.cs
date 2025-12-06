using System;
namespace BB.Di
{
    public static class IDiContainerExtensions
    {
        public static bool TryResolve<T>(this IEntity entity, out T result)
        {
            if (entity.TryResolve(typeof(T), out var obj))
            {
                result = (T)obj;
                return true;
            }
            result = default;
            return false;
        }
        public static object Resolve(this IEntity entity, Type type)
        {
            using var _ = Log.Logger.UseContext(entity);
            if (!entity.TryResolve(type, out var result))
                throw new DiException($"{type.FullName} contract not found in entity {entity.Name}.");
            return result;
        }
        public static void Var<TVar, TValue>(this IDiContainer container, TValue value)
            where TVar : Variable<TVar, TValue>, new()
        {
            var v = new TVar();
            v.SetValueNoUpdate(value);
            container.Instance(v)
                .Inject()
                .BindEvents();
            container.Event<TVar>();
        }
        public static void Var<T>(this IDiContainer container)
            where T : IVariable
        {
            container.Construct<T>()
                .Lazy()
                .Inject()
                .BindEvents();
            container.Event<T>();
        }
        public static IDiStrategy System<T>(
            this IDiContainer container,
            params object[] args)
            => container.System<T, T>(args);
        public static IDiStrategy SystemWithArgs<TContract>(
            this IDiContainer container,
            params (Type, object)[] args)
            => container.SystemWithArgs<TContract, TContract>(args);
        public static IDiStrategy SystemWithArgs<TContract, TInstance>(
            this IDiContainer container, params (Type, object)[] args)
        {
            var result = container.Construct<TContract, TInstance>()
                .WithArgsExplicit(args)
                .Inject()
                .BindEvents()
                .NonLazy();
            return result;
        }
        public static IDiStrategy System<TContract, TInstance>(
            this IDiContainer container,
            params object[] args)
            where TInstance : TContract
        {
            var result = container.Construct<TContract, TInstance>()
                .Inject()
                .BindEvents()
                .NonLazy() as IDiConstructorStrategy;
            if (args is not null)
                result.WithArgs(args);
            return result;
        }

        public static IDiStrategy Service<TContract, TInstance>(
            this IDiContainer container,
            params object[] args)
        {
            var result = container.Construct<TContract, TInstance>();
            if (args is not null)
                result.WithArgs(args);
            return result;
        }
        public static void Event<T>(this IDiContainer container)
            => container
            .Construct<IEvent<T>, DefaultEventImpl<T>>()
            .Lazy();
        public static void CascadingEvent<T>(this IDiContainer container)
            => container
            .Construct<IEvent<T>, CascadingEvent<T>>()
            .Lazy();
        public static IDiStrategy WithArgsExplicit(this IDiConstructorStrategy strategy, params (Type, object)[] args)
        {
            strategy.SetConstructorArgs(args);
            return strategy;
        }
        public static IDiStrategy WithArgs(this IDiConstructorStrategy strategy, params object[] args)
        {
            if (args is null || args.Length == 0)
                return strategy;
            using var _ = Log.Logger.UseContext(strategy);
            //convert args to explicit args
            (Type, object)[] explicitArgs;

            if (args.GetType() == typeof(object[]))
            {
                explicitArgs = new (Type, object)[args.Length];
                foreach (var i in args.Length)
                    if (args[i] is null)
                        throw new Exception("One of the args passed during binding is null");
                    else explicitArgs[i] = (args[i].GetType(), args[i]);
            }
            //if args type is not object[], then it means that an array was passed as an argument
            //in this case, treat it as a single param arg
            else explicitArgs = new (Type, object)[1] { (args.GetType(), args) };

            strategy.WithArgsExplicit(explicitArgs);
            return strategy;
        }
        public static IDiStrategy Instance(this IDiContainer container, Type contractType, object instance)
        {
            var strategy = new InstanceIocStrategy((IEntity)container, instance);
            container.BindStrategy(contractType, strategy);
            return strategy;
        }
        public static void InjectedInstance(this IDiContainer container, Type contractType, object instance)
            => container
            .Instance(contractType, instance)
            .Inject()
            .BindEvents()
            .NonLazy();
        public static void InjectedInstance<T>(this IDiContainer container, T instance)
            => container
            .Instance(typeof(T), instance)
            .Inject()
            .BindEvents()
            .NonLazy();
        public static IDiConstructorStrategy Construct(this IDiContainer container, Type contractType, Type instanceType)
        {
            var strategy = new ConstructedIocStrategy((IEntity)container, instanceType);
            container.BindStrategy(contractType, strategy);
            return strategy;
        }
        public static IDiStrategy Instance<T>(this IDiContainer container, T instance)
            => container.Instance(typeof(T), instance);
        public static IDiConstructorStrategy Construct<TContract, TInstance>(
            this IDiContainer container)
            => container.Construct(typeof(TContract), typeof(TInstance));
        public static IDiConstructorStrategy Construct<TContract>(this IDiContainer container)
            => container.Construct<TContract, TContract>();
        public static IDiStrategy Lazy(this IDiStrategy strategy)
        {
            strategy.Params |= IocParams.Lazy;
            return strategy;
        }
        public static IDiStrategy NonLazy(this IDiStrategy strategy)
        {
            strategy.Params &= ~IocParams.Lazy;
            return strategy;
        }
        public static IDiStrategy Inject(this IDiStrategy strategy)
        {
            strategy.Params |= IocParams.Inject;
            return strategy;
        }
        public static IDiStrategy BindEvents(this IDiStrategy strategy)
        {
            strategy.Params |= IocParams.BindEvents;
            return strategy;
        }
    }
}