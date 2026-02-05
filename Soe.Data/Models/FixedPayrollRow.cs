using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class FixedPayrollRow : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region FixedPayrollRow

        public static FixedPayrollRowDTO ToDTO(this FixedPayrollRow e, bool includeProduct)
        {
            if (e == null)
                return null;

            #region Try load

            if (!e.IsAdded() && includeProduct && !e.PayrollProductReference.IsLoaded)
            {
                e.PayrollProductReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("FixedPayrollRow.cs e.PayrollProductReference");
            }

            #endregion

            return new FixedPayrollRowDTO()
            {
                FixedPayrollRowId = e.FixedPayrollRowId,
                ActorCompanyId = e.ActorCompanyId,
                ProductId = e.ProductId,
                EmployeeId = e.EmployeeId,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                UnitPrice = e.UnitPrice,
                Quantity = e.Quantity,
                Amount = e.Amount,
                VatAmount = e.VatAmount,
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
                Distribute = e.Distribute,
                ProductNr = e.PayrollProduct?.Number ?? string.Empty,
                ProductName = e.PayrollProduct?.Name ?? string.Empty,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
            };
        }

        public static IEnumerable<FixedPayrollRowDTO> ToDTOs(this IEnumerable<FixedPayrollRow> l, bool includeProduct)
        {
            var dtos = new List<FixedPayrollRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeProduct));
                }
            }
            return dtos;
        }

        #endregion
    }
}
