//using System;
//namespace BB.Di
//{
//	public sealed partial class EntityImpl : IEntityLifecycle
//	{
//		~EntityImpl()
//		{
//			using var _ = Log.Logger.UseContext(this);
//			Log.Logger.Info($"Entity is being destroyed.");
//		}
//		public event Action ExternalDespawned;

//		public void FixedUpdate()
//		{
//			FixedUpdateEvent?.Invoke();
//			ProcessChild(e => e.FixedUpdate());
//		}

//		public void LateUpdate()
//		{
//			LateUpdateEvent?.Invoke();
//			ProcessChild(e => e.LateUpdate());
//		}

//		public void Update()
//		{
//			UpdateEvent?.Invoke();
//			ProcessChild(e => e.Update());
//		}
//		void ProcessChild(Action<EntityImpl> processor)
//		{
//			if (_children is null)
//				return;
//			for (var i = 0; i < _children.Count; i++)
//			{
//				var child = _children[i];
//				if (child.State == EntityState.Enabled)
//					processor(child);
//			}
//		}
//	}
//}