using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class EmailManagerTests
    {
        [TestMethod()]
        public void SendEmailViaCommunicatorTest()
        {
            EmailManager em = new EmailManager(null);
            em.SendEmailViaCommunicator("rickard.dahlgren@softone.se", "rickard.karlsson@softone.se", new List<string>(), "Test via Communicator", "Test body", false, null);
            Assert.IsTrue(true);
        }
    }
}