using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public abstract class StackValue<TSelf, TValue> : IStackValue<TValue>
        where TSelf : StackValue<TSelf, TValue>
    {
        [Inject]
        IEvent<TSelf> _publisher;
        TValue _defaultValue;
        readonly List<StackSourcedValue<TValue>> _stack = new();
        public void SetValueNoUpdate(TValue value)
        {
            _defaultValue = value;
        }
        [OnEvent(typeof(EntityDespawnedEvent))]
        void OnDespawn()
        {
            _stack.Clear();
        }
        public TValue Value
        {
            get
            {
                if (_stack.Count == 0)
                    return _defaultValue;
                var top = _stack[^1];
                if (top.ValueGetter is not null)
                    return top.ValueGetter();
                return top.Value;
            }
        }
        public TValue PreviousValue { get; private set; }

        public int Count => _stack.Count;
        public bool IsDirty { get; set; }
        public bool AutoFlushDisabled { get; set; }
        public TValue this[int index] => _stack[index].Value;

        public bool HasValue(out TValue value)
        {
            value = Value;
            return _stack.Count > 0;
        }
        public StackValuePushDisposable<TValue> Push(in StackSourcedValue<TValue> value)
        {
            if (!IsDirty)
                PreviousValue = Value;
            _stack.Add(value);
            _stack.SortByPriority();
            this.SetDirtyAndAutoFlushChanges();
            return new((TSelf)this, value);
        }
        public bool Pop(in StackSourcedValue<TValue> value)
        {
            if (_stack.Count == 0)
                return false;

            if (EqualityComparer<StackSourcedValue<TValue>>.Default.Equals(_stack[^1], value))
            {
                _stack.RemoveLast();
                this.SetDirtyAndAutoFlushChanges();
                return true;
            }

            return _stack.Remove(value);
        }
        public bool Contains(in StackSourcedValue<TValue> value)
            => _stack.Contains(value);
        public override string ToString()
            => $"{typeof(TSelf).Name}: {StringExtensions.SafeToString(Value)}";

        public IEnumerator<TValue> GetEnumerator()
            => _stack.Select(x => x.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator
            TValue(StackValue<TSelf, TValue> state)
            => state.Value;



        public async UniTask WaitForValue(TValue value)
        {
            while (true)
            {
                if (IsCurrentValue(value))
                    return;
                await UniTask.NextFrame();
            }
        }
        public virtual bool IsCurrentValue(TValue value)
            => EqualityComparer<TValue>.Default.Equals(Value, value);

        public virtual void ForceFlushChanges()
        {
            _publisher.Publish((TSelf)this);
        }

        public IEnumerable<StackSourcedValue> GetTypelessSourceValues()
        {
            foreach (var value in _stack.Inverse())
                yield return new()
                {
                    Value = value.Value,
                    Priority = value.Priority,
                    Source = value.Source
                };
        }
    }

    public readonly struct StackSourcedValue : IPriority
    {
        public object Value { get; init; }
        public int Priority { get; init; }
        public DataSourceDesc Source { get; init; }
        public override string ToString()
            => $"{Value} ({Source})";
    }
    public readonly struct StackSourcedValue<TValue> : IPriority
    {
        public TValue Value { get; init; }
        public Func<TValue> ValueGetter { get; init; }
        public int Priority { get; init; }
        public DataSourceDesc Source { get; init; }
    }
    public readonly struct DataSourceDesc
    {
        public Entity Entity { get; init; }
        public Type Type { get; init; }
        public override string ToString()
            => $"{Entity}:{Type.Name}";
        public static implicit operator DataSourceDesc(EntitySystem system)
            => new()
            {
                Entity = system.Entity,
                Type = system.GetType()
            };
        public static implicit operator DataSourceDesc((Entity, object) e)
            => new()
            {
                Entity = e.Item1,
                Type = e.Item2.GetType()
            };
    }
}