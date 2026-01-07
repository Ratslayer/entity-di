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
                World = container.World,
                InstanceType = typeof(TVar),
                ContractType = typeof(TVar),
                Lazy = true,
                AdditionalParams = new[]
                {
                    (typeof(InitialVariableValue),
                    (object)new InitialVariableValue { Value=value})
                }
            }));
            container.Event<TVar>();
        }
        public static void Var<T>(this IDiContainer container)
            where T : IVariable
        {
            container.AddComponent(new ConstructDiComponent(new()
            {
                World = container.World,
                InstanceType = typeof(T),
                ContractType = typeof(T),
                Lazy = true,
                AdditionalParams = new[]
                {
                    (typeof(InitialVariableValue),
                    (object)new InitialVariableValue())
                }
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
                World = container.World,
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
               World = container.World,
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
                World = container.World,
                ContractType = typeof(IEvent<T>),
                InstanceType = typeof(DefaultEvent<T>),
                Lazy = true
            }));
        public static void CascadingEvent<T>(this IDiContainer container)
            => container.AddComponent(new ConstructDiComponent(new()
            {
                World = container.World,
                ContractType = typeof(IEvent<T>),
                InstanceType = typeof(CascadingEvent<T>),
                Lazy = true
            }));
        public static void Instance(this IDiContainer container, Type contractType, object instance)
            => container.AddComponent(new InstanceDiComponent(new()
            {
                World = container.World,
                ContractType = contractType,
                InstanceType = instance.GetType(),
            }, instance));
        public static void Instance<T>(this IDiContainer container, T instance)
            => container.Instance(typeof(T), instance);
    }
}