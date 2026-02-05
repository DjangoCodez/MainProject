using Banker.Shared.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Banker;
using System.IO;

namespace SoftOne.Soe.Business.Util.API.Tests
{
    [TestClass()]
    public class BankerConnectorTests
    {
        [TestMethod()]
        public void UploadFile()
        {
            var actorCompanyId = 7;
            var sysCompDBId = 1;
            var cm = new CountryCurrencyManager(null);
            var sm = new SettingManager(null);
            var pm = new PaymentManager(null);
            var paymentMethod = pm.GetPaymentMethod(1, actorCompanyId);
            var sysBank = cm.GetSysBank("HANDSESS");
            var file = File.ReadAllBytes("c:\\temp\\Avalo\\ISO20022_fileupload.xml");
            var result = Core.Banker.BankerConnector.UploadPaymentFile(sm, paymentMethod, actorCompanyId,"111", sysCompDBId, sysBank,"softone-1", file);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetOnboardingFiles()
        {
            var sm = new SettingManager(null);
            var result = Core.Banker.BankerConnector.GetOnboardingRequests(sm);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetDownloadedFiles()
        {
            var sm = new SettingManager(null);
            var result = Core.Banker.BankerConnector.GetDownloadRequest(sm, new Common.DTO.SoeBankerRequestFilterDTO {MaterialType = (int)MaterialType.FinvoiceDownload });
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetDownloadedFeedbackFiles()
        {
            var sm = new SettingManager(null);
            var bankConnector = new BankerConnector();
            var result = bankConnector.DownloadPaymentFeedback(sm, new Common.DTO.SysBankDTO {SysCountryId = 1, BIC = "HANDSESS", SysBankId = 1 } );
            Assert.IsTrue(result != null);
        }
    }
}