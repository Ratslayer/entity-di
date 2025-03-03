using System;
using System.Collections.Generic;

namespace BB.Di
{
	public interface IStackValue { }
	public interface IStackValue<TValue> : IStackValue
	{
		void SetDefaultValue(TValue value);
		StackValuePushDisposable<TValue> Push(IReadOnlyValue<TValue> value);
		void Pop(IReadOnlyValue<TValue> value);
		IEnumerable<IReadOnlyValue<TValue>> Values { get; }
	}
	public readonly struct StackValuePushDisposable<TValue> : IDisposable
	{
		readonly IStackValue<TValue> _stack;
		readonly IReadOnlyValue<TValue> _value;
		public StackValuePushDisposable(IStackValue<TValue> stack, IReadOnlyValue<TValue> value)
		{
			_stack = stack;
			_value = value;
		}

		public void Dispose() => _stack?.Pop(_value);
	}
}