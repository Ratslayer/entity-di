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
	public abstract record Variable<TValue> : IVariable<TValue>
	{
        [OnDespawn]
        void Reset() => _value = PreviousValue = default;
        TValue _value;
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
            PublishUpdate();
        }
		protected abstract void PublishUpdate();
        public override string ToString()
            => StringExtensions.SafeToString(_value);
        public static implicit operator TValue(Variable<TValue> s)
            => s is not null ? s.Value : default;
    }

    public abstract record Variable<TSelf, TValue> : Variable<TValue>
		where TSelf : Variable<TSelf, TValue>
	{
		[Inject]
		public readonly IEvent<TSelf> _event;
		protected override void PublishUpdate()
			=> _event.Publish((TSelf)this);
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