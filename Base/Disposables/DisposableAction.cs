using System;
namespace BB
{
    public sealed class DisposableAction : ProtectedPooledObject<DisposableAction>
    {
        Action _action;
        public static DisposableAction GetPooled(Action action)
        {
            var result = GetPooledInternal();
            result._action = action;
            return result;
        }
        public override void Dispose()
        {
            _action?.Invoke();
            base.Dispose();
        }
    }
}
