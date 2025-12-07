//using System;
//using System.Reflection;
//namespace BB.Di
//{
//	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
//	public sealed class InjectFromAttachAttribute : Attribute { }
//	public abstract class AbstractEntityValueAttachment : IEntitySubscription
//	{
//		protected abstract Type Type { get; }
//		protected abstract void Set(object target, object value);
//		public object _target;

//		public void Subscribe(IEntity entity)
//		{
//			if (!entity.TryResolve(Type, out var value))
//			{
//				Log.Logger.Error($"[InjectOnAttach] {entity} could not resolve {Type.Name}");
//				value = null;
//			}
//			Set(_target, value);
//		}

//		public void Unsubscribe(IEntity entity)
//		{
//			Set(_target, null);
//		}

//	}
//	public sealed class EntityFieldAttachment : AbstractEntityValueAttachment
//	{
//		public FieldInfo _info;

//		protected override Type Type => _info.FieldType;

//		protected override void Set(object target, object value)
//			=> _info.SetValue(target, value);
//	}
//	public sealed class EntityPropertyAttachment : AbstractEntityValueAttachment
//	{
//		public PropertyInfo _info;

//		protected override Type Type => _info.PropertyType;

//		protected override void Set(object target, object value)
//			=> _info.SetValue(target, value);
//	}
//}