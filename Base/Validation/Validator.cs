using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BB
{
	public sealed class Validator : ProtectedPooledObject<Validator>
	{
		readonly List<IValidator> _validators = new();
		Entity _entity;
		string _fileName;
		public static Validator GetPooled(
			Entity entity,
			[CallerFilePath] string fileName = "")
		{
			var result = GetPooledInternal();
			result._entity = entity;
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
						sb.AppendLine($"Validation error in Entity {_entity}");
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
		public Validator IsAssigned(UnityEngine.Object obj, string propertyName)
		{
			_validators.Add(AssignedObjectValidator.GetPooled(obj, propertyName));
			return this;
		}
	}
}