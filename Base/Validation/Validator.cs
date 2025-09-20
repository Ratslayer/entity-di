using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BB
{
    public sealed class Validator : ProtectedPooledObject<Validator>
    {
        readonly List<IValidator> _validators = new();
        Entity _entity;
        BaseScriptableObject _asset;
        string _fileName;
        public static Validator GetPooled([CallerFilePath] string fileName = "")
        {
            var result = GetPooledInternal();
            result._fileName = fileName;
            return result;
        }
        public override void Dispose()
        {
            _entity = default;
            _validators.DisposeAndClear();
            base.Dispose();
        }
        public bool ValidateAndDispose()
        {
            var sb = PooledStringBuilder.GetPooled();
            var result = true;
            foreach (var validator in _validators)
            {
                var isValid = validator.IsValid(out var message);
                if (isValid)
                    continue;
                result = false;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (sb.Empty)
                    {
                        if (_entity)
                            sb.AppendLine($"Validation error in Entity {_entity}");
                        else if (_asset)
                            sb.AppendLine($"Validation error in Asset {_asset.name}");
                        else sb.AppendLine($"Validation error");
                        sb.AppendLine($"File {_fileName}:");
                    }
                    sb.AppendLine(message);
                }
            }
            if (!result)
                Log.Error(sb.ToString());
            sb.Dispose();
            return result;
        }
        public Validator WithEntity(Entity entity)
        {
            _entity = entity;
            return this;
        }
        public Validator WithAsset(BaseScriptableObject asset)
        {
            _asset = asset;
            return this;
        }
        public Validator IsAssigned(UnityEngine.Object obj, string propertyName)
        {
            _validators.Add(AssignedObjectValidator.GetPooled(obj, propertyName));
            return this;
        }
        public Validator NotEmpty(string value, string propertyName)
        {
            _validators.Add(NotEmptyStringValidator.GetPooled(value, propertyName));
            return this;
        }
    }
}