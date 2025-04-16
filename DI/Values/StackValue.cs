using System;
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
		public void SetDefaultValue(TValue value)
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

		public IEnumerable<TValue> Values
			=> _stack.Select(v => v._value);

		readonly List<ValueWrapper> _stack = new();
		public bool HasValue(out TValue value)
		{
			value = Value;
			return _stack.Count > 0;
		}
		public void Update()
		{
			PreviousValue = Value;
			Value = _stack.Count > 0 ? _stack[^1]._value : _defaultValue;
			Publisher.Publish((TSelf)this);
		}
		public StackValuePushDisposable<TValue> Push(TValue value, int priority = default)
		{
			_stack.Add(new(value, priority));
			_stack.SortByPriority();
			Update();
			return new((TSelf)this, value, priority);
		}
		public TValue Pop()
		{
			if (_stack.Count <= 0)
				return default;

			var result = _stack.RemoveLast();
			Update();

			return result._value;
		}
		public bool Remove(TValue value, int priority = default)
		{
			if (!_stack.Remove(new(value, priority)))
				return false;

			Update();
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
		public bool Contains(TValue value, int priority)
			=> _stack.Contains(new(value, priority));
		public override string ToString()
			=> $"[{typeof(TSelf).Name}] {StringExtensions.SafeToString(Value)}";
		public static implicit operator
			TValue(StackValue<TSelf, TValue> state)
			=> state.Value;

		readonly struct ValueWrapper : IPriority
		{
			public readonly TValue _value;
			public readonly int _priority;
			public int Priority => _priority;
			public ValueWrapper(TValue value, int priority)
			{
				_value = value;
				_priority = priority;
			}
		}
	}
}