using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftOne.Soe.Business.Core.SoftOneId.Tests
{
    [TestClass()]
    public class SoftOneIdConnectorTests
    {
        [TestMethod()]
        public void UpdateDomainTest()
        {
            SoftOneIdConnector.UpdateDomain(Guid.NewGuid(), 1, 1, "test", Guid.NewGuid());
            Assert.IsTrue(true);
        }
    }
}