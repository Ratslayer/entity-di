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
        public static void Var<TVar, TValue>(this IDiContainer container, TValue value = default)
            where TVar : Variable<TVar, TValue>, new()
        {
            container.AddComponent(new ConstructDiComponent(new()
            {
                InstanceType = typeof(TVar),
                ContractType = typeof(TVar),
                Lazy = true,
                AdditionalParams = new[] { (typeof(TValue), (object)default(TValue)) }
            }));
            container.Event<TVar>();
        }
        public static void Var<T>(this IDiContainer container)
            where T : IVariable
        {
            container.AddComponent(new ConstructDiComponent(new()
            {
                InstanceType = typeof(T),
                ContractType = typeof(T),
                Lazy = true,
            }));
            container.Event<T>();
        }
        public static void System<T>(
            this IDiContainer container,
            params object[] args)
            where T : new()
            => container.System<T, T>(args);
        public static void System<TContract, TInstance>(
            this IDiContainer container,
            params object[] args)
            where TInstance : TContract, new()
            => container.AddComponent(new ConstructDiComponent(new()
            {
                ContractType = typeof(TContract),
                InstanceType = typeof(TInstance),
                Lazy = false,
                TypelessAdditionalParams = args
            }));
        public static void SystemWithArgs<TContract, TInstance>(
           this IDiContainer container,
           params (Type, object)[] args)
           where TInstance : TContract, new()
           => container.AddComponent(new ConstructDiComponent(new()
           {
               ContractType = typeof(TContract),
               InstanceType = typeof(TInstance),
               Lazy = false,
               AdditionalParams = args
           }));
        public static void SystemWithArgs<TContract>(
           this IDiContainer container,
           params (Type, object)[] args)
            where TContract : new()
            => SystemWithArgs<TContract, TContract>(container, args);

        public static void Event<T>(this IDiContainer container)
            => container.AddComponent(new ConstructDiComponent(new()
            {
                ContractType = typeof(IEvent<T>),
                InstanceType = typeof(DefaultEvent<T>),
                Lazy = true
            }));
        public static void CascadingEvent<T>(this IDiContainer container)
            => container.AddComponent(new ConstructDiComponent(new()
            {
                ContractType = typeof(IEvent<T>),
                InstanceType = typeof(CascadingEvent<T>),
                Lazy = true
            }));
        //public static IDiStrategy WithArgsExplicit(this IDiConstructorStrategy strategy, params (Type, object)[] args)
        //{
        //    strategy.SetConstructorArgs(args);
        //    return strategy;
        //}
        //public static IDiStrategy WithArgs(this IDiConstructorStrategy strategy, params object[] args)
        //{
        //    if (args is null || args.Length == 0)
        //        return strategy;
        //    using var _ = Log.Logger.UseContext(strategy);
        //    //convert args to explicit args
        //    (Type, object)[] explicitArgs;

        //    if (args.GetType() == typeof(object[]))
        //    {
        //        explicitArgs = new (Type, object)[args.Length];
        //        foreach (var i in args.Length)
        //            if (args[i] is null)
        //                throw new Exception("One of the args passed during binding is null");
        //            else explicitArgs[i] = (args[i].GetType(), args[i]);
        //    }
        //    //if args type is not object[], then it means that an array was passed as an argument
        //    //in this case, treat it as a single param arg
        //    else explicitArgs = new (Type, object)[1] { (args.GetType(), args) };

        //    strategy.WithArgsExplicit(explicitArgs);
        //    return strategy;
        //}
        public static void Instance(this IDiContainer container, Type contractType, object instance)
            => container.AddComponent(new InstanceDiComponent(new()
            {
                ContractType = contractType,
                InstanceType = instance.GetType(),
            }, instance));
        //public static void InjectedInstance(this IDiContainer container, Type contractType, object instance)
        //    => container
        //    .Instance(contractType, instance)
        //    .Inject()
        //    .BindEvents()
        //    .NonLazy();
        //public static void InjectedInstance<T>(this IDiContainer container, T instance)
        //    => container
        //    .Instance(typeof(T), instance)
        //    .Inject()
        //    .BindEvents()
        //    .NonLazy();
        //public static IDiConstructorStrategy Construct(this IDiContainer container, Type contractType, Type instanceType)
        //{
        //    var strategy = new ConstructedIocStrategy((IEntity)container, instanceType);
        //    container.BindStrategy(contractType, strategy);
        //    return strategy;
        //}
        public static void Instance<T>(this IDiContainer container, T instance)
            => container.Instance(typeof(T), instance);
        //public static IDiConstructorStrategy Construct<TContract, TInstance>(
        //    this IDiContainer container)
        //    => container.Construct(typeof(TContract), typeof(TInstance));
        //public static IDiConstructorStrategy Construct<TContract>(this IDiContainer container)
        //    => container.Construct<TContract, TContract>();
        //public static IDiStrategy Lazy(this IDiStrategy strategy)
        //{
        //    strategy.Params |= IocParams.Lazy;
        //    return strategy;
        //}
        //public static IDiStrategy NonLazy(this IDiStrategy strategy)
        //{
        //    strategy.Params &= ~IocParams.Lazy;
        //    return strategy;
        //}
        //public static IDiStrategy Inject(this IDiStrategy strategy)
        //{
        //    strategy.Params |= IocParams.Inject;
        //    return strategy;
        //}
        //public static IDiStrategy BindEvents(this IDiStrategy strategy)
        //{
        //    strategy.Params |= IocParams.BindEvents;
        //    return strategy;
        //}
    }
}