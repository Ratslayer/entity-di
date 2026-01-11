using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
namespace BB
{
	public interface IListVariable { }
	public abstract class ListVariable<SelfType, ElementType>
		: IList<ElementType>,
		IReadOnlyList<ElementType>,
		IAutoFlushable,
		IListVariable,
		IDirtyFlushable
		where SelfType : ListVariable<SelfType, ElementType>
	{
		[Inject]
		readonly IEvent<SelfType> _changePublisher;
		readonly List<ElementType> _elements = new();
		public bool AutoFlushDisabled { get; set; }
		public bool IsDirty { get; set; }
		public IEnumerator<ElementType> GetEnumerator() => _elements.GetEnumerator();
		public void Add(ElementType e)
		{
			_elements.Add(e);
			OnAdd(e);
			this.SetDirtyAndAutoFlushChanges();
		}

		public void AddRange(IEnumerable<ElementType> range)
		{
			_elements.AddRange(range);
			foreach (var e in range)
				OnAdd(e);
			this.SetDirtyAndAutoFlushChanges();
		}
		public void RemoveRange(IEnumerable<ElementType> range)
		{
			foreach (var e in range)
				if (_elements.Remove(e))
					OnRemove(e);
			this.SetDirtyAndAutoFlushChanges();
		}
		public void RemoveDisposeAndFlush(Func<ElementType, bool> predicate)
		{
			using var _ = this.FlushOnDispose();
			foreach (var i in -Count)
			{
				var element = this[i];
				if (!predicate(element))
					continue;
				if (element is IDisposable disposable)
					disposable.Dispose();
				RemoveAt(i);
			}
		}
		public bool Remove(ElementType e)
		{
			var result = _elements.Remove(e);
			if (result)
			{
				OnRemove(e);
				this.SetDirtyAndAutoFlushChanges();
			}
			return result;
		}
		public ElementType this[int index] => _elements[index];
		public int Count => _elements.Count;
		[OnEvent(typeof(EntityDespawnedEvent))]
		void OnDespawn() => Clear();
		public void Clear()
		{
			foreach(var e in _elements)
				OnRemove(e);
			_elements.Clear();
			this.SetDirtyAndAutoFlushChanges();
		}
		public void SetRange(IEnumerable<ElementType> elements)
		{
			using var _ = this.FlushOnDispose();
			Clear();
			if (elements is not null)
				AddRange(elements);
		}
		public IEnumerable<ElementType> Elements => _elements;

		public bool IsReadOnly => false;

		ElementType IList<ElementType>.this[int index]
		{
			get => _elements[index];
			set => _elements[index] = value;
		}

		protected virtual void OnListUpdate()
		{
			_changePublisher.Publish(this as SelfType);
		}
		public virtual void ForceFlushChanges()
		{
			using var _ = this.DisableAutoFlush();
			OnListUpdate();
		}
		public bool Contains(out ElementType e, Func<ElementType,bool> predicate)
			=> _elements.TryGetValue(predicate, out e);

		public int IndexOf(ElementType item)
			=> _elements.IndexOf(item);
		public void Insert(int index, ElementType item)
		{
			_elements.Insert(index, item);
			this.SetDirtyAndAutoFlushChanges();
		}

		public void RemoveAt(int index)
		{
			_elements.RemoveAt(index);
			this.SetDirtyAndAutoFlushChanges();
		}

		protected virtual void OnRemove(ElementType e) { }
		protected virtual void OnAdd(ElementType e) { }
		public bool Contains(ElementType item)
			=> _elements.Contains(item);

		public void CopyTo(ElementType[] array, int arrayIndex)
			=> _elements.CopyTo(array, arrayIndex);

		IEnumerator IEnumerable.GetEnumerator()
			=> _elements.GetEnumerator();
		public void Sort(Comparison<ElementType> comparer)
			=> _elements.Sort(comparer);
	}
}