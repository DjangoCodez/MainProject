using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class CommunicationManagerTests
    {
        [TestMethod()]
        public void SendPayrollToSftpTest()
        {
            CommunicationManager cm = new CommunicationManager(null);
            ActionResult result = cm.SendPayrollToSftp(7, 75);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SendMessagePushNotificationSystemDown()
        {
            CommunicationManager cm = new CommunicationManager(null);
            List<string> filteredIds = new List<string>();

            //List<string> allIds = File.ReadAllLines(@"c:\temp\userids.txt").ToList();
            //List<string> validAllGuids = File.ReadAllLines(@"c:\temp\guids.txt").ToList();
            //List<string> validTodayGuids = File.ReadAllLines(@"c:\temp\useridstoday.txt").ToList();



            //foreach (var validGuid in validTodayGuids)
            //{
            //    string validId = allIds.Where(x => x.ToLower().Contains(validGuid.ToLower())).FirstOrDefault();
            //    if (validId != null)
            //        filteredIds.Add(validId);
            //}

            //int count = filteredIds.Count();

            cm.SendMessagePushNotificationSystemDown(filteredIds);
            Assert.IsTrue(true);
        }
    }
}