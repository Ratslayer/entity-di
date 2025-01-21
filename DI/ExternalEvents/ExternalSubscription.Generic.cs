using System;
namespace BB.Di
{
	public sealed class DiExternalSubscription<TEvent> : DiExternalSubscription
	{
		Action<TEvent> _action;
		public override void Init()
		{
			_action = DiEventsUtils.CreateEventAction<TEvent>(_method, _target);
		}

		public override void Subscribe(IEntity entity)
		{
			if (_action is not null
				&& entity.Has(out IEvent<TEvent> p))
				p.Subscribe(_action);
		}

		public override void Unsubscribe(IEntity entity)
		{
			if (_action is not null
				&& entity.Has(out IEvent<TEvent> p))
				p.Unsubscribe(_action);
		}
	}
}