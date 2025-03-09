using System;
namespace BB
{
	public sealed class OnEventAttachedAttribute : Attribute
	{
		public readonly Type[] _eventTypes;
		public OnEventAttachedAttribute(params Type[] eventTypes)
		{
			_eventTypes = eventTypes;
		}
	}
}