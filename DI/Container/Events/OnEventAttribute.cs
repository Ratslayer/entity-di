using System;
namespace BB
{
	public sealed class OnEventAttribute : Attribute
	{
		public readonly Type[] _eventTypes;
		public OnEventAttribute(params Type[] eventTypes)
		{
			_eventTypes = eventTypes;
		}
	}
}