using System;

public sealed class OnEventAttribute : Attribute
{
	public readonly Type[] _eventTypes;
	public OnEventAttribute(params Type[] eventTypes)
	{
		_eventTypes = eventTypes;
	}
}
public sealed class OnEventAttachedAttribute : Attribute 
{
	public readonly Type[] _eventTypes;
	public OnEventAttachedAttribute(params Type[] eventTypes)
	{
		_eventTypes = eventTypes;
	}
}