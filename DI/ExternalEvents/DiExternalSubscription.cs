using System;
using System.Reflection;
namespace BB.Di
{
	public abstract class DiExternalSubscription : IExternalSubscription
	{
		public MethodInfo _method;
		public object _target;
		public abstract void Init();
		public abstract void Subscribe(IEntity entity);
		public abstract void Unsubscribe(IEntity entity);
	}
	public sealed record OnEventExternalSubscription<TEvent>(Action<TEvent> Action)
		: IExternalSubscription
	{
		public void Subscribe(IEntity entity)
		{
			if (entity.Has(out IEvent<TEvent> e))
				e.Subscribe(Action);
		}

		public void Unsubscribe(IEntity entity)
		{
			if (entity.Has(out IEvent<TEvent> e))
				e.Unsubscribe(Action);
		}
	}
}