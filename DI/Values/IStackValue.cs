using System;
using System.Collections.Generic;

namespace BB.Di
{
    public interface IStackValue { }
    public interface IStackValue<TValue> : IVariable<TValue>, IStackValue, IReadOnlyList<TValue>, IAutoFlushable, IDirtyFlushable
    {
        StackValuePushDisposable<TValue> Push(in ValueWrapper<TValue> value);
        bool Pop(in ValueWrapper<TValue> value);
        TValue Pop();
    }
    public readonly struct StackValuePushDisposable<TValue> : IDisposable
    {
        readonly IStackValue<TValue> _stack;
        readonly ValueWrapper<TValue> _value;
        public StackValuePushDisposable(IStackValue<TValue> stack, ValueWrapper<TValue> value)
        {
            _stack = stack;
            _value = value;
        }

        public void Dispose() => _stack?.Pop(_value);
    }
}