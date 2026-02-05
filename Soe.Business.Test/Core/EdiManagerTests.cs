using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Edi.Common.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.AzoraOne;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class EdiManagerTests : TestBase
    {
        [TestMethod()]
        public void GetPngPagesFromImageTest()
        {
            EdiManager em = new EdiManager(null);
            byte[] image = File.ReadAllBytes(@"e:\temp\25.tif");

            //Check pages in file
            byte[] tiffImage = image;
            MemoryStream tiffMemStream = new MemoryStream(tiffImage)
            {
                Position = 0
            };
            Dictionary<int, byte[]> pages = em.GetPngPagesFromImage(tiffMemStream);
            if (pages != null)
                pages = em.GetPngPagesFromImage2(tiffMemStream);
            Assert.IsTrue(pages != null);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        [TestMethod()]
        public void ImportEdiXMLMessage()
        {
            ConfigurationSetupUtil.Init();

            var actorCompanyId = 7;
            var ediManager = new EdiManager(GetParameterObject(actorCompanyId) );

            var sysEdiMessageHeadDTO = new SysEdiMessageHeadDTO();
            sysEdiMessageHeadDTO.SysEdiMessageHeadGuid = Guid.NewGuid();
            sysEdiMessageHeadDTO.XDocument = File.ReadAllText(@"C:\Temp\edi\ahlsell_ordersp_cancelrow.xml");
            sysEdiMessageHeadDTO.SysWholesellerId = 2;

            var result = ediManager.AddEdiEntryFromSysEdiMessageHeadDTO(actorCompanyId, sysEdiMessageHeadDTO);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GeneratePDF()
        {
            ConfigurationSetupUtil.Init();

            var actorCompanyId = 7;
            var dataManager = new ReportDataManager(GetParameterObject(actorCompanyId));
            var result = dataManager.GenerateReportForEdi(new List<int>() { 2563 }, actorCompanyId);

            Assert.IsTrue(result.Success);
        }


        [TestMethod()]
        public void GenerateFinvoiceHTML()
        {
            ConfigurationSetupUtil.Init();

            var actorCompanyId = 288;
            var ediManager = new EdiManager(GetParameterObject(actorCompanyId));
            var edientry = ediManager.GetEdiEntry(3036, actorCompanyId);

            var htmlData = SoftOne.Soe.Business.Util.Finvoice.FInvoiceFileGen.GetHTML(edientry);
            if (!string.IsNullOrEmpty(htmlData))
            {
                File.WriteAllText(@"C:\Temp\finvoice\html\finvoice_" + edientry.InvoiceNr + ".html", htmlData);
            }

            Assert.IsTrue(!string.IsNullOrEmpty(htmlData));
        }

        [TestMethod()]
        public void SyncSuppliersWithAzoraOne()
        {
            ConfigurationSetupUtil.Init();
            var actorCompanyId = 7;
            var ediManager = new EdiManager(GetParameterObject(actorCompanyId));
            var result = ediManager.SyncAllSuppliersWithAzoraOne(actorCompanyId);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public async Task ImportFinvoice()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 288; //SoftOne Finland Oy
            string finvoiceFilePath = @"C:\Temps\Finvoices\Finvoice 20222368.xml";
            string finvoiceFileName = Path.GetFileName(finvoiceFilePath);

            var ediManager = new EdiManager(GetParameterObject(actorCompanyId));
            var result = await ediManager.AddFinvoiceFromFileImportAsync(finvoiceFilePath, finvoiceFileName, actorCompanyId, null, false).ConfigureAwait(false);

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ImportFinvoiceAttachement()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 288; //SoftOne Finland Oy
            string finvoiceAttachmentFilePath = @"C:\Temps\Finvoices\Finvoice 20222368_attachments.xml";
            string fileName = Path.GetFileName(finvoiceAttachmentFilePath);

            var ediManager = new EdiManager(GetParameterObject(actorCompanyId));
            using (var stream = new MemoryStream(File.ReadAllBytes(finvoiceAttachmentFilePath)))
            {
                var result = ediManager.AddFinvoiceAttachment(fileName, actorCompanyId, stream);
                Assert.IsTrue(result.Success);
            }
        }

        [TestMethod()]
        public void TestSaveCompany()
        {
            ConfigurationSetupUtil.Init();
            var companyManager = new CompanyManager(GetParameterObject(7));
            var company = companyManager.GetCompany(347).ToCompanyDTO();
            var aoManager = new AzoraOneManager("9967016E-9B57-4FCF-8BFD-3CA545AA6F8F");

            company.VatNr = "AOZBDASD";

            aoManager.SaveCompany(company);
        }
    }
}