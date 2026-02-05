using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SoftOne.Soe.Business.Util.Converter.Tests
{
    [TestClass()]
    public class PdfConvertConnectorTests
    {
        [TestMethod()]
        public void ConvertToPdfTest()
        {
            //var html = "<html><head></head><body><h1>Test</h1></body></html>";
            var html = File.ReadAllText(@"c:\temp\finvoice\html\finvoice_20222300.html");
            var result = PdfConvertConnector.ConvertHtmlToPdf(html, true);
            File.WriteAllBytes(@"C:\temp\finvoice\pdf\finvoice_20222300_aspose_api_12.pdf", result);
        }
    }
}