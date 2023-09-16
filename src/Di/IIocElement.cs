using System;

namespace EntityDi.Container;

internal interface IIocElement
{
	void Assert(Type contract, DiContainer container);
	object Resolve(DiContainer container);
}
internal interface IInstallElement
{
	bool ResolveOnInstall { get; }
}