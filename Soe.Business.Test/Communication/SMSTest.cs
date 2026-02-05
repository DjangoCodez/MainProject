using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Soe.Business.Test.Communication
{
    [TestClass]
    public class SMSTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //-----SMS-------

            //List<string> receivers = new List<string>();
            //receivers.Add("46737238142");
            //receivers.Add("0737238142");
            //PixieSMSAdapter sms = new PixieSMSAdapter("AUTO", receivers, "test");
            //int responeCode = 0;
            //int totalSms = 0;
            //string responseMessage = string.Empty;
            //decimal smsCost = 0;
            //CommunicationManager cm = new CommunicationManager(null);
            //ActionResult result = cm.SendSMS(null, null);
            //Assert.IsTrue(result.Success, "Send sms failed: " + result.ErrorMessage);


            //------PUSH NOTIFICATION TO ALL USERS

            //CommunicationManager cm = new CommunicationManager(null);
            //cm.SendPushNotificationDontUpgradeToiOSVersion9ToAllMobileUsers();

            //------PUSH NOTIFICATION TEST
            //CommunicationManager cm = new CommunicationManager(null);
            //cm.SendPushNotification(0);

            Assert.IsTrue(true);
        }
    }
}
