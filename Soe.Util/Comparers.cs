using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Util
{
    public class ArrayComparer<T> : IEqualityComparer<T[]>
	{
		public bool Equals(T[] x, T[] y)
		{
			return (x == null && y == null) ||
				(x != null && y != null && x.Length == y.Length && ElementEquals(x, y));
		}

		private static bool ElementEquals(T[] x, T[] y)
		{
			for (int i = 0; i < x.Length; i++)
			{
				if (!x[i].Equals(y[i]))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(T[] obj)
		{
			return obj.Sum(x => x.GetHashCode());
		}
	}
}
