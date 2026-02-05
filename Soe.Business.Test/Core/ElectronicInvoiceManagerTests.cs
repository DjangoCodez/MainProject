using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.InExchange;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ElectronicInvoiceManagerTests
    {
        [TestMethod()]
        public void CreateEInvoice()
        {
            var actorCompanyId = 56992;
            var userId = 12582;
            var eim = new ElectronicInvoiceMananger(GetParameterObject(actorCompanyId, userId));
            
            var result = eim.CreateEInvoice(actorCompanyId, userId, 6892485, false);

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CreatePeppolSvefakturaFile()
        {
            var actorCompanyId = 56992;
            var userId = 12582;
            var eim = new ElectronicInvoiceMananger(GetParameterObject(actorCompanyId, userId));
            var info = new InExchangeApiSendInfo();
            var result = eim.CreatePeppolSveFaktura(actorCompanyId, userId, 6892485, String.Empty, null, info, SoftOne.Soe.Common.Util.TermGroup_EInvoiceFormat.SvefakturaAPI, out byte[] data);
            File.WriteAllBytes(@"c:\Temp\inexchange\createinvoicetest.xml", data);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CreateFinvoice()
        {
            var eim = new ElectronicInvoiceMananger(GetParameterObject(288, 190));
            List<FileDataItem> outFiles;
            var result = eim.CreateFinvoice(288, 24804, true, out outFiles );
            //var result = eim.SendFinVoiceToInexchange(new List<int>() { 25281, }, 190, 288);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void UpdateFinvoiceFromFeedback()
        {
            var im = new InvoiceDistributionManager(GetParameterObject(288, 190));
            var fileContent = File.ReadAllText(@"C:\Temp\finvoice\Finvoice 181361 Acknowlegde.xml");

            var result = im.UpdateFinvoiceFromFeedback(288, fileContent);

            //var result = eim.SendFinVoiceToInexchange(new List<int>() { 25281, }, 190, 288);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CreateIntrum()
        {
            var eim = new ElectronicInvoiceMananger(GetParameterObject(7, 72));
            var result = eim.CreateIntrumInvoice(new List<int> { 30884 },7, false);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CreateFortnox()
        {
            var eim = new ElectronicInvoiceMananger(GetParameterObject(7, 72));
            var result = eim.CreateFortnoxInvoices(new List<int> { 33887 }, 7);
            Assert.IsTrue(result.Success);
        }

        private ParameterObject GetParameterObject(int actorCompanyId, int userId)
        {
            Company company = null;
            if (actorCompanyId > 0)
                company = new CompanyManager(null).GetCompany(actorCompanyId);

            User user = null;
            if (userId > 0)
                user = new UserManager(null).GetUser(userId);
            else
                user = new User() { LoginName = this.ToString() };

            return ParameterObject.Create(company: company.ToCompanyDTO(),
                                          user: user.ToDTO(),
                                          thread: "test");
        }
    }
}