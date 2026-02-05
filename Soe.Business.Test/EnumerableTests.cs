using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Soe.Business.Test
{
    [TestClass]
    public class EnumerableTests
    {
		public EnumerableTests()
		{
		}

		[TestMethod]
		public void TakeReturnsAllItemsIfLessThanArgument()
		{
			int[] sequence = new int[] { 1, 2, 3 };
			var firstFive = sequence.Take(5);
			Assert.AreEqual(3, firstFive.Count());
		}
    }
}
