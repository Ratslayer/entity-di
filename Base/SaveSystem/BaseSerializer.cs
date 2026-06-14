using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
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

        protected virtual void ApplySpawn(TTarget target, TData data)
        {
        }

        protected virtual void ApplyAfterSpawn(TTarget target, TData data)
        {
        }

        protected abstract TData Serialize(TTarget target);

        public object Serialize(object target)
        {
            AssertProperType(target);
            return Serialize((TTarget)target);
        }

        protected bool IsLoadableBehaviour(in TransformAdapter transform, out string key)
        {
            if (!transform._transform.TryGetComponent(out LoadableComponent lb))
            {
                key = null;
                return false;
            }

            key = lb.Key;
            return true;
        }

        protected string GetLoadableBehaviourKey(in TransformAdapter transform)
            => IsLoadableBehaviour(transform, out var key) ? key : null;

        protected bool HasLoadableComponent<T>(string key, out T result)
            where T : Component
        {
            if (!World.Require<ILoadableComponents>().TryGet(key, out var lb))
            {
                result = default;
                return false;
            }

            if (!lb.TryGetComponent(out result))
            {
                LogError($"{key} loadable behaviour was found, " +
                         $"but it had no component of type {typeof(T).Name}");
                return false;
            }

            return true;
        }

        void AssertProperType(object target)
        {
            if (target is not TTarget)
                throw new ArgumentException(
                    $"{GetType().Name} serializer expects target of type{typeof(TTarget).Name}. " +
                    $"Actual type: {target.GetType().Name}.");
        }

        protected bool IsValidLoadableAsset(ILoadableAsset asset)
        {
            var a = (BaseScriptableObject)asset;
            if (!a)
                return false;

            if (string.IsNullOrWhiteSpace(asset.AssetLoadKey))
            {
                LogError($"{a.name} has no AssetLoadKey");
                return false;
            }

            var assets = World.Require<ILoadableAssets>();
            if (!assets.HasAsset(asset.AssetLoadKey, out ILoadableAsset existingAsset))
            {
                LogError($"{a.name} is not added to LoadableAssets");
                return false;
            }

            if (asset != existingAsset)
            {
                LogError($"Another asset {existingAsset} is already added by key {asset.AssetLoadKey}");
                return false;
            }

            return true;

            void LogError(string msg)
            {
                GetLogger()
                    .WithUnityObject(a)
                    .WithClass(GetType())
                    .Error(msg);
            }
        }

        protected bool HasLoadableAsset<T>(string key, out T asset)
            where T : BaseScriptableObject, ILoadableAsset
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                asset = null;
                return false;
            }

            var assets = World.Require<ILoadableAssets>();
            if (!assets.HasAsset(key, out asset) || !asset)
            {
                LogError($"No loadable asset found for key {key}");
                return false;
            }

            return true;
        }

        protected T GetLoadableAsset<T>(string key)
            where T : BaseScriptableObject, ILoadableAsset
            => HasLoadableAsset(key, out T asset) ? asset : default;

        protected string GetValidKey(ILoadableAsset asset)
        {
            if (IsValidLoadableAsset(asset))
                return asset.AssetLoadKey;

            return null;
        }


        public void ApplySpawn(in DeserializationContext context)
        {
            if (context.Component is not TTarget target)
                return;
            var serializedData = context.SerializedData.ToString();
            var data = JsonConvert.DeserializeObject<TData>(serializedData);
            ApplySpawn(target, data);
        }

        public void ApplyAfterSpawn(in DeserializationContext context)
        {
            if (!context.Entity.Has(out TTarget target))
                return;
            var data = JsonConvert.DeserializeObject<TData>(context.SerializedData.ToString());
            ApplyAfterSpawn(target, data);
        }

        protected ILoggerScope GetLogger([CallerMemberName] string caller = null)
            => World.Entity.GetLogger(caller);

        protected void LogError(string msg, [CallerMemberName] string caller = null)
            => GetLogger(caller).Error(msg);
    }
}