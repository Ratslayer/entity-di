using System.Collections.Generic;
using BB.Di;
namespace BB
{
    public static class EntityListSystemExtensions
    {
        public static void List<ListType>(this IDiContainer container)
            where ListType : IListVariable, new()
        {
            container.System<ListType>();
            container.Event<ListType>();
        }
    }
}