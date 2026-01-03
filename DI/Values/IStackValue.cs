using System;
using System.Collections.Generic;

namespace BB.Di
{
    public interface IStackValue
    {
        IEnumerable<StackSourcedValue> GetTypelessSourceValues();
    }
    public interface IStackValue<TValue> 
        : IVariable<TValue>, IStackValue, IReadOnlyList<TValue>, IAutoFlushable, IDirtyFlushable
    {
        StackValuePushDisposable<TValue> Push(in StackSourcedValue<TValue> value);
        bool Pop(in StackSourcedValue<TValue> value);
    }
    public readonly struct StackValuePushDisposable<TValue> : IDisposable
    {
        readonly IStackValue<TValue> _stack;
        readonly StackSourcedValue<TValue> _value;
        public StackValuePushDisposable(IStackValue<TValue> stack, StackSourcedValue<TValue> value)
        {
            _stack = stack;
            _value = value;
        }

        public void Dispose() => _stack?.Pop(_value);
    }
}