using SoftOne.Soe.Common.DTO.CustomerInvoice;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public abstract class RowBasedSupplierAgreementBase : ISupplierAgreementWithNetPrices
    {
        private GenericProvider provider;
        protected int SkipRows { get; set; } = 0;
        protected bool ErrorOnNoRows { get; set; } = false;
        protected virtual Encoding FileEncoding { get { return Constants.ENCODING_LATIN1; } }

        protected abstract string WholesellerName { get; }
        public abstract SoeSupplierAgreementProvider Provider { get; }

        public virtual bool HasNetPrice { get { return false; } }
        public virtual SoeWholeseller SysWholeSeller { get { return SoeWholeseller.Unknown; }}
        public virtual List<WholsellerNetPriceRowDTO> ToNetPrices() { return new List<WholsellerNetPriceRowDTO>(); }

        public Common.Util.ActionResult Read(System.IO.Stream stream)
        {
            var result = new ActionResult();
            int errors = 0;
            int success = 0;
            provider = new GenericProvider { supplierAgreements = new List<GenericSupplierAgreement>() };
            
            var sr = new StreamReader(stream, this.FileEncoding);

            //Skip rows if specified
            for (int i = 0; i < SkipRows; i++)
            {
                sr.ReadLine();
            }

            while (!sr.EndOfStream)
            {
                try
                {
                    string row = sr.ReadLine();
                    if (string.IsNullOrEmpty(row)) continue;
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
                catch (Exception)
                {
                    // Move on to next
                    errors++;
                }
            }

            result.IntegerValue = success;
            result.IntegerValue2 = errors;
            
            // If more successes then errors, then mark as success
            if (ErrorOnNoRows)
                result.Success = success > 0;
            else
                result.Success = success > errors;

            return result;
        }

        public GenericProvider ToGeneric()
        {
            return this.provider;
        }


        /// <summary>
        /// Must be overridden in order to return an generic supplier agreement, no need for try catch since it will iterate to next anyway.
        /// </summary>
        /// <param name="line">A single line fetched from pricelist, not empty.</param>
        /// <returns>Return a generic agreement.</returns>
        protected abstract (bool success,GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string line);

        #region HelperMethods

        protected decimal ToDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return Convert.ToDecimal(value.Replace('.', ','));
        }

        protected decimal ToDecimalInvariant(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return Convert.ToDecimal(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
