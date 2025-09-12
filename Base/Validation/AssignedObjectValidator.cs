namespace BB
{
	public sealed class AssignedObjectValidator : ProtectedPooledObject<AssignedObjectValidator>,
		IValidator
	{
		UnityEngine.Object _object;
		string _propertyName;
		public static AssignedObjectValidator GetPooled(UnityEngine.Object obj, string propertyName)
		{
			var result = GetPooledInternal();
			result._object = obj;
			result._propertyName = propertyName;
			return result;
		}

		public bool IsValid(out string message)
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
}
