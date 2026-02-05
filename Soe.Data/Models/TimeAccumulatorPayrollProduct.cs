using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeAccumulatorPayrollProduct
    {
        public static TimeAccumulatorPayrollProduct Create(TimeAccumulator timeAccumulator, PayrollProduct payrollProduct, decimal factor)
        {
            if (timeAccumulator == null || payrollProduct == null)
                return null;

            var timeAccumulatorPayrollProduct = new TimeAccumulatorPayrollProduct()
            {
                TimeAccumulator = timeAccumulator,
                PayrollProduct = payrollProduct,
            };
            timeAccumulatorPayrollProduct.SetFactor(factor);
            return timeAccumulatorPayrollProduct;
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
        public static TimeAccumulatorPayrollProductDTO ToDTO(this TimeAccumulatorPayrollProduct e)
        {
            if (e == null)
                return null;

            return new TimeAccumulatorPayrollProductDTO()
            {
                PayrollProductId = e.PayrollProductId,
                Factor = e.Factor
            };
        }

        public static List<TimeAccumulatorPayrollProductDTO> ToDTOs(this IEnumerable<TimeAccumulatorPayrollProduct> l)
        {
            var dtos = new List<TimeAccumulatorPayrollProductDTO>();
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
