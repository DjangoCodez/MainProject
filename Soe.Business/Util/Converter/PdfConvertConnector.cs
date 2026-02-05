using Converter.Shared.PDF;
using System.Collections.Generic;
using System.Windows.Documents;

namespace SoftOne.Soe.Business.Util.Converter
{
    public class PdfConvertConnector
    {
        public static byte[] ConvertHtmlToPdf(string html, bool forFinvoice)
        {
            var options = new PDFHtmlOptions()
            {
                HtmlMediaType = OutputMediaType.Print,
                IsEmbedFonts = true,
            };

            if (forFinvoice)
            {
                options.FitToWidestContentWidth = true;
                options.PageSize = OutputPageSize.A4;
                options.Margin = 15;
            }

            return ConvertPdfClient.ConvertToPdf(GetConvertServiceUrl(), html, options);
        }

        public static byte[] ConvertImageToPdf(byte[] imageData, string fileName, bool isMultiFrame)
        {
            return ConvertPdfClient.ConvertImageToPdf(GetConvertServiceUrl(), imageData, fileName, isMultiFrame);
        }

        public static PdfResponse ExtractPages(byte[] pdf, string fileName, List<int> pageIndices)
        {
            return ConvertPdfClient.ExtractPages(GetConvertServiceUrl(), pdf, fileName, pageIndices);
        }

        private static string GetConvertServiceUrl()
        {
#if DEBUG
            //return "https://localhost:7174/";
            //return "http://localhost:5299/";
            return "https://s1converter.azurewebsites.net/";
#else
            return "https://s1converter.azurewebsites.net/";
#endif

        }
    }
}
