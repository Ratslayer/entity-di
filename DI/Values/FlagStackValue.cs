using BB.Di;
using System;

public abstract record FlagStackValue<TSelf, TEnum> : StackValue<TSelf, TEnum>
	where TSelf : FlagStackValue<TSelf, TEnum>
	where TEnum : Enum
{
	public bool HasFlag(TEnum e) => Value.HasFlag(e);
}