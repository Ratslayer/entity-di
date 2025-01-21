//using System;
//using System.Collections.Generic;
//namespace BB.Di
//{
//	public static class DiExceptionUtils
//	{
//		public static DiException Exception(this IDiContainer container, string msg)
//			=> new(GetErrorMsg(container, msg));
//		public static void Error(this IDiContainer container, string msg)
//			=> Logger.LogError(GetErrorMsg(container, msg));
//		public static bool Assert(this IDiContainer container, bool value, string msg)
//		{
//			if (!value)
//				container.Error(msg);
//			return value;
//		}
//		public static T Default<T>(this IDiContainer container, bool valid, T value, T defaultValue, string errorMsg = null)
//		{
//			if (!valid && !string.IsNullOrWhiteSpace(errorMsg))
//				container.Error(errorMsg);
//			return valid ? value : defaultValue;
//		}
//		static string GetErrorMsg(IDiContainer container, string msg)
//		{
//			var name = container is IDiContainerDetails debug ? debug.Name : "Unknown Container";
//			return $"[{name}] {msg}";
//		}
//	}
//	public sealed class DiContainer 
//		: IDiContainer, IDiContainerDetails
//	{
//		readonly Dictionary<Type, IDiStrategy> _elements = new();
//		readonly IEntityEventsBinder _eventsBinder;
//		public IEnumerable<(Type, object)> GetElements()
//		{
//			foreach (var kvp in _elements)
//				yield return (kvp.Key, kvp.Value.Resolve());
//		}
//		public IEntity Entity { get; set; }
//		public string Name { get; init; }
//		public bool Installed { get; private set; }
//		public DiContainer(
//			string name,
//			IEntityEventsBinder eventsBinder)
//		{
//			Name = name;
//			_eventsBinder = eventsBinder;
//		}
//		public void Dispose()
//		{
//			foreach ((var _, var strategy) in _elements)
//				if (strategy is IDisposable disposable)
//					disposable.Dispose();
//		}
//		public IDiStrategy BindStrategy(Type contract, IDiStrategy element)
//		{
//			if (Installed)
//				throw this.Exception($"Attempting to bind {contract.Name} after installing.");
//			element.Assert(contract, this);
//			_elements.TryAdd(contract, element);
//			return element;
//		}
//		public bool TryResolve(Type contract, out object result)
//		{
//			if (!Installed)
//				throw this.Exception($"Attempting to resolve {contract.Name} before installing");
//			//var container = this;
//			//while (container is not null)
//			//{
//			//	if (container._elements.TryGetValue(contract, out var element))
//			//	{
//			//		result = element.Resolve();
//			//		return true;
//			//	}
//			//	container = (AttachedTo as DiContainer) ?? container._parent;
//			//}
//			if (_elements.TryGetValue(contract, out var element))
//			{
//				result = element.Resolve();
//				return true;
//			}
//			result = default;
//			return false;
//		}
//		public void Install()
//		{
//			Installed = true;
//			foreach (var element in _elements.Values)
//				if (!element.Params.HasFlag(IocParams.Lazy))
//					element.Resolve(this, _eventsBinder);
//		}

//		public void BindStrategy(Type contract, IDiStrategy strategy)
//		{
//			throw new NotImplementedException();
//		}
//	}
//}
