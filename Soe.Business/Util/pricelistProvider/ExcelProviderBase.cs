using ExcelDataReader;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Data;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public abstract class ExcelProviderBase : IPriceListProvider
    {
        private GenericProvider provider;
        protected abstract string WholesellerName { get; }

        public Common.Util.ActionResult Read(System.IO.Stream stream, string fileName = null)
        {
            int errors = 0;
            int success = 0;
            provider = new GenericProvider(WholesellerName);
            //int providerType = WholesellerName(providerType);
            provider.products = new System.Collections.Hashtable();

            IExcelDataReader excelReader = null;
            try
            {
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
            }
            catch (Exception)
            {
                return new ActionResult(TermCacheManager.Instance.GetText(2062, (int)TermGroup.General, "Filen är ej giltig!"));
            }

            //It is assumed that excel file contains header row.
            DataSet ds = excelReader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });

            var result = Validate(fileName, ds.Tables[0].Columns.Count);
            if (!result.Success)
                return result;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                GenericProduct genProduct = this.ToGenericProduct(row);
                if (genProduct == null)
                    errors++;
                else
                    provider.products.Add(success++, genProduct);
            }

            return result;
        }

        public GenericProvider ToGeneric()
        {
            if (provider.header == null)
                provider.header = new GenericHeader(DateTime.Now);
            return this.provider;
        }

        protected abstract GenericProduct ToGenericProduct(DataRow row);

        protected virtual ActionResult Validate(string fileName, int nrOfColumns)
        {
            var fileType = FileUtil.GetFileType(fileName);
            if (fileType != SoeFileType.Excel)
            {
                return new ActionResult(TermCacheManager.Instance.GetText(7643, (int)TermGroup.General, "Felaktigt filnamn, borde vara:") + "Excel");
            }
            return new ActionResult(true);
        }
    }
}
