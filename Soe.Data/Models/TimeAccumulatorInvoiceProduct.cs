using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeAccumulatorInvoiceProduct
    {
        public static TimeAccumulatorInvoiceProduct Create(TimeAccumulator timeAccumulator, InvoiceProduct invoiceProduct, decimal factor)
        {
            if (timeAccumulator == null || invoiceProduct == null)
                return null;

            var timeAccumulatorInvoiceProduct = new TimeAccumulatorInvoiceProduct()
            {
                TimeAccumulator = timeAccumulator,
                InvoiceProduct = invoiceProduct,
            };
            timeAccumulatorInvoiceProduct.SetFactor(factor);
            return timeAccumulatorInvoiceProduct;
        }

        public void Update(InvoiceProduct invoiceProduct, decimal factor)
        {
            this.InvoiceProduct = invoiceProduct;
            this.SetFactor(factor);
        }

        private void SetFactor(decimal factor)
        {
            this.Factor = Decimal.Round(factor, 5, MidpointRounding.AwayFromZero);
        }
    }

    public static partial class EntityExtensions
    {
        public static TimeAccumulatorInvoiceProductDTO ToDTO(this TimeAccumulatorInvoiceProduct e)
        {
            if (e == null)
                return null;

            return new TimeAccumulatorInvoiceProductDTO()
            {
                InvoiceProductId = e.InvoiceProductId,
                Factor = e.Factor
            };
        }

        public static List<TimeAccumulatorInvoiceProductDTO> ToDTOs(this IEnumerable<TimeAccumulatorInvoiceProduct> l)
        {
            var dtos = new List<TimeAccumulatorInvoiceProductDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
    }
}
