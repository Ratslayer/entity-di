using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

namespace BB.Di
{
	public interface IStackValue { }
	public interface IStackValue<TValue> : IStackValue
	{
		void SetDefaultValue(TValue value);
		StackValuePushDisposable<TValue> Push(TValue value, int priority = 0);
		bool Remove(TValue value, int priority = 0);
		TValue Pop();
		IEnumerable<TValue> Values { get; }
	}
	public readonly struct StackValuePushDisposable<TValue> : IDisposable
	{
		readonly IStackValue<TValue> _stack;
		readonly TValue _value;
		readonly int _priority;
		public StackValuePushDisposable(IStackValue<TValue> stack, TValue value, int priority)
		{
			_stack = stack;
			_value = value;
			_priority = priority;
		}

		public void Dispose() => _stack?.Remove(_value, _priority);
	}
}