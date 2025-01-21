using System.Collections.Generic;
using BB.Di;
namespace BB
{
	public static class EntityListSystemExtensions
	{
		public static void List<ListType>(this IDiContainer container)
			where ListType : IListVariable
		{
			container.Construct<ListType>().Lazy().Inject();
			container.Event<ListType>();
		}
		public static void List<ListType, ElementType>(
			this IDiContainer container,
			List<ElementType> values)
			where ListType : ListVariable<ListType, ElementType>, new()
		{
			var list = new ListType();

			list.AutoFlushDisabled = true;
			list.AddRange(values);
			list.AutoFlushDisabled = false;

			container.Instance(list).Lazy().Inject();
			container.Event<ListType>();
		}
	}
}