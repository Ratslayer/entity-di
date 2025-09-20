namespace BB
{
    public abstract class PooledValidator<TSelf, TValue> : ProtectedPooledObject<TSelf>, IValidator
        where TSelf : PooledValidator<TSelf, TValue>, new()
    {
        protected TValue _object;
        protected string _propertyName;
        public static TSelf GetPooled(TValue obj, string propertyName)
        {
            var result = GetPooledInternal();
            result._object = obj;
            result._propertyName = propertyName;
            return result;
        }
        public abstract bool IsValid(out string message);
    }
    public sealed class AssignedObjectValidator
        : PooledValidator<AssignedObjectValidator, UnityEngine.Object>
    {
        public override bool IsValid(out string message)
        {
            if (_object)
            {
                message = "";
                return true;
            }
            message = $"{_propertyName} has not been assigned";
            return false;
        }
    }
    public sealed class NotEmptyStringValidator
        : PooledValidator<NotEmptyStringValidator, string>
    {
        public override bool IsValid(out string message)
        {
            if (!string.IsNullOrWhiteSpace(_object))
            {
                message = "";
                return true;
            }
            message = $"{_propertyName} is null or whitespace";
            return false;
        }
    }
}
