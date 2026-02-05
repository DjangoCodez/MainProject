using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public abstract class RowBasedProviderBase : IPriceListProvider
    {
        private GenericProvider provider;
        protected virtual int SkipRows{ get { return 0; } }
        protected virtual Encoding FileEncoding { get { return Constants.ENCODING_LATIN1; } }
        protected virtual bool ContainsHeaderRow { get { return false; } }

        protected abstract string WholesellerName { get; }
        protected DateTime? PriceChangeDate;

        public Common.Util.ActionResult Read(Stream stream, string fileName = null)
        {
            var result = new ActionResult();
            int errors = 0;
            int success = 0;
            provider = new GenericProvider(WholesellerName);
            provider.products = new System.Collections.Hashtable();

            var sr = new StreamReader(stream, this.FileEncoding);

            //Skip rows if specified
            for (int i = 0; i < SkipRows; i++)
            {
                sr.ReadLine();
            }

            if (ContainsHeaderRow)
            {
                var header = sr.ReadLine();
                if(header != null)
                    this.provider.header = this.ToGenericHeader(header);
            }
            
            while (!sr.EndOfStream)
            {
                try
                {
                    string row = sr.ReadLine();
                    if (string.IsNullOrEmpty(row)) continue;
                    GenericProduct genProduct = this.ToGenericProduct(row);
                    if (genProduct == null)
                        errors++;
                    else
                        provider.products.Add(success++, genProduct);
                }
                catch(ActionFailedException aex)
                {
                    return new ActionResult(aex);
                }
                catch (Exception)
                {
                    // Move on to next
                    errors++;
                }
            }

            return result;
        }

        protected virtual GenericHeader ToGenericHeader(string header) 
        {
            // Override to change behavior
            return new GenericHeader(DateTime.Now);
        }

        public GenericProvider ToGeneric()
        {
            if(provider.header == null)
                provider.header = new GenericHeader(PriceChangeDate ?? DateTime.Now);
            return this.provider;
        }

        /// <summary>
        /// Must be overridden in order to return an generic product, no need for try catch since it will iterate to next anyway.
        /// </summary>
        /// <param name="line">A single line fetched from pricelist, not empty.</param>
        /// <returns>Return a generic product.</returns>
        protected abstract GenericProduct ToGenericProduct(string line);


        #region HelperMethods

  
        protected decimal ToDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return Convert.ToDecimal(value.Replace('.', ','));
        }

        #endregion
    }
}
