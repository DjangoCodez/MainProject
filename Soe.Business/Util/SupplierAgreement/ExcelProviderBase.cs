using ExcelDataReader;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Data;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{


    public abstract class ExcelProviderBase : ISupplierAgreement
    {
        private GenericProvider provider;
        protected abstract string WholesellerName { get; }
        public abstract SoeSupplierAgreementProvider Provider { get; }

        public Common.Util.ActionResult Read(System.IO.Stream stream)
        {
            var result = new ActionResult();
            int errors = 0;
            int success = 0;
            provider = new GenericProvider { supplierAgreements = new List<GenericSupplierAgreement>() };

            IExcelDataReader excelReader = null;

            try
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            catch
            {
                try
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                catch
                {
                    excelReader = ExcelReaderFactory.CreateReader(stream);
                }
            }            

            //It is assumed that excel file contains header row.
            DataSet ds = excelReader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });
            
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var parseResult = this.ToGenericSupplierAgreement(row);
                if (!parseResult.success)
                {
                    errors++;
                    continue;
                }

                if (parseResult.agreement != null)
                {
                    provider.supplierAgreements.Add(parseResult.agreement);
                    success++;
                }
            }

            return result;
        }

        public GenericProvider ToGeneric()
        {
            return this.provider;
        }

        protected abstract (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(DataRow row);
    }
}
