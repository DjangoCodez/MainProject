using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodeInvoiceProduct
    {
        public static TimeCodeInvoiceProduct Create(TimeCode timeCode, InvoiceProduct invoiceProduct, decimal factor)
        {
            if (timeCode == null || invoiceProduct == null)
                return null;

            var timeCodeInvoiceProduct = new TimeCodeInvoiceProduct()
            {
                TimeCode = timeCode,
                InvoiceProduct = invoiceProduct,
            };
            timeCodeInvoiceProduct.SetFactor(factor);
            return timeCodeInvoiceProduct;
        }

        public void Update(InvoiceProduct invoiceProduct, decimal factor)
        {
            this.InvoiceProduct = invoiceProduct;
            this.SetFactor(factor);
        }

        private void SetFactor(decimal factor)
        {
            this.Factor = Decimal.Round(factor, 2, MidpointRounding.AwayFromZero);
        }
    }

    public static partial class EntityExtensions
    {
        public static TimeCodeInvoiceProductDTO ToDTO(this TimeCodeInvoiceProduct e)
        {
            if (e == null)
                return null;

            return new TimeCodeInvoiceProductDTO
            {
                TimeCodeInvoiceProductId = e.TimeCodeInvoiceProductId,
                TimeCodeId = e.TimeCodeId,
                InvoiceProductId = e.ProductId,
                Factor = e.Factor,
                invoiceProductPrice = e.InvoiceProduct != null ? e.InvoiceProduct.PurchasePrice : 0
            };
        }

        public static List<TimeCodeInvoiceProductDTO> ToDTOs(this IEnumerable<TimeCodeInvoiceProduct> l)
        {
            List<TimeCodeInvoiceProductDTO> dtos = new List<TimeCodeInvoiceProductDTO>();
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
