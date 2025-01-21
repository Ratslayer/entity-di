using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class InjectAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class InjectFromParentAttribute : Attribute { }
