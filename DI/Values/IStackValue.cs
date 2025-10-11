using System;
using System.Collections.Generic;

namespace BB.Di
{
    public interface IStackValue
    {
        IEnumerable<SourcedValue> GetTypelessSourceValues();
        string CustomToString();
    }
    public interface IStackValue<TValue> : IVariable<TValue>, IStackValue, IReadOnlyList<TValue>, IAutoFlushable, IDirtyFlushable
    {
        StackValuePushDisposable<TValue> Push(in SourcedValue<TValue> value);
        bool Pop(in SourcedValue<TValue> value);
        TValue Pop();
    }
    public readonly struct StackValuePushDisposable<TValue> : IDisposable
    {
        readonly IStackValue<TValue> _stack;
        readonly SourcedValue<TValue> _value;
        public StackValuePushDisposable(IStackValue<TValue> stack, SourcedValue<TValue> value)
        {
            _stack = stack;
            _value = value;
        }

        public void Dispose() => _stack?.Pop(_value);
    }
}