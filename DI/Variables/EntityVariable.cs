using Cysharp.Threading.Tasks;
using System.Threading;

namespace BB.Di
{
	public abstract record EntityVariable<TSelf> : Variable<TSelf, Entity>
		where TSelf : Variable<TSelf, Entity>
	{
		public bool Has<T>(out T comp)
			=> Value.Has(out comp);
		public bool Has<T1, T2>(out T1 c1, out T2 c2)
			=> Value.Has(out c1, out c2);
		public static implicit operator bool(EntityVariable<TSelf> b)
			=> b.Value;
		public void RaiseEvent<T>(T msg = default)
			=> Value.Publish(msg);
		public T Get<T>() => Value.Get<T>();
		public void GetValue<T>(ref T cacheRef)
			where T : class
			=> Value.Get(ref cacheRef);
		public async UniTask WaitForEvent<T>(CancellationToken ct)
		{
			if (Has(out IEvent<T> e))
				await e.WaitForEvent(ct);
		}
	}
}