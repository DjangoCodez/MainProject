using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class Intrastat : ExportFilesBase
    {
        #region Ctor

        public Intrastat(ParameterObject parameterObject, CreateReportResult ReportResult) : base(parameterObject, ReportResult) { }

        #endregion

        #region Public methods

        public string CreateFile(int actorCompanyId)
        {
            #region Init

            if (ReportResult == null)
                return null;

            var cm = new CompanyManager(parameterObject);
            var sb = new StringBuilder();

            #endregion

            #region Prereq

            var company = cm.GetCompany(actorCompanyId);
            if (company == null)
                return null;

            #endregion

            #region Create file

            string url = String.Empty;
            if(es.Special == "export")
            {
                byte[] bytes = CreateExportFile();
                if (bytes == null || bytes.Length == 0)
                    return String.Empty;

                string filename = GetText(26, (int)TermGroup.ReportExportFileType, "Intrastat export") + ".xlsx";
                url = GeneralManager.GetUrlForDownload(bytes, filename);
            }
            else
            {
                byte[] bytes = CreateImportFile();
                if (bytes == null || bytes.Length == 0)
                    return String.Empty;

                string filename = GetText(27, (int)TermGroup.ReportExportFileType, "Intrastat import") + ".xlsx";
                url = GeneralManager.GetUrlForDownload(bytes, filename);
            }

            #endregion

            return url;
        }

        public byte[] CreateImportFile()
        {
            #region Column headers

            List<string> headerNames = new List<string>();
            headerNames.Add("Varukod");
            headerNames.Add("Landkod");
            headerNames.Add("Transaktionstyp");
            headerNames.Add("Nettovikt");
            headerNames.Add("Annan kvantitet");
            headerNames.Add("Fakturavärde SEK");

            ExcelHelper helper = new ExcelHelper(GetText(27, (int)TermGroup.ReportExportFileType, "Intrastat import"), headerNames);

            #endregion

            #region Content

            var transactions = CommodityCodeManager.GetIntrastatTransactionsForExport(IntrastatReportingType.Import, es.DateFrom, es.DateTo, base.ActorCompanyId);

            int rowNr = 2;
            foreach(var transaction in transactions)
            {
                List<object> row = new List<object>();
                row.Add(transaction.IntrastatCode); 
                row.Add(transaction.Country); 
                row.Add(transaction.IntrastatTransactionType); 
                row.Add(transaction.NetWeight); 
                row.Add(transaction.OtherQuantity);
                row.Add(transaction.Amount);

                helper.AddDataRow(rowNr, row);
                rowNr++;
            }

            #endregion

            helper.FormatRowAsHeader(headerNames.Count);
            helper.AutoFitColumns();

            // Parse to bytes
            return helper.GetData();
        }

        public byte[] CreateExportFile()
        {
            #region Column headers

            List<string> headerNames = new List<string>();
            headerNames.Add("Varukod");
            headerNames.Add("Landkod");
            headerNames.Add("Transaktionstyp");
            headerNames.Add("Nettovikt");
            headerNames.Add("Annan kvantitet");
            headerNames.Add("Fakturavärde SEK");
            headerNames.Add("Partner-ID/momsregistreringsnummer");
            headerNames.Add("Ursprungsland");

            ExcelHelper helper = new ExcelHelper(GetText(26, (int)TermGroup.ReportExportFileType, "Intrastat export"), headerNames);

            #endregion

            #region Content

            var transactions = CommodityCodeManager.GetIntrastatTransactionsForExport(IntrastatReportingType.Export, es.DateFrom, es.DateTo, base.ActorCompanyId);

            int rowNr = 2;
            foreach (var transaction in transactions)
            {
                List<object> row = new List<object>();
                row.Add(transaction.IntrastatCode);
                row.Add(transaction.Country);
                row.Add(transaction.IntrastatTransactionType);
                row.Add(transaction.NetWeight);
                row.Add(transaction.OtherQuantity);
                row.Add(transaction.Amount);
                row.Add(transaction.VatNr);
                row.Add(transaction.OriginCountry);

                helper.AddDataRow(rowNr, row);
                rowNr++;
            }

            #endregion

            helper.FormatRowAsHeader(headerNames.Count);
            helper.AutoFitColumns();

            // Parse to bytes
            return helper.GetData();
        }

        #endregion
    }
}



