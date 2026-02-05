using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class PayrollStartValueHead : ICreatedModified, IState
    {
    }

    public partial class PayrollStartValueRow : IPayrollType, ICreatedModified, IState
    {
        public string Appellation { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region PayrollStartValueHead

        public static PayrollStartValueHeadDTO ToDTO(this PayrollStartValueHead e, bool includeRows = false, bool includePayrollProduct = false, bool includeTransaction = false)
        {
            if (e == null)
                return null;

            PayrollStartValueHeadDTO dto = new PayrollStartValueHeadDTO()
            {
                PayrollStartValueHeadId = e.PayrollStartValueHeadId,
                ActorCompanyId = e.ActorCompanyId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                ImportedFrom = e.ImportedFrom,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };

            if (includeRows || includeTransaction)
                dto.Rows = e.PayrollStartValueRow?.Where(p => p.State == (int)SoeEntityState.Active).OrderBy(r => r.Date).ToDTOs(includePayrollProduct, includeTransaction) ?? new List<PayrollStartValueRowDTO>();               

            return dto;
        }

        public static IEnumerable<PayrollStartValueHeadDTO> ToDTOs(this IEnumerable<PayrollStartValueHead> l, bool includeRows = false, bool includePayrollProduct = false, bool includeTransaction = false)
        {
            var dtos = new List<PayrollStartValueHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, includePayrollProduct, includeTransaction));
                }
            }
            return dtos;
        }

        #endregion

        #region PayrollStartValueRow

        public static PayrollStartValueRowDTO ToDTO(this PayrollStartValueRow e, bool includePayrollProduct, bool includeTransaction)
        {
            if (e == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = includeTransaction ? e.TimePayrollTransaction?.FirstOrDefault(p => p.State == (int)SoeEntityState.Active) : null;

            return new PayrollStartValueRowDTO()
            {
                PayrollStartValueRowId = e.PayrollStartValueRowId,
                PayrollStartValueHeadId = e.PayrollStartValueHeadId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                ProductId = e.ProductId,
                ProductNr = e.PayrollProduct?.Number ?? string.Empty,
                ProductName = e.PayrollProduct?.Name ?? string.Empty,
                SysPayrollStartValueId = e.SysPayrollStartValueId,
                Appellation = e.Appellation,
                Date = e.Date,
                Quantity = e.Quantity,
                Amount = e.Amount,
                ScheduleTimeMinutes = e.ScheduleTimeMinutes,
                AbsenceTimeMinutes = e.AbsenceTimeMinutes,
                SysPayrollTypeLevel1 = (TermGroup_SysPayrollType)(e.SysPayrollTypeLevel1 ?? 0),
                SysPayrollTypeLevel2 = (TermGroup_SysPayrollType)(e.SysPayrollTypeLevel2 ?? 0),
                SysPayrollTypeLevel3 = (TermGroup_SysPayrollType)(e.SysPayrollTypeLevel3 ?? 0),
                SysPayrollTypeLevel4 = (TermGroup_SysPayrollType)(e.SysPayrollTypeLevel4 ?? 0),
                State = (SoeEntityState)e.State,

                //TimePayrollTransaction
                TimePayrollTransactionId = timePayrollTransaction?.TimePayrollTransactionId ?? 0,
                TransactionProductId = timePayrollTransaction?.ProductId ?? 0,
                TransactionProductNr = (timePayrollTransaction?.CachedProduct?.Number ?? timePayrollTransaction?.PayrollProduct?.Number).NullToEmpty(),
                TransactionProductName = (timePayrollTransaction?.CachedProduct?.Name ?? timePayrollTransaction?.PayrollProduct?.Name).NullToEmpty(),
                TransactionAmount = timePayrollTransaction?.Amount,
                TransactionQuantity = timePayrollTransaction?.Quantity ?? 0,
                TransactionUnitPrice = timePayrollTransaction?.UnitPrice ?? 0,
                TransactionDate = ((timePayrollTransaction?.CachedTimeBlockDate?.Date) ?? timePayrollTransaction?.TimeBlockDate?.Date) ?? (DateTime?)null,
                TransactionComment = timePayrollTransaction?.Comment ?? string.Empty,
                TransactionLevel1 = (TermGroup_SysPayrollType)(timePayrollTransaction?.SysPayrollTypeLevel1 ?? 0),
                TransactionLevel2 = (TermGroup_SysPayrollType)(timePayrollTransaction?.SysPayrollTypeLevel2 ?? 0),
                TransactionLevel3 = (TermGroup_SysPayrollType)(timePayrollTransaction?.SysPayrollTypeLevel3 ?? 0),
                TransactionLevel4 = (TermGroup_SysPayrollType)(timePayrollTransaction?.SysPayrollTypeLevel4 ?? 0),
            };
        }

        public static List<PayrollStartValueRowDTO> ToDTOs(this IEnumerable<PayrollStartValueRow> l, bool includePayrollProduct = false, bool includeTransaction = false)
        {
            var dtos = new List<PayrollStartValueRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePayrollProduct: includePayrollProduct, includeTransaction: includeTransaction));
                }
            }
            return dtos;
        }

        public static List<PayrollStartValueRow> Filter(this List<PayrollStartValueRow> l, int sysPayrollTypeLevel3)
        {
            return l?.Where(p => p.SysPayrollTypeLevel3 == sysPayrollTypeLevel3).ToList() ?? new List<PayrollStartValueRow>();
        }

        public static bool HasTransactions(this List<PayrollStartValueRow> l)
        {
            return l?.Any(i => i.TimePayrollTransaction != null && i.TimePayrollTransaction.Any(tpt => tpt.State == (int)SoeEntityState.Active)) ?? false;
        }

        #endregion
    }
}
