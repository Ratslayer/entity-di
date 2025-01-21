//using System.Collections.Generic;
//using System.Text;
//using UnityEngine.Pool;
//public static class GenericPoolUtilsOld
//{
//	static readonly ObjectPool<StringBuilder> _stringBuilders = new(
//		() => new StringBuilder(),
//		actionOnRelease: sb => sb.Clear());
//	public static PooledObject<StringBuilder> GetStringBuilder(out StringBuilder sb)
//		=> _stringBuilders.Get(out sb);
//	public static PooledObject<List<T>> GetList<T>(out List<T> list)
//		=> CollectionPool<List<T>, T>.Get(out list);
//}
