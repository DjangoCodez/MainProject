using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodePayrollProduct
    {
        public static TimeCodePayrollProduct Create(TimeCode timeCode, PayrollProduct payrollProduct, decimal factor)
        {
            if (timeCode == null || payrollProduct == null)
                return null;

            var timeCodePayrollProduct = new TimeCodePayrollProduct()
            {  
                TimeCode = timeCode,
                PayrollProduct = payrollProduct,
            };
            timeCodePayrollProduct.SetFactor(factor);
            return timeCodePayrollProduct;
        }

        public void Update(PayrollProduct payrollProduct, decimal factor)
        {
            this.PayrollProduct = payrollProduct;
            this.SetFactor(factor);
        }

        private void SetFactor(decimal factor)
        {
            this.Factor = Decimal.Round(factor, 5, MidpointRounding.AwayFromZero);
        }
    }

    public static partial class EntityExtensions
    {
        public static TimeCodePayrollProductDTO ToDTO(this TimeCodePayrollProduct e)
        {
            if (e == null)
                return null;

            return new TimeCodePayrollProductDTO()
            {
                TimeCodePayrollProductId = e.TimeCodePayrollProductId,
                TimeCodeId = e.TimeCodeId,
                PayrollProductId = e.ProductId,
                Factor = e.Factor
            };
        }

        public static List<TimeCodePayrollProductDTO> ToDTOs(this IEnumerable<TimeCodePayrollProduct> l)
        {
            List<TimeCodePayrollProductDTO> dtos = new List<TimeCodePayrollProductDTO>();
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
