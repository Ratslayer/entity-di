using Newtonsoft.Json;
using System;
using UnityEngine;
namespace BB
{
    public abstract class BaseSerializer<TSelf, TTarget, TData> : IEntityComponentSerializer
        where TSelf : BaseSerializer<TSelf, TTarget, TData>, new()
    {
        static TSelf _default;
        public static TSelf Default
        {
            get
            {
                _default ??= new();
                return _default;
            }
        }
        public void Apply(Entity entity, object serializedData)
        {
            if (!entity.Has(out TTarget target))
                return;
            var data = JsonConvert.DeserializeObject<TData>(serializedData.ToString());
            Apply(target, data);
        }

        public object Serialize(object target)
        {
            AssertProperType(target);
            return Serialize((TTarget)target);
        }
        protected abstract void Apply(TTarget target, TData data);
        protected abstract TData Serialize(TTarget target);
        void AssertProperType(object target)
        {
            if (target is not TTarget)
                throw new ArgumentException(
                    $"{GetType().Name} serializer expects target of type{typeof(TTarget).Name}. " +
                    $"Actual type: {target.GetType().Name}.");
        }

        protected bool IsLoadableBehaviour(in TransformAdapter transform, out string key)
        {
            if (!transform._transform.TryGetComponent(out LoadableBehaviour lb))
            {
                key = null;
                return false;
            }
            key = lb.Key;
            return true;
        }
        protected string GetLoadableBehaviourKey(in TransformAdapter transform)
            => IsLoadableBehaviour(transform, out var key) ? key : null;
        protected bool HasLoadableBehaviour<T>(string key, out T result)
            where T:Component
        {
            if(!World.Require<ILoadableBehaviours>().TryGet(key, out var lb))
            {
                result = default;
                return false;
            }

            if(!lb.TryGetComponent(out result))
            {
                Log.Error($"{key} loadable behaviour was found, " +
                    $"but it had no component of type {typeof(T).Name}");
                return false;
            }

            return true;
        }
    }
}