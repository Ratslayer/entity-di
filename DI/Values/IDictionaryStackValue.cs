using BB.Di;
using BB.Serialized.States;
using System;
using System.Collections.Generic;

namespace BB.Di
{
    public interface IDictionaryStackValue
    {
    }
    public interface IDictionaryStackValue<TKey, TValue> : IDictionaryStackValue
    {
        DictionaryStackValuePushDisposable<TKey,TValue> Push(in DictionaryStackSourcedValue<TKey, TValue> value);
        bool Pop(in DictionaryStackSourcedValue<TKey, TValue> value);
        bool HasValue(TKey key, out TValue value);
    }
    public abstract record DictionaryStackValue<TSelf, TKey, TValue>
        : IDictionaryStackValue<TKey, TValue>, IVariable, IAutoFlushable, IDirtyFlushable
        where TSelf : DictionaryStackValue<TSelf, TKey, TValue>
    {
        [Inject]
        IEvent<TSelf> _event;
        readonly Dictionary<TKey, List<DictionaryStackSourcedValue<TKey, TValue>>> _stacks = new();
        public bool AutoFlushDisabled { get; set; }
        public bool IsDirty { get; set; }

        public void ForceFlushChanges()
        {
            _event.Publish((TSelf)this);
        }

        public bool Pop(in DictionaryStackSourcedValue<TKey, TValue> value)
        {
            if (!_stacks.TryGetValue(value.Key, out var stack))
                return false;
            if (stack.Count == 0)
                return false;
            var top = stack[^1];
            stack.Remove(value);
            if (!EqualityComparer<DictionaryStackSourcedValue<TKey, TValue>>.Default.Equals(top, value))
                IsDirty = true;

            this.AutoFlushChangesIfDirty();
            return true;
        }

        public DictionaryStackValuePushDisposable<TKey,TValue> Push(in DictionaryStackSourcedValue<TKey, TValue> value)
        {
            var stack = _stacks.GetOrCreate(value.Key);
            if (stack.Count == 0)
            {
                stack.Add(value);
                IsDirty = true;
            }
            else
            {
                var top = stack[^1];
                stack.Add(value);
                stack.SortByPriority();
                if (EqualityComparer<DictionaryStackSourcedValue<TKey, TValue>>.Default.Equals(top, value))
                    IsDirty = true;
            }

            this.AutoFlushChangesIfDirty();

            return new()
            {
                Dictionary = this,
                Value = value
            };
        }

        public bool HasValue(TKey key, out TValue value)
        {
            if (!_stacks.TryGetValue(key, out var stack)
                || stack.Count == 0)
            {
                value = default;
                return false;
            }

            value = stack[^1].Value;
            return true;
        }
    }
    public readonly struct DictionaryStackSourcedValue<TKey, TValue> : IPriority
    {
        public TKey Key { get; init; }
        public TValue Value { get; init; }
        public int Priority { get; init; }
        public DataSourceDesc Source { get; init; }

    }
    public readonly struct DictionaryStackValuePushDisposable<TKey, TValue> : IDisposable
    {
        public IDictionaryStackValue<TKey, TValue> Dictionary { get; init; }
        public DictionaryStackSourcedValue<TKey, TValue> Value { get; init; }

        public void Dispose() => Dictionary?.Pop(Value);
    }
}
namespace BB.Serialized.Actions
{
    public abstract class BasePushDictionaryStackValueAction<TDictionary, TKey, TValue>
        : SerializedStateActionSync
        where TDictionary : DictionaryStackValue<TDictionary, TKey, TValue>
    {
        DictionaryStackValuePushDisposable<TKey, TValue>? _disposable;
        protected abstract (TKey, TValue) GetKvp(in SerializedActionContext context);
        protected override void InvokeSync(SerializedActionContext context)
        {
            var (key, value) = GetKvp(context);
            _disposable = context.Entity.Get<TDictionary>()?.Push(new()
            {
                Key = key,
                Value = value,
                Source = (context.Entity, this)
            });
        }
        protected override void InvokeExitSync(SerializedActionContext context)
        {
            _disposable?.Dispose();
            _disposable = null;
        }
    }
}