using System;
using System.Collections;
using System.Collections.Generic;
namespace BB
{
	public interface IListVariable { }
	public abstract record ListVariable<SelfType, ElementType>
		: IList<ElementType>, IAutoFlushable, IListVariable
		where SelfType : ListVariable<SelfType, ElementType>
	{
		[Inject]
		readonly IEvent<SelfType> _changePublisher;
		readonly List<ElementType> _elements = new();
		public bool AutoFlushDisabled { get; set; }
		public IEnumerator<ElementType> GetEnumerator() => _elements.GetEnumerator();
		public void Add(ElementType e)
		{
			_elements.Add(e);
			AutoFlush();
		}
		public void AddRange(IEnumerable<ElementType> e)
		{
			_elements.AddRange(e);
			AutoFlush();
		}
		public void RemoveRange(IEnumerable<ElementType> e)
		{
			_elements.RemoveRange(e);
			AutoFlush();
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
				AutoFlush();
			return result;
		}
		public ElementType this[int index] => _elements[index];
		public int Count => _elements.Count;
		[OnDespawn]
		void OnDespawn() => _elements.Clear();
		public void Clear()
		{
			_elements.Clear();
			AutoFlush();
		}
		public void SetRange(IEnumerable<ElementType> elements)
		{
			_elements.SetRange(elements);
			AutoFlush();
		}
		public IEnumerable<ElementType> Elements => _elements;

		public bool IsReadOnly => throw new NotImplementedException();

		ElementType IList<ElementType>.this[int index]
		{
			get => _elements[index];
			set => _elements[index] = value;
		}

		protected virtual void OnListUpdate()
		{
			_changePublisher.Raise(this as SelfType);
		}
		public virtual void FlushChanges() => OnListUpdate();
		public bool Contains(out ElementType e, Predicate<ElementType> predicate)
			=> _elements.TryGetValue(predicate, out e);

		public int IndexOf(ElementType item)
			=> _elements.IndexOf(item);
		public void Insert(int index, ElementType item)
		{
			_elements.Insert(index, item);
			AutoFlush();
		}

		public void RemoveAt(int index)
		{
			_elements.RemoveAt(index);
			AutoFlush();
		}
		void AutoFlush()
		{
			if (!AutoFlushDisabled)
				FlushChanges();
		}

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