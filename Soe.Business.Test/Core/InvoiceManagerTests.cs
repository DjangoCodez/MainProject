using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class InvoiceManagerTests: TestBase
    {
        [TestMethod()]
        public void GetCustomerInvoicesFromSelectionTest()
        {
            InvoiceManager m = new InvoiceManager(null);
            EvaluatedSelection es = new EvaluatedSelection();
            es.SB_InvoiceNrFrom = "170300";
            es.SB_InvoiceNrTo = "170320";
            es.SB_HasInvoiceNrInterval = true;
            es.ActorCompanyId = 305182;
            SoeOriginType type = SoeOriginType.Order;
            List<CustomerInvoice> orders;
            using (CompEntities entities = new CompEntities())
            {
                orders = m.GetCustomerInvoicesFromSelection(entities, es, true, ref type, false);
            }
            Assert.IsTrue(orders != null);
        }

        [TestMethod()]
        public void GetCustomerInvoicesForAnalysisTest()
        {
            InvoiceManager m = new InvoiceManager(GetParameterObject(7,72));
            var es = new Dictionary<string, object>();
            es.Add("invoiceDateFrom", DateTime.Today.AddDays(-30));
            es.Add("invoiceDateTo", DateTime.Today);
            es.Add("showOpen", true);
            es.Add("showClosed", false);
            es.Add("onlyMine", false);
            using (CompEntities entities = new CompEntities())
            {
                var orders = m.GetCustomerInvoicesForAnalys(entities, SoeOriginType.Order, 7, es, true, true);
                //var orders = m.GetCustomerInvoicesForAnalys(entities, SoeOriginType.CustomerInvoice, 7, es, true);
                Assert.IsTrue(orders != null);
            }
            
        }


        [TestMethod()]
        public void GetCustomerInvoicesGridTest()
        {
            InvoiceManager m = new InvoiceManager(null);
            using (CompEntities entities = new CompEntities())
            {
                var item2 = m.GetCustomerInvoiceForGrid(entities, 23129, 7);
                Assert.IsTrue(item2 != null);
            }
        }

        [TestMethod()]
        public void TransferOrderToInvoiceTest()
        {
            InvoiceManager m = new InvoiceManager(GetParameterObject(18312, 1158, 206));

            using (CompEntities entities = new CompEntities())
            {
                var invoices = m.GetCustomerInvoicesForGrid(SoeOriginStatusClassification.OrdersAll,(int)SoeOriginType.Order,18312,0, true, true, false,false, null, true);

                invoices = invoices.Where(x => x.CustomerInvoiceId == 375409).ToList();
                var result = m.TransferCustomerInvoices(invoices, SoeOriginStatusChange.Billing_OrderToInvoice, 0, false, 0, null, null, null, null, false, true, false);
                Assert.IsTrue(result.Success);
            }
        }

        [TestMethod()]
        public void GetSuppliersCustomersFromUnpaidInvoicesTest()
        {
            InvoiceManager m = new InvoiceManager(null);
            var item = m.GetSuppliersCustomersFromUnpaidInvoices(SoeOriginType.CustomerInvoice, 7);
            Assert.IsTrue(item != null);

        }

        [TestMethod()]
        public void GetCustomerInvoice()
        {
            InvoiceManager m = new InvoiceManager(GetParameterObject(7));
            var item = m.GetCustomerInvoiceSmall(8837);
            Assert.IsTrue(item != null);
        }

        [TestMethod()]
        public void ValidateOCR()
        {
            bool result = InvoiceUtility.ValidateSwedishOCRNumber("6200817968431");
            if (result)
                result = InvoiceUtility.ValidateSwedishOCRNumber("21951395710");
            if (result)
                result = InvoiceUtility.ValidateSwedishOCRNumber("0498609100132");
            if (result)
                result = InvoiceUtility.ValidateSwedishOCRNumber("45120482");
            if (result)
                result = !InvoiceUtility.ValidateSwedishOCRNumber("454564654564"); //false
            if (result)
                result = !InvoiceUtility.ValidateSwedishOCRNumber("85456454"); //false
            if (result)
                result = InvoiceUtility.ValidateSwedishOCRNumber("1234475");
            if (result)
                result = InvoiceUtility.ValidateSwedishOCRNumber("220609036");
            Assert.IsTrue(result);
        }


        [TestMethod()]
        public void GetInvoicesSuperOfficeStyle()
        {
            var im = new InvoiceManager(GetParameterObject());
            var invoices = im.GetChangedCustomerInvoices(SoeOriginStatusClassification.ContractsAll, (int)SoeOriginType.Contract, 18312, 0, true, true, false, true, null, true, 0, forceHasPermissions: true);

            Assert.IsTrue(true);
        }


        [TestMethod()]
        public void GetOrderTest()
        {
            InvoiceManager m = new InvoiceManager(null);
            {
                var orders = m.GetOrder(39757, true, true, 7);
                Assert.IsTrue(orders != null);
            }

        }

        [TestMethod()]
        public void GetDeleteOrderTest()
        {
            var actorCompanyId = 904067;
            InvoiceManager m = new InvoiceManager(GetParameterObject(actorCompanyId, 0,0));
            var result = m.DeleteInvoice(20306567,actorCompanyId, false);
            Assert.IsTrue(result.Success);
        }
    }
}