using System;
using System.Collections.Generic;

namespace BB.Di
{
	public interface IStackValue { }
	public interface IStackValue<TValue> : IStackValue, IReadOnlyList<TValue>, IAutoFlushable, IDirtyFlushable
	{
		void SetDefaultValue(TValue value);
		StackValuePushDisposable<TValue> Push(TValue value, int priority = 0);
		bool Pop(TValue value, int priority = 0);
		TValue Pop();
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

		public void Dispose() => _stack?.Pop(_value, _priority);
	}
}