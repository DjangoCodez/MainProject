using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.PaymentIO.BBS;
using SoftOne.Soe.Business.Core.PaymentIO.SEPAV3;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.IO;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class PaymentManagerTests : TestBase
    {
        [TestMethod()]
        public void SepaPain002Import()
        {
            var pio = new PaymentIOManager(GetParameterObject(2458712, 72));

            var fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\pain002\actuel_error_single.xml");

            var result = pio.ImportPain002(fileContent);
            if (result != null)
            {
                fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\pain002\example_pain.002.001.03_part_incl_accept.xml");
                result = pio.ImportPain002(fileContent);
            }

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SepaPain002Import_NordeaV2()
        {
            var pio = new PaymentIOManager(GetParameterObject(2458712, 72));

            var fileContent = File.ReadAllText(@"C:\Temp\betalfiler\\Pain_002_v2_Nordea2.xml");

            var result = pio.ImportPain002(fileContent);

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SepaPain002Import_V3()
        {
            var pio = new PaymentIOManager(GetParameterObject(2458712, 72));

            var fileContent = File.ReadAllText(@"C:\Temp\betalfiler\\Pain_002_v3.xml");

            var result = pio.ImportPain002(fileContent);

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CAMT54Import()
        {
            var pIo = new PaymentIOManager(GetParameterObject(7, 72));

            var fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\Camt054\Camt054D_avalo.xml");

            var result = pIo.ImportCAMTFile("HANDSESS", "2255-5555", 7, Common.Util.ImportPaymentType.SupplierPayment, fileContent);
            
            Assert.IsTrue(result.Success);
        }
        [TestMethod()]
        public void SHBOnboardingImport()
        {
            var sepa = new SEPAV3Manager(GetParameterObject(7, 72));
            var fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\shb_Onboarding.xml");

            var result = sepa.ImportOnboardingFile(fileContent, 7);

            Assert.IsTrue(result.Success);
        }
        [TestMethod()]
        public void CAMT53Import()
        {
            ConfigurationSetupUtil.Init();
            var pIo = new PaymentIOManager(GetParameterObject(7, 72));

            var fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\shb_camt053_extended_SE_incoming.xml");

            var result = pIo.ImportCAMT53("HANDSESS", "2255-5555", 7, fileContent);

            
            Assert.IsTrue(result.Success);
        }
        [TestMethod()]
        public void CamtFIImport()
        {
            ConfigurationSetupUtil.Init();
            var pIo = new PaymentIOManager(GetParameterObject(288, 65)); //SoftOne Finland Oy, Admin
            var fileContent = File.ReadAllText(@"C:\temp\Banker\SEPA_Handelsbanken_202404_SoftOne_Finland.xml");
            var result = pIo.ImportCAMTFile("HANDFIHH", "2255-5555", 288, Common.Util.ImportPaymentType.CustomerPayment, fileContent);
        }
        [TestMethod()]
        public void Camt53Contains()
        {
            ConfigurationSetupUtil.Init();
            var sepa = new SEPAV3Manager(GetParameterObject(7, 72));

            bool hasCustomerPayments;
            bool hasSupplierPayments;

            var fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\shb_camt053_extended_SE_incoming.xml");
            var result = sepa.Camt53Contains(fileContent, out hasCustomerPayments, out hasSupplierPayments);

            fileContent = File.ReadAllText(@"C:\Temp\betalalfiler\shb_camt053_extended_SE_outgoing.xml");
            result = sepa.Camt53Contains(fileContent, out hasCustomerPayments, out hasSupplierPayments);

            Assert.IsTrue(result.Success);
        }


        [TestMethod()]
        public void ImportBBSOCRFile()
        {
            ConfigurationSetupUtil.Init();
            var manager = new BBSPaymentManager(GetParameterObject(7, 72));
            var fileContent = File.OpenRead(@"C:\Temp\Betalfiler\OCR-fil.txt");
            var streamReader = new StreamReader(fileContent);
            var bbsFile = new BBSFile(streamReader);
            var row1 = bbsFile.PaymentRows[0];
            var row2 = bbsFile.PaymentRows[1];
            Assert.IsTrue(row1 != null);
        }

        [TestMethod()]
        public void MatchBBSInvoice()
        {
            ConfigurationSetupUtil.Init();
            var manager = new InvoiceManager(GetParameterObject(7, 72));
            var entities = new CompEntities();
            var invoice = manager.GetCustomerInvoiceAmountExtractNumerics(entities, "123456789101112");
            Assert.IsTrue(invoice.InvoiceNr == "L123456789101112");

            var invoice3 = manager.GetCustomerInvoiceAmountExtractNumerics(entities, "12345");
            Assert.IsTrue(invoice3.InvoiceNr == "L12345");

            var invoice4 = manager.GetCustomerInvoiceAmountExtractNumerics(entities, "12");
            Assert.IsTrue(invoice4.InvoiceNr == "L12");

            var invoice5 = manager.GetCustomerInvoiceAmountExtractNumerics(entities, "12345678910");
            Assert.IsNull(invoice5);
        }
    }
}