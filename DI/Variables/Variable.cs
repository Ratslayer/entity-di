using System.Collections.Generic;
namespace BB
{
    public interface IVariable { }
    public interface IVariable<T> : IVariable
    {
        T Value { get; }
        T PreviousValue { get; }
        void SetValueNoUpdate(T value);
    }
    public sealed class InitialVariableValue
    {
        public object Value { get; init; }
    }
    public abstract class Variable<TValue> : IVariable<TValue>
    {
        [Inject] InitialVariableValue _startingValue;
        TValue _value;
        [OnEvent]
        void InitValue(EntitySpawnedEvent _)
        {
            if (_startingValue.Value is null)
                return;
            Value = (TValue)_startingValue.Value;
        }

        [OnEvent]
        void ClearValue(EntityDespawnedEvent _) => _value = PreviousValue = default;
        public TValue Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<TValue>.Default.Equals(_value, value))
                    SetValueAndUpdate(value);
            }
        }
        public TValue PreviousValue { get; private set; }
        public void SetValueNoUpdate(TValue value) => _value = value;
        public void SetValueAndUpdate(TValue value)
        {
            PreviousValue = _value;
            _value = value;
            OnUpdate();
        }
        protected abstract void OnUpdate();
        public override string ToString()
            => StringExtensions.SafeToString(_value);
        public static implicit operator TValue(Variable<TValue> s)
            => s is not null ? s.Value : default;
    }

    public abstract class Variable<TSelf, TValue> : Variable<TValue>
        where TSelf : Variable<TSelf, TValue>
    {
        [Inject] IEvent<TSelf> _event;
        protected override void OnUpdate()
            => _event.Publish((TSelf)this);
        public override string ToString()
           => $"{typeof(TSelf).Name}: {StringExtensions.SafeToString(Value)}";
    }
    public static class VariableUtils
    {
        public static bool Toggle<TVar>(this TVar v)
            where TVar : Variable<TVar, bool>
        {
            v.Value = !v.Value;
            return v.Value;
        }
        public static TValue Var<TVar, TValue>(this Entity entity, TValue defaultValue = default)
            where TVar : Variable<TVar, TValue>
        {
            if (entity.Has(out TVar variable))
                return variable.Value;
            return defaultValue;
        }
    }
}