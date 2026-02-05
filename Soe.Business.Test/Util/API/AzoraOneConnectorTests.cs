using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.API.AzoraOne;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Soe.Business.Test.Util.API
{
    [TestClass()]
    public class AzoraOneConnectorTests
    {
        public void TrainInterpretor()
        {
            var edm = new EdiManager(null);

            edm.TrainAzoraOneInterpretor(7);
            Assert.IsTrue(true);
        }

        public void UploadSuppliers()
        {
            var edm = new EdiManager(null);
            edm.SyncAllSuppliersWithAzoraOne(37173);
        }

        [TestMethod()]
        public void Test_Fix_Org_Nr()
        {
            string orgNr = "5560640120";
            string fixedOrgNr = AzoraOneHelper.ParseOrgNr(orgNr);
            Assert.AreEqual(fixedOrgNr, "556064-0120");
        }

        [TestMethod()]
        public void Bookkeep_Invoice_With_Discount_Row()
        {
            // Goal is to test that we can bookkeep an invoice with a discount row.
            // Previously we have had problems with this, as AzoraOne only expects cost rows.
            var mockedInvoice = CreateMockedSupplierInvoice(18003.0m, 1928.93m);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2443", -18003.0m),
                CreateAccountingRow("2640", 1928.93m, true),
                CreateAccountingRow("5997", -1786.05m),
                CreateAccountingRow("4010", 17860.12m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_Credit()
        {
            var amount = -1185.0m;
            var vat = -126.98m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("2640", vat, true),
                CreateAccountingRow("5997", 117.58m),
                CreateAccountingRow("4010", -1175.60m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_With_Multiple_Cost_Rows()
        {
            var amount = 9793m;
            var vat = 1049.24m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("2640", vat, true),
                CreateAccountingRow("5997", -971.52m),
                CreateAccountingRow("4010", 2471.40m),
                CreateAccountingRow("4010", 7243.88m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_Without_Vat()
        {
            var amount = 80m;
            var vat = 0m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("4050", amount),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_Cent_Diff_Vat()
        {
            var amount = 25951m;
            var vat = 5190.20m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2443", -amount),
                CreateAccountingRow("2640", 5190m, isVatRow: true),
                CreateAccountingRow("1220", 20761m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_Cent_Diff_Vat2()
        {
            var amount = 5227m;
            var vat = 542.93m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("2640", 543m, isVatRow: true),
                CreateAccountingRow("4012", 4524m),
                CreateAccountingRow("4014", 160m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_Cent_Diff_Vat3()
        {
            var amount = 8644m;
            var vat = 834.5m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("2640", 835m, isVatRow: true),
                CreateAccountingRow("6560", 3338m),
                CreateAccountingRow("6570", 4471m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [Ignore]
        [TestMethod()]
        public void Bookkeep_Invoice_ImportEU_Vat()
        {
            // Currently fails due to the diff between the amount and 2440.
            // Most likely due to stupid rounding issue for currency invoices.
            // Real example from InvoiceId 19661113, 19622169 in soecompv2.
            var amount = 847.75m;
            var vat = 0m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -847.74m),
                CreateAccountingRow("2615", -211.93m, isContractorRow: true),
                CreateAccountingRow("2645", 211.93m, isContractorRow: true),
                CreateAccountingRow("2990", 847.74m),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }
        
        [TestMethod()]
        public void Bookkeep_Invoice_Contractor()
        {
            var amount = 1500m;
            var vat = 0m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("2617", -375m, isContractorRow: true),
                CreateAccountingRow("2647", 375m, isContractorRow: true),
                CreateAccountingRow("4660", amount),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void Bookkeep_Invoice_Multiple_Debt_Accountings()
        {
            // Goal is to test that we can bookkeep an invoice with a discount row.
            // Previously we have had problems with this, as AzoraOne only expects cost rows.
            var amount = 2000m;
            var vat = 0m;
            var mockedInvoice = CreateMockedSupplierInvoice(amount, vat);
            var accountingRows = new List<AccountingRowDTO>()
            {
                CreateAccountingRow("2440", -amount),
                CreateAccountingRow("7420", amount),
                CreateAccountingRow("2219", -amount),
                CreateAccountingRow("1385", amount),
            };

            var result = Bookkeep(mockedInvoice, accountingRows);
            Assert.IsTrue(result.Success);
        }

        [Ignore]
        [TestMethod()]
        public void Create_Supplier()
        {
            var client = new AzoraOneManager("d0053b1e-e011-45e7-8b71-e8a874600627");
            // Scenario when initial supplier sync has gone wrong for some reason. It's worth attempting to fix it.
            var manager = new SupplierManager(null);
            var supplier = manager.GetSuppliers(7,
                loadActor: true,
                loadAccount: false,
                loadContactAddresses: false,
                loadCategories: false,
                loadPaymentInformation: true,
                loadTemplateAttestHead: false,
                supplierIds: new List<int> { 5666 }) //5274-7896 / Avanza
                .ToDistributionDTOs()
                .FirstOrDefault();

            var result = client.SyncSupplier(supplier);
            Assert.IsTrue(result.Success);
        }

        [Ignore]
        [TestMethod()]
        public void SyncCompany()
        {
            var client = new AzoraOneManager("d0053b1e-e011-45e7-8b71-e8a874600627");
            // Scenario when initial supplier sync has gone wrong for some reason. It's worth attempting to fix it.
            var manager = new CompanyManager(null);
            var company = manager.GetCompany(7, loadLicense: true, loadEdiConnection: false, loadActorAndContact: true).ToCompanyDTO();
            var result = client.SaveCompany(company);
            Assert.IsTrue(result.Success);
        }

        private ActionResult Bookkeep(SupplierInvoiceDTO invoice, List<AccountingRowDTO> accountingRows)
        {
            // Hard coded values are Hantverkardemo and a specific document that we can reuse.
            var azoraOneManager = new AzoraOneManager("d0053b1e-e011-45e7-8b71-e8a874600627");
            return azoraOneManager.BookkeepInvoice("978f2530-a059-49ec-863b-f4b76769aef0", invoice, accountingRows);
        }

        private AccountingRowDTO CreateAccountingRow(string accountNr, decimal amount, bool isVatRow = false, bool isContractorRow = false)
        {
            return new AccountingRowDTO()
            {
                Dim1Nr = accountNr,
                Amount = amount,
                AmountCurrency = amount,
                IsVatRow = isVatRow,
                IsContractorVatRow = isContractorRow,
                DebitAmountCurrency = amount >= 0 ? amount : 0,
                DebitAmount = amount >= 0 ? amount : 0,
                CreditAmountCurrency = amount < 0 ? -amount : 0,
                CreditAmount = amount < 0 ? -amount : 0,
                IsCreditRow = amount < 0,
                IsDebitRow = amount >= 0,
            };
        }


        private SupplierInvoiceDTO CreateMockedSupplierInvoice(decimal amount, decimal vatAmount)
        {
            return new SupplierInvoiceDTO()
            {
                ActorId = 10029,
                OriginDescription = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                InvoiceNr = "123",
                OrderNr = 123,
                OCR = "",
                TotalAmount = amount,
                TotalAmountCurrency = amount,
                VatAmount = vatAmount,
                VatAmountCurrency = vatAmount,
            };
        }
    }
}
