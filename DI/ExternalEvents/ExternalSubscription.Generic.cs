using System;
namespace BB.Di
{
	public sealed class DiExternalSubscription<TEvent> : DiExternalSubscription
	{
		Action<TEvent> _action;
		IEntity _entity;
		public override void Init()
		{
			_action = DiEventsUtils.CreateAction<TEvent>(_method, _target, _entity);
		}

		public override void Subscribe(IEntity entity)
		{
			if (_action is not null
				&& entity.Has(out IEvent<TEvent> p))
			{
				_entity = entity;
				p.Subscribe(_action);
			}
		}

		public override void Unsubscribe(IEntity entity)
		{
			if (_action is not null
				&& entity.Has(out IEvent<TEvent> p))
			{
				p.Unsubscribe(_action);
				_entity = null;
			}
		}
	}
}