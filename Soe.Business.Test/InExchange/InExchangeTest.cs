using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Newtonsoft.Json.Linq;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Business.Util.API.Models;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.InExchange;
using SoftOne.Soe.Data;

namespace Soe.Business.Test.InExchange
{
    [TestClass]
    public class InExchangeTest
    {
        public ParameterObject GetParamsObject(int userId = 72, int companyId = 7, int roleId = 0)
        {
            //Defaults are Anders Svensson, Hantverkardemo (demo db)
            Company company = new CompanyManager(null).GetCompany(companyId, true);
            User user = new UserManager(null).GetUser(userId);

            return ParameterObject.Create(company: company.ToCompanyDTO(),
                                          user: user.ToDTO(),
                                          roleId: roleId,
                                          thread: "test");
        }

        [TestMethod]
        public void TestBuyerLookupTest()
        {
            int actorCompanyId = 2721622;
            bool releaseMode = true;
            string token = InExchangeConnector.GetToken(actorCompanyId, releaseMode);
            //var response = InExchangeConnector.GetBuyerCompany(token, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup {GLN= "7365560999205", PartyId = actorCompanyId.ToString() });
            //var result = InExchangeConnector.GetInexchangeBuyerReciverCompanyId(token, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { OrgNo = "5566956768", PartyId = actorCompanyId.ToString() });
            var result = InExchangeConnector.GetInexchangeBuyerReciverCompanyId(token, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { OrgNo = "2021002817", PartyId = actorCompanyId.ToString() });
            
            //var result = InExchangeConnector.GetInexchangeBuyerReciverCompanyId(token, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { OrgNo = "5566956768", PartyId = actorCompanyId.ToString() });
            //var companyId = InExchangeConnector.GetInexchangeBuyerReciverCompanyId(token, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { GLN = "7381036454528", PartyId = actorCompanyId.ToString() });

            //var response = InExchangeConnector.GetBuyerCompany(token, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { Name = "Tim",CountryCode="SE", PartyId = actorCompanyId.ToString() });
            InExchangeConnector.RevokeTokenForCustomer(token, true);

            Assert.IsTrue(!string.IsNullOrEmpty(result.StringValue));
        }

        [TestMethod]
        public void TestBulkGetStatusFromSentDocument()
        {
            int actorCompanyId = 7;
            DateTime lastChecked = DateTime.Today.AddDays(-60);
            bool releaseMode = false;
            var result = InExchangeConnector.GetStatusList(actorCompanyId, releaseMode, lastChecked);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGetAndUpdateFunction()
        {
            var idm = new InvoiceDistributionManager(null);
            bool releaseMode = false;
            var result = idm.GetAndUpdateStatusesFromInExchange(releaseMode);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestRegisterCompany()
        {
            int actorCompanyId = 7;
            bool releaseMode = false;
            var companyDto = new CompanyDTO { OrgNr = "575757-5757", VatNr = "SE575757-5757", Name = "Test företag", SysCountryId = 1 };
            var result = InExchangeConnector.RegisterCompanyToInExchangeAPI(actorCompanyId, companyDto,"Testgatan 1" ,"111 11","Stockholm", "XE_NoReply@softone.se", true, false,releaseMode);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestFixFileName()
        {
            var result = InExchangeConnector.FixFileName(@"c:\kalle\ÅsaOchÖrjan.txt");
            result = InExchangeConnector.FixFileName(@"c:\kalle\large text(paranteser)text,.txt");
            result = InExchangeConnector.FixFileName(@"c:\kalle\large text@text.txt");
            result = InExchangeConnector.FixFileName(@"c:\kalle\large text@text/200.txt");
            Assert.IsTrue(result != null);
        }

        #region IncomingDocuments

        [TestMethod]
        public void TestGetIncomingDocumentsMethod()
        {
            int actorCompanyId = 7;
            var electronicInvoice = new ElectronicInvoiceMananger(null);
            var result = electronicInvoice.AddInexchangeInvoices(actorCompanyId, 72);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestGetIncomingDocumentsImport()
        {
            int actorCompanyId = 7;            
            var documents = InExchangeConnector.GetIncomingDocuments(actorCompanyId, false);
            var import = new ImportExportManager(null);
            var supplierInvoiceIOItem = new SupplierInvoiceIOItem();
            foreach (var doc in documents)
            {
                var invoice = doc.CreateSupplierInvoiceIO(true);
                supplierInvoiceIOItem.supplierInvoices.Add(invoice);
            }            
            var result = import.ImportSupplierInvoiceIO(supplierInvoiceIOItem, TermGroup_IOImportHeadType.SupplierInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.Inexchange, actorCompanyId, true);
            Assert.IsTrue(result.Success);
        }

        
        [TestMethod]
        public void TestGetIncomingDocumentFromFile()
        {
            var param = GetParamsObject();
            var import = new ImportExportManager(param);
            var supplierInvoiceIOItem = new SupplierInvoiceIOItem();
            var document = new InExchangeApiIncomingDocument();
            document.filedata = new MemoryStream( File.ReadAllBytes(@"C:\Temp\Inexchange\incoming\74152_exempel.xml") );
            var invoice = document.CreateSupplierInvoiceIO(true);
            supplierInvoiceIOItem.supplierInvoices.Add(invoice);
            var result = import.ImportSupplierInvoiceIO(supplierInvoiceIOItem, TermGroup_IOImportHeadType.SupplierInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.Inexchange, param.ActorCompanyId, true);
            Assert.IsTrue(result.Success);
        }

        #endregion

        #region Parser

        [TestMethod]
        public void ParseURLForGuid()
        {
            string pathUrl = "https://test.inexchange.se:80/inexchange.webapi/api/documents/outbound/357306d0-bda5-47b5-99ff-25b82bda5062";
            var result = Path.GetFileName(new Uri(pathUrl).AbsolutePath);
            Assert.IsNotNull(result);
        }

        #endregion

        #region Poller       

        [TestMethod]
        public void TestGetInexchangeEntry()
        {
            var idm = new InvoiceDistributionManager(null);
            var entry = idm.GetActiveEntry(11530, TermGroup_EDistributionType.Inexchange);
            Assert.IsTrue(entry != null);
        }

        #endregion

        #region Help Methods

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        #endregion
    }
}
