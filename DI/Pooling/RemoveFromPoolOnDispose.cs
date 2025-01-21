//using System;
//using System.Collections.Generic;
//public sealed record RemoveFromPoolOnDispose(List<IEntity> Pool, IEntity Entity)
//	: IDisposable
//{
//	public void Dispose() => Pool.Remove(Entity);
//}