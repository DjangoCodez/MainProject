using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Util
{
    public static class AssertExt
    {
        public static void IsNotNullOrEmpty<T>(IEnumerable<T> collection, string message = null)
        {
            Assert.IsNotNull(collection, message ?? "Collection is null.");
            Assert.IsTrue(collection.Any(), message ?? "Collection is empty.");
        }
    }
}
