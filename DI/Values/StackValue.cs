using System;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
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

		public IEnumerable<IReadOnlyValue<TValue>> Values => _stack;

		readonly List<IReadOnlyValue<TValue>> _stack = new();
		public bool HasValue(out TValue value)
		{
			value = Value;
			return _stack.Count > 0;
		}
		public void Update()
		{
			var value = _stack.Count > 0 ? _stack[^1] : default;
			PreviousValue = Value;
			Value = value is null ? _defaultValue : value.Value;
			Publisher.Raise((TSelf)this);
		}
		public StackValuePushDisposable<TValue> Push(IReadOnlyValue<TValue> value)
		{
			_stack.Add(value);
			_stack.SortByPriority();
			Update();
			return new((TSelf)this, value);
		}
		public TValue Pop()
		{
			if (_stack.Count <= 0)
				return default;

			var result = _stack.RemoveLast();
			Update();

			return result.Value;
		}
		public void Pop(IReadOnlyValue<TValue> value)
		{
			if (!_stack.Remove(value))
				return;
			Update();
		}
		public void PushUniqueOrUpdate(IReadOnlyValue<TValue> value)
		{
			if (!_stack.Contains(value))
				Push(value);
			else Update();
		}
		public void SetUniqueOrUpdate(IReadOnlyValue<TValue> value, bool push)
		{
			if (push)
				PushUniqueOrUpdate(value);
			else Pop(value);
		}
		public bool TryPop(IReadOnlyValue<TValue> value)
		{
			if (!_stack.Contains(value))
				return false;
			Pop(value);
			return true;
		}
		public override string ToString()
			=> $"[{typeof(TSelf).Name}] {StringExtensions.SafeToString(Value)}";
		public static implicit operator
			TValue(StackValue<TSelf, TValue> state)
			=> state.Value;
	}
	public static class StackValueExtensions
	{
		public static StackValuePushDisposable<T> Push<T>(this IStackValue<T> stack, T value)
		{
			var val = new Val<T>(value);
			return stack.Push(val);
		}
		public static void Pop<T>(this IStackValue<T> stack, Predicate<T> predicate)
		{
			if (stack.Values.TryGetValue(
				x => predicate(x.Value),
				out var v))
				stack.Pop(v);
		}
		public static void Pop<T>(this IStackValue<T> stack, T value)
		{
			if (stack.Values.TryGetValue(
				x => EqualityComparer<T>.Default.Equals(x.Value, value),
				out var v))
				stack.Pop(v);
		}
	}
}