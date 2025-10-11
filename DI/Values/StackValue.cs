using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace BB.Di
{
    public abstract record StackValue<TSelf, TValue> : IStackValue<TValue>
        where TSelf : StackValue<TSelf, TValue>
    {
        [Inject]
        IEvent<TSelf> Publisher { get; set; }
        TValue _defaultValue;
        readonly List<SourcedValue<TValue>> _stack = new();
        public void SetValueNoUpdate(TValue value)
        {
            _defaultValue = value;
            if (_stack.Count == 0)
                Value = _defaultValue;
        }
        [OnDespawn]
        void OnDespawn()
        {
            _stack.Clear();
            Value = _defaultValue;
        }
        public TValue Value { get; private set; }
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
        void FlushIfChanged()
        {
            PreviousValue = Value;
            Value = _stack.Count > 0 ? _stack[^1].Value : _defaultValue;
            if (!EqualityComparer<TValue>.Default.Equals(PreviousValue, Value))
                this.SetDirtyAndAutoFlushChanges();
        }
        public StackValuePushDisposable<TValue> Push(in SourcedValue<TValue> value)
        {
            _stack.Add(value);
            _stack.SortByPriority();
            FlushIfChanged();
            return new((TSelf)this, value);
        }
        public TValue Pop()
        {
            if (_stack.Count <= 0)
                return default;

            var result = _stack.RemoveLast();
            FlushIfChanged();

            return result.Value;
        }
        public bool Pop(in SourcedValue<TValue> value)
        {
            if (!_stack.Remove(value))
                return false;

            FlushIfChanged();
            return true;
        }
        public bool TryPop(out TValue value)
        {
            if (_stack.Count == 0)
            {
                value = default;
                return false;
            }

            value = Pop();
            return true;
        }
        public bool Contains(in SourcedValue<TValue> value)
            => _stack.Contains(value);
        public string CustomToString()
            => $"[{typeof(TSelf).Name}] {StringExtensions.SafeToString(Value)}";

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

        public void ForceFlushChanges()
        {
            Publisher.Publish((TSelf)this);
        }

        public IEnumerable<SourcedValue> GetTypelessSourceValues()
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

    public readonly struct SourcedValue : IPriority
    {
        public object Value { get; init; }
        public int Priority { get; init; }
        public DataSourceDesc Source { get; init; }
        public override string ToString()
            => $"{Value} ({Source})";
    }
    public readonly struct SourcedValue<TValue> : IPriority
    {
        public TValue Value { get; init; }
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