using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ActorManagerTests
    {
        [TestMethod()]
        public void TryChangingGuidTest()
        {
            ActorManager am = new ActorManager(null);
            am.TryChangingGuid("522231304650344", "101", "rickard.ah.karlsson@gmail.com", Guid.Parse("b643e093-6307-4312-8d0d-1ff397d786e4"));

            Assert.Fail();
        }
    }
}